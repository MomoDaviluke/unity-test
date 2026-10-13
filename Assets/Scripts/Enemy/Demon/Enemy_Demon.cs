using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Demon : Enemy
{
    // ==================== 状态机声明 ====================
    // Demon拥有9个状态：待机、行走、受击、劈砍、踩踏、施法、喷火、瞬移、死亡
    #region States
    public DemonIdleState idleState { get; private set; }
    public DemonWalkState walkState { get; private set; }
    public DemonTakeHitState takeHitState { get; private set; }
    public DemonCleaveState cleaveState { get; private set; }
    public DemonSmashState smashState { get; private set; }
    public DemonCastSpellState castSpellState { get; private set; }
    public DemonFireBreathState fireBreathState { get; private set; }
    public DemonTeleportState teleportState { get; private set; }
    public DemonDeadState deadState { get; private set; }
    #endregion

    // ==================== Boss基础设置 ====================
    [Header("Boss设置")]
    public bool bossFightBegun;
    public int formLevel = 1;

    // ==================== 移动参数 ====================
    [Header("移动设置")]
    public float chaseSpeed = 4f;
    public float stopDistance = 3f;

    // ==================== 技能冷却管理 ====================
    [Header("技能冷却")]
    public float skillCooldown = 0.3f;
    public float lastTimeSkillUsed;

    // ==================== 瞬移参数 ====================
    [Header("瞬移设置")]
    public float teleportCooldown = 5f;
    public float lastTimeTeleported;
    public float teleportRange = 8f;
    [SerializeField] private BoxCollider2D arena;

    // ==================== 魔法与喷火技能 ====================
    [Header("魔法技能")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private GameObject fireBreathPrefab;
    public Vector2 spellSpawnOffset = new Vector2(1.5f, 0.5f);
    public float spellCastCooldown = 4f;
    public float lastTimeSpellCast;

    // ==================== 范围技能参数 ====================
    [Header("踩踏技能")]
    public float smashRadius = 5f;
    public float smashDamage = 30f;

    [Header("打击技能")]
    public float cleaveDamage = 25f;
    public float cleaveRange = 3f;

    // ==================== 特效预制体 ====================
    [Header("特效")]
    public GameObject teleportEffect;
    public GameObject castSpellEffect;
    public GameObject smashGroundEffect;

    private GameObject currentTeleportEffectInstance;
    private GameObject currentCastSpellEffectInstance;
    private GameObject currentSmashGroundEffectInstance;

    // ==================== 生命周期 ====================
    // Awake中创建所有状态实例
    // Start时初始化面朝方向、隐藏反击图片、订阅受击事件

    protected override void Awake()
    {
        base.Awake();

        idleState = new DemonIdleState(this, stateMachine, "Idle", this);
        walkState = new DemonWalkState(this, stateMachine, "Move", this);
        takeHitState = new DemonTakeHitState(this, stateMachine, "TakeHit", this);
        cleaveState = new DemonCleaveState(this, stateMachine, "Attack", this);
        smashState = new DemonSmashState(this, stateMachine, "smash", this);
        castSpellState = new DemonCastSpellState(this, stateMachine, "CastSpell", this);
        fireBreathState = new DemonFireBreathState(this, stateMachine, "FireBreath", this);
        teleportState = new DemonTeleportState(this, stateMachine, "Teleport", this);
        deadState = new DemonDeadState(this, stateMachine, "Die", this);
    }

    protected override void Start()
    {
        base.Start();
        
        counterImage?.SetActive(false);

        SetupDefaultFacingDir(1);
        stateMachine.Initialize(idleState);
        
        // 订阅受击回调：非技能释放状态下受击会切换到受击状态
        stats.GetComponent<EnemyStats>().onTakeDamage += OnTakeDamage;
    }
    
    // ==================== 受击处理 ====================
    // 受击回调：如果当前不在受击、死亡或技能释放状态，则被打断进入受击状态
    private void OnTakeDamage()
    {
        if (stateMachine.currentState != takeHitState && 
            stateMachine.currentState != deadState &&
            stateMachine.currentState != cleaveState &&
            stateMachine.currentState != smashState &&
            stateMachine.currentState != castSpellState)
        {
            stateMachine.ChangeState(takeHitState);
        }
    }

    // ==================== 死亡处理 ====================
    // 显示所有背景对象并切换到死亡状态
    public override void Die()
    {
        base.Die();
        ShowAllBgBgObjects();
        Debug.Log("[Boss] 已被击杀，通关");
        stateMachine.ChangeState(deadState);
    }

    // ==================== 背景对象管理 ====================
    // Boss死亡后显示所有名为"Bg_BG"的背景对象
    private void ShowAllBgBgObjects()
    {
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allGameObjects)
        {
            if (obj == null)
                continue;

            if (!obj.scene.IsValid())
                continue;

            if (obj.name != "Bg_BG")
                continue;

            obj.SetActive(true);
        }
    }

    // ==================== 技能选择系统 ====================
    // 随机选择下一个技能（各20%概率）：踩踏、劈砍、瞬移、喷火、施法
    public void SelectNextSkill()
    {
        lastTimeSkillUsed = Time.time;

        float random = Random.Range(0, 100);
        Debug.Log("[SelectNextSkill] random: " + random);

        // 20% 踩踏
        if (random < 20)
        {
            Debug.Log("[SelectNextSkill] Smash");
            stateMachine.ChangeState(smashState);
        }
        // 20% 劈砍
        else if (random < 40)
        {
            Debug.Log("[SelectNextSkill] Cleave");
            stateMachine.ChangeState(cleaveState);
        }
        // 20% 瞬移
        else if (random < 60)
        {
            Debug.Log("[SelectNextSkill] Teleport");
            FindSafeTeleportPosition();
            Teleport();
            stateMachine.ChangeState(walkState);
        }
        // 20% 喷火
        else if (random < 80)
        {
            Debug.Log("[SelectNextSkill] FireBreath");
            stateMachine.ChangeState(fireBreathState);
        }
        // 20% 魔法
        else
        {
            Debug.Log("[SelectNextSkill] CastSpell");
            stateMachine.ChangeState(castSpellState);
        }
    }

    // ==================== 冷却判定 ====================
    public bool CanCastSpell()
    {
        return Time.time >= lastTimeSpellCast + spellCastCooldown;
    }

    public bool CanTeleport()
    {
        return Time.time >= lastTimeTeleported + teleportCooldown;
    }

    // ==================== 施法技能 ====================
    // 分批发射多颗跟踪子弹，带有冷却检查
    public void CastSpell()
    {
        if (lastTimeSpellCast > 0 && Time.time < lastTimeSpellCast + spellCastCooldown)
        {
            Debug.Log("[CastSpell] Cooldown, lastTime: " + lastTimeSpellCast + ", cooldown: " + spellCastCooldown);
            return;
        }

        lastTimeSpellCast = Time.time;
        
        Player player = PlayerManager.instance.player;
        if (player == null) return;

        Debug.Log("[CastSpell] spellPrefab: " + spellPrefab);

        PlayCastSpellEffect(transform.position);

        // 分批发射子弹
        StartCoroutine(SpawnBulletsInWaves());
    }

    // ==================== 魔法子弹协程 ====================
    // 分波次发射跟踪子弹，每波之间有延迟
    private System.Collections.IEnumerator SpawnBulletsInWaves()
    {
        int totalBullets = 4;
        int bulletsPerWave = 1;
        float delayBetweenWaves = 0.3f;
        float delayBetweenBullets = 0.1f;

        int waveCount = (totalBullets + bulletsPerWave - 1) / bulletsPerWave;

        for (int wave = 0; wave < waveCount; wave++)
        {
            for (int i = 0; i < bulletsPerWave && (wave * bulletsPerWave + i) < totalBullets; i++)
            {
                if (spellPrefab != null)
                {
                    float offsetX = Random.Range(-0.3f, 0.3f);
                    float offsetY = Random.Range(-0.2f, 0.2f);
                    
                    // 根据朝向计算生成位置
                    int dir = facingDir > 0 ? 1 : -1;
                    Vector3 spawnPos = new Vector3(
                        transform.position.x + spellSpawnOffset.x * dir + offsetX,
                        transform.position.y + spellSpawnOffset.y + offsetY,
                        0
                    );
                    
                    GameObject newSpell = Instantiate(spellPrefab, spawnPos, Quaternion.identity);
                    DemonSpell_Controller controller = newSpell.GetComponent<DemonSpell_Controller>();
                    if (controller != null)
                    {
                        controller.SetupSpell(stats, true);
                    }
                }
                yield return new WaitForSeconds(delayBetweenBullets);
            }
            
            yield return new WaitForSeconds(delayBetweenWaves);
        }
    }

    // ==================== 喷火技能 ====================
    public void CastFireBreath()
    {
        lastTimeSpellCast = Time.time;

        if (fireBreathPrefab != null)
        {
            GameObject fireBreath = Instantiate(fireBreathPrefab, transform.position, Quaternion.identity);
            fireBreath.GetComponent<DemonFireBreath_Controller>()?.SetupSpell(stats);
        }
    }

    // ==================== 瞬移技能 ====================
    // 瞬移到玩家身后（相对位置偏移3个单位）
    public void Teleport()
    {
        lastTimeTeleported = Time.time;

        Player player = PlayerManager.instance.player;
        if (player == null) return;

        // 生成瞬移特效
        PlayTeleportEffect(transform.position);

        // 瞬移到玩家身后
        float teleportX = player.transform.position.x + (player.transform.position.x > transform.position.x ? -3f : 3f);
        transform.position = new Vector3(teleportX, player.transform.position.y, 0);

        // 再次生成特效
        PlayTeleportEffect(transform.position);
    }

    // ==================== 安全位置瞬移 ====================
    // 在竞技场范围内随机选择安全位置瞬移
    public void FindSafeTeleportPosition()
    {
        if (arena == null)
        {
            Teleport();
            return;
        }

        float x = Random.Range(arena.bounds.min.x + 2, arena.bounds.max.x - 2);
        float y = Random.Range(arena.bounds.min.y + 2, arena.bounds.max.y - 2);

        PlayTeleportEffect(transform.position);

        transform.position = new Vector3(x, y, 0);

        PlayTeleportEffect(transform.position);
    }

    // ==================== 特效播放 ====================
    // 统一特效生成方法：先销毁旧特效再生成新特效，避免重复堆积
    public void PlayTeleportEffect(Vector3 position)
    {
        SpawnSingleEffect(teleportEffect, position, ref currentTeleportEffectInstance);
    }

    public void PlayCastSpellEffect(Vector3 position)
    {
        SpawnSingleEffect(castSpellEffect, position, ref currentCastSpellEffectInstance);
    }

    public void PlaySmashGroundEffect(Vector3 position)
    {
        SpawnSingleEffect(smashGroundEffect, position, ref currentSmashGroundEffectInstance);
    }

    private void SpawnSingleEffect(GameObject effectPrefab, Vector3 position, ref GameObject currentEffectInstance)
    {
        if (effectPrefab == null)
            return;

        if (currentEffectInstance != null)
        {
            Destroy(currentEffectInstance);
        }

        currentEffectInstance = Instantiate(effectPrefab, position, Quaternion.identity);
    }

    // ==================== 状态结束回调 ====================
    public void TakeHitFinished()
    {
        stateMachine.ChangeState(walkState);
    }

    public void SkillFinished()
    {
        stateMachine.ChangeState(walkState);
    }

    // ==================== 距离计算 ====================
    // 计算与玩家的水平距离
    public float GetPlayerDistance()
    {
        Player player = PlayerManager.instance.player;
        if (player == null) return float.MaxValue;
        
        return Mathf.Abs(transform.position.x - player.transform.position.x);
    }

    // ==================== 命中特效 ====================
    public void PlayHitEffect(Vector3 position)
    {
        // 可以在这里添加粒子效果、屏幕震动等
    }

    // ==================== Gizmos绘制 ====================
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // 绘制踩踏范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, smashRadius);

        // 绘制瞬移范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, teleportRange);
    }
}
