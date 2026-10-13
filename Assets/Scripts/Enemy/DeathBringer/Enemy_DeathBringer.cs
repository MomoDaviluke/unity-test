using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_DeathBringer : Enemy
{
    // ==================== 状态机声明 ====================
    // DeathBringer作为Boss拥有6个状态：待机、战斗、攻击、死亡、施法、传送
    #region States
    public DeathBringerBattleState battleState { get; private set; }
    public DeathBringerAttackState attackState { get; private set; }
    public DeathBringerIdleState idleState { get; private set; }
    public DeathBringerDeadState deadState { get; private set; }
    public DeathBringerSpellCastState spellCastState { get; private set; }
    public DeathBringerTeleportState teleportState { get; private set; }
    #endregion

    // ==================== 战斗阶段标记 ====================
    // bossFightBegun标记战斗是否已开始（玩家进入范围后触发）
    public bool bossFightBegun;

    // ==================== 施法参数 ====================
    // spellPrefab: 法术预制体
    // amountOfSpells: 每次施法释放的法术数量
    // spellCooldown: 法术释放间隔
    // spellStateCooldown: 两次施法状态之间的冷却
    // spellOffset: 法术生成位置相对于玩家的偏移
    [Header("Spell cast details")]
    [SerializeField] private GameObject spellPrefab;
    public int amountOfSpells;
    public float spellCooldown;
    public float lastTimeCast;
    [SerializeField] private float spellStateCooldown;
    [SerializeField] private Vector2 spellOffset;

    // ==================== 传送参数 ====================
    // arena: 战斗区域碰撞体，用于限制传送范围
    // surroundingCheckSize: 传送后检测周围是否有障碍物的Box大小
    // chanceToTeleport: 每次攻击后增加的传送概率（逐步累加）
    [Header("Teleport details")]
    [SerializeField] private BoxCollider2D arena;
    [SerializeField] private Vector2 surroundingCheckSize;
    public float chanceToTeleport;
    public float defaultChangeToTeleport = 25;

    // ==================== 地面检测 ====================
    [Header("Ground check")]
    [SerializeField] private LayerMask whatIsGround;

    // ==================== 生命周期 ====================
    // Awake中创建所有状态，设置默认面朝方向为左（-1）
    // Start时初始化状态机为待机状态

    protected override void Awake()
    {
        base.Awake();

        SetupDefaultFacingDir(-1);

        idleState = new DeathBringerIdleState(this, stateMachine, "Idle", this);

        battleState = new DeathBringerBattleState(this, stateMachine, "Move", this);
        attackState = new DeathBringerAttackState(this, stateMachine, "Attack", this);

        deadState = new DeathBringerDeadState(this, stateMachine, "Idle", this);
        spellCastState = new DeathBringerSpellCastState(this, stateMachine, "SpellCast", this);
        teleportState = new DeathBringerTeleportState(this, stateMachine, "Teleport", this);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    // ==================== 死亡处理 ====================
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }

    // ==================== 施法技能 ====================
    // 在玩家位置（带预测偏移）生成法术
    // 若玩家在移动，根据面朝方向预判位置
    public void CastSpell()
    {
        Player player = PlayerManager.instance.player;

        float xOffset = 0;

        if (player.rb.velocity.x != 0)
            xOffset = player.facingDir * spellOffset.x;

        Vector3 spellPosition = new Vector3(player.transform.position.x + xOffset, player.transform.position.y + spellOffset.y);

        GameObject newSpell = Instantiate(spellPrefab, spellPosition, Quaternion.identity);
        newSpell.GetComponent<DeathBringerSpell_Controller>().SetupSpell(stats);
    }

    // ==================== 传送位置查找 ====================
    // 在arena范围内随机选取位置，确保下方有地面且周围无障碍物
    // 修复 B6：原实现为无深度上限的递归重试，极端布局下会栈溢出/死循环卡死。
    //          改为带上限的迭代查找，超过上限后沿用最后一次落点并告警。
    private const int MaxFindPositionAttempts = 30;

    public void FindPosition()
    {
        for (int i = 0; i < MaxFindPositionAttempts; i++)
        {
            float x = Random.Range(arena.bounds.min.x + 3, arena.bounds.max.x - 3);
            float y = Random.Range(arena.bounds.min.y + 3, arena.bounds.max.y - 3);

            transform.position = new Vector3(x, y);
            transform.position = new Vector3(transform.position.x, transform.position.y - GroundBelow().distance + (cd.size.y / 2));

            // 落点有效：下方有地面且周围无障碍物
            if (GroundBelow() && !SomethingIsAround())
                return;

            Debug.Log("Looking for new position");
        }

        Debug.LogWarning($"[{gameObject.name}] 传送落点查找达到 {MaxFindPositionAttempts} 次上限，沿用最后一次落点，请检查 arena 布局。");
    }

    // ==================== 辅助检测方法 ====================
    // GroundBelow: 向下射线检测地面距离
    // SomethingIsAround: Box检测周围是否有障碍物
    private RaycastHit2D GroundBelow() => Physics2D.Raycast(transform.position, Vector2.down, 100, whatIsGround);
    private bool SomethingIsAround() => Physics2D.BoxCast(transform.position, surroundingCheckSize, 0, Vector2.zero, 0, whatIsGround);

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - GroundBelow().distance));
        Gizmos.DrawWireCube(transform.position, surroundingCheckSize);
    }

    // ==================== 传送判定 ====================
    // 每次攻击后增加5%传送概率，触发后重置为默认值
    public bool CanTeleport()
    {
        if (Random.Range(0, 100) <= chanceToTeleport)
        {
            chanceToTeleport = defaultChangeToTeleport;
            return true;
        }

        return false;
    }

    // ==================== 施法冷却判定 ====================
    // 检查是否超过施法状态冷却时间
    public bool CanDoSpellCast()
    {
        if (Time.time >= lastTimeCast + spellStateCooldown)
        {
            return true;
        }

        return false;
    }
}
