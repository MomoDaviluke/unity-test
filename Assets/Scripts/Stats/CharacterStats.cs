using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum StatType
{
    strength, agility, intelligence, vitality, damage, critChance, critPower,
    maxHealth, armor, evasion, magicResistance, fireDamage, iceDamage, lightningDamage
}

public enum DamageType
{
    Physical, Fire, Ice, Lightning, Heal
}

/// <summary>
/// CharacterStats（角色属性基类）- 管理属性计算、伤害、状态效果
/// </summary>
public class CharacterStats : MonoBehaviour
{
    // ==================== 特效组件引用 ====================
    // fx用于播放受击特效、弹出伤害数字、状态异常视觉效果
    private EntityFX fx;

    // ==================== 角色基础属性 ====================
    // 四大主属性：力量（影响物理伤害/暴击）、敏捷（影响闪避/暴击率）、智力（影响魔法伤害/魔抗）、体质（影响生命值）
    // 使用Stat类封装，支持基础值+动态修饰符（装备/Buff）
    [Header("主要属性")]
    public Stat strength;
    public Stat agility;
    public Stat intelligence;
    public Stat vitality;

    // ==================== 进攻与防御属性 ====================
    // 物理伤害系统：damage基础伤害 + strength加成，critChance暴击概率，critpower暴击倍率
    // 防御系统：armor护甲减伤，evasion闪避概率，magicResistance魔法抗性
    [Header("进攻属性")]
    public Stat damage;
    public Stat critChance;
    public Stat critpower;

    [Header("防御属性")]
    public Stat maxHealth;
    public Stat armor;
    public Stat evasion;
    public Stat magicResistance;

    // ==================== 元素伤害属性 ====================
    // 三种元素伤害：火焰（可点燃持续伤害）、冰霜（可减速）、闪电（可感电连锁）
    // 元素伤害同时受intelligence加成
    [Header("魔法伤害")]
    public Stat fireDamage;
    public Stat iceDamage;
    public Stat lightningDamage;

    // ==================== 状态异常标志与计时器 ====================
    // 三种状态异常：点燃（持续掉血）、寒冷（减速+护甲降低）、感电（闪避降低+连锁闪电）
    // 使用计时器实现持续时间，Update中倒计时归零后清除状态
    public bool isIgnited;
    public bool isChilled;
    public bool isShocked;

    [SerializeField] private float ailmentsDuration = 4;
    private float ignitedTimer;
    private float chilledTimer;
    private float shockedTimer;

    // ==================== 点燃伤害计算 ====================
    // 点燃伤害 = 火焰伤害的20%，每0.3秒触发一次（igniteDamageCooldown）
    // shockStrikePrefab是感电时生成的连锁闪电预制体
    private float igniteDamageCooldown = .3f;
    private float igniteDamageTimer;
    private float igniteDamage;
    [SerializeField] private GameObject shockStrikePrefab;
    private int shockDamage;

    // ==================== 生命值与状态标志 ====================
    // currentHealth是当前生命值，onHealthChanged事件通知UI更新血条
    // isInvincible用于无敌帧（如冲刺、受击后短暂时间），isVulnerable使受到的伤害增加10%
    [SerializeField] public int currentHealth;
    public System.Action onHealthChanged;

    public bool isDead { get; protected set; }
    public bool isInvincible { get; protected set; }
    private bool isVulnerable;

    // ==================== 生命周期方法 ====================
    // Start初始化暴击倍率默认值和生命值，获取特效组件
    // Update管理状态异常计时器，点燃伤害按固定间隔触发

    protected virtual void Start()
    {
        critpower.SetDefaultValue(150);
        currentHealth = GetMaxHealthValue();
        fx = GetComponent<EntityFX>();
    }

    protected virtual void Update()
    {
        ignitedTimer -= Time.deltaTime;
        chilledTimer -= Time.deltaTime;
        shockedTimer -= Time.deltaTime;
        igniteDamageTimer -= Time.deltaTime;

        if (ignitedTimer < 0)
            isIgnited = false;
        if (chilledTimer < 0)
            isChilled = false;
        if (shockedTimer < 0)
            isShocked = false;

        if (igniteDamageTimer < 0 && isIgnited)
        {
            DecreaseHealthBy((int)igniteDamage, false);
            igniteDamageTimer = igniteDamageCooldown;
        }
    }

    // ==================== 临时状态修饰 ====================
    // MakeVulnerableFor使角色受到的伤害增加10%，用于某些技能效果
    // IncreaseStaBy提供临时属性加成（如药水、Buff），持续时间结束后自动移除修饰符

    public void MakeVulnerableFor(float _duration) => StartCoroutine(VulnerableForCorutine(_duration));

    private IEnumerator VulnerableForCorutine(float _duration)
    {
        isVulnerable = true;
        yield return new WaitForSeconds(_duration);
        isVulnerable = false;
    }

    public virtual void IncreaseStaBy(int _modifier, float _duration, Stat _staToModify)
    {
        StartCoroutine(StatModCorountine(_modifier, _duration, _staToModify));
    }

    private IEnumerator StatModCorountine(int _modifier, float _duration, Stat _staToModify)
    {
        _staToModify.AddModifier(_modifier);
        yield return new WaitForSeconds(_duration);
        _staToModify.RemoveModifier(_modifier);
    }

    // ==================== 伤害计算系统 ====================
    // 物理伤害流程：检查无敌/死亡 → 记录伤害来源 → 闪避判定 → 击退方向 → 暴击判定 → 扣血 → 元素伤害
    // 魔法伤害流程：计算三元素总和+智力 → 扣血 → 尝试附加状态异常
    // 状态异常规则：伤害最高的元素优先，若相等则随机决定
    #region 伤害计算
    public virtual void DoDamage(CharacterStats _targetStats)
    {
        if (_targetStats.isInvincible)
            return;
        if (_targetStats == null || _targetStats.isDead)
            return;

        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
            targetPlayer.lastDamageSource = transform;

        if (TargetCanAvoidAttack(_targetStats))
            return;

        _targetStats.GetComponent<Entity>().SetupKnockbackDir(transform);

        int totalDamage = damage.GetValue() + strength.GetValue();
        bool isCritical = false;

        if (Cancrit())
        {
            totalDamage = CalculateCriticalDamage(totalDamage);
            isCritical = true;
        }

        fx.CreateFitFx(_targetStats.transform, isCritical);
        _targetStats.DecreaseHealthBy(totalDamage, true, DamageType.Physical, isCritical);
        DoMagicDamage(_targetStats);
    }

    public virtual void DoMagicDamage(CharacterStats _targetStats)
    {
        if (_targetStats == null || _targetStats.isDead)
            return;

        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
            targetPlayer.lastDamageSource = transform;

        int _fireDamage = fireDamage.GetValue();
        int _iceDamage = iceDamage.GetValue();
        int _lightningDamage = lightningDamage.GetValue();
        int totalDamage = _fireDamage + _iceDamage + _lightningDamage + intelligence.GetValue();

        _targetStats.DecreaseHealthBy(totalDamage, true, GetHighestDamageType(_fireDamage, _iceDamage, _lightningDamage));

        if (Mathf.Max(_fireDamage, _iceDamage, _lightningDamage) <= 0)
            return;

        AttemptToApplyAilements(_targetStats, _fireDamage, _iceDamage, _lightningDamage);
    }

    private DamageType GetHighestDamageType(int fire, int ice, int lightning)
    {
        if (fire >= ice && fire >= lightning) return DamageType.Fire;
        if (ice >= fire && ice >= lightning) return DamageType.Ice;
        return DamageType.Lightning;
    }

    private void AttemptToApplyAilements(CharacterStats _targetStats, int _fireDamage, int _iceDamage, int _lightningDamage)
    {
        bool canApplyIgnite = _fireDamage > _iceDamage && _fireDamage > _lightningDamage;
        bool canApplyChill = _iceDamage > _fireDamage && _iceDamage > _lightningDamage;
        bool canApplyShock = _lightningDamage > _fireDamage && _lightningDamage > _iceDamage;

        while (!canApplyIgnite && !canApplyChill && !canApplyShock)
        {
            if (UnityEngine.Random.value < .33f && _fireDamage > 0)
            {
                canApplyIgnite = true;
                _targetStats.ApplyAilment(canApplyIgnite, canApplyChill, canApplyShock);
            }
            if (UnityEngine.Random.value < .5f && _iceDamage > 0)
            {
                canApplyChill = true;
                _targetStats.ApplyAilment(canApplyIgnite, canApplyChill, canApplyShock);
            }
            if (UnityEngine.Random.value < 1f && _lightningDamage > 0)
            {
                canApplyShock = true;
                _targetStats.ApplyAilment(canApplyIgnite, canApplyChill, canApplyShock);
            }
        }

        if (canApplyIgnite)
            _targetStats.SetupIgniteDamage(Mathf.RoundToInt(_fireDamage * .2f));

        if (canApplyShock)
            _targetStats.SetupShockStrikeDamage(Mathf.RoundToInt(_lightningDamage * .1f));

        _targetStats.ApplyAilment(canApplyIgnite, canApplyChill, canApplyShock);
    }

    // ==================== 状态异常应用 ====================
    // 应用规则：同一时间内只能有一种状态异常（互斥）
    // 点燃：持续掉血；寒冷：减速+护甲降低；感电：闪避降低，再次感电触发连锁闪电
    // 玩家被感电时不会触发连锁闪电（避免自杀）

    public void ApplyAilment(bool _isIgnited, bool _isChilled, bool _isShocked)
    {
        bool canApplyIgnite = !isIgnited && !isChilled && !isShocked;
        bool canApplyChill = !isIgnited && !isChilled && !isShocked;
        bool canApplyShock = !isIgnited && !isChilled;

        if (_isIgnited && canApplyIgnite)
        {
            isIgnited = _isIgnited;
            ignitedTimer = ailmentsDuration;
            fx.IgniteFxFor(ailmentsDuration);
        }

        if (_isChilled && canApplyChill)
        {
            isChilled = _isChilled;
            chilledTimer = ailmentsDuration;
            fx.ChilFxFor(ailmentsDuration);
            GetComponent<Entity>().SlowEntityBy(.2f, ailmentsDuration);
        }

        if (_isShocked && canApplyShock)
        {
            if (!isShocked)
                ApplyShock(_isShocked);
            else
            {
                if (GetComponent<Player>() != null)
                    return;
                HieNearestTargetWithShockStrike();
            }
        }
    }

    public void ApplyShock(bool _isShocked)
    {
        if (isShocked)
            return;

        isShocked = _isShocked;
        shockedTimer = ailmentsDuration;
        fx.ShockFxFor(ailmentsDuration);
    }

    private void HieNearestTargetWithShockStrike()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 20);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && hit.transform != transform)
            {
                GameObject newShockStrike = Instantiate(shockStrikePrefab, transform.position, Quaternion.identity);
                newShockStrike.GetComponent<ShockStrike_Controller>().Setup(shockDamage, hit.GetComponent<CharacterStats>());
            }
        }
    }

    public void SetupIgniteDamage(int _damage) => igniteDamage = _damage;
    public void SetupShockStrikeDamage(int _damage) => shockDamage = _damage;
    #endregion

    // ==================== 生命值管理系统 ====================
    // TakeDamage是外部入口，DecreaseHealthBy是实际扣血逻辑
    // 扣血流程：检查死亡/无敌 → 易伤加成 → 扣血 → 弹出伤害数字 → 击退/闪白 → 死亡判定
    // IncreaseHealthBy治疗并限制不超过最大生命值
    #region 生命值管理
    public virtual void TakeDamage(int _damage)
    {
        if (isDead || isInvincible)
            return;
        DecreaseHealthBy(_damage, true);
    }

    protected virtual void DecreaseHealthBy(int _damage, bool showImpact = false, DamageType damageType = DamageType.Physical, bool isCritical = false)
    {
        if (isDead)
            return;
        if (isInvincible)
            return;

        if (isVulnerable)
            _damage = Mathf.RoundToInt(_damage * 1.1f);

        currentHealth -= _damage;

        if (_damage > 0)
            fx.CreatePopUpText(_damage.ToString(), damageType, isCritical);

        if (showImpact)
        {
            Entity entity = GetComponent<Entity>();
            if (entity != null)
                entity.DamageImpact();
            if (fx != null)
                fx.StartCoroutine("FlashFX");
        }

        onHealthChanged?.Invoke();

        if (currentHealth <= 0 && !isDead)
        {
            currentHealth = 0;
            Die();
        }
    }

    public virtual void IncreaseHealthBy(int _amount)
    {
        currentHealth += _amount;
        if (currentHealth > GetMaxHealthValue())
            currentHealth = GetMaxHealthValue();

        fx.CreatePopUpText("+" + _amount.ToString(), DamageType.Heal);

        if (onHealthChanged != null)
            onHealthChanged();
    }

    protected virtual void Die()
    {
        if (isDead)
            return;
        isDead = true;
        enabled = false;
    }

    public void KillEntity()
    {
        if (!isDead)
            Die();
    }

    public void MakeInvincible(bool _invincible) => isInvincible = _invincible;
    #endregion

    // ==================== 属性计算与判定 ====================
    // 护甲计算：寒冷状态降低20%护甲效果；魔抗计算：魔抗+智力*3
    // 闪避判定：evasion+agility vs 随机数（0-100），感电时闪避+20（负面效果）
    // 暴击判定：critChance+agility vs 随机数，暴击伤害 = 伤害 * (critpower+strength)%
    // 最大生命值 = maxHealth基础值 + vitality*10
    #region 属性计算
    protected int CheckTargetArmor(CharacterStats _targetStats, int totleDamage)
    {
        if (_targetStats.isChilled)
            totleDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);
        else
            totleDamage -= _targetStats.armor.GetValue();

        totleDamage = Math.Clamp(totleDamage, 0, int.MaxValue);
        return totleDamage;
    }

    private int CheckTargetResistance(CharacterStats _targetStats, int totleDamage)
    {
        totleDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
        totleDamage = Math.Clamp(totleDamage, 0, int.MaxValue);
        return totleDamage;
    }

    public virtual void OnEvasion() { }

    protected bool TargetCanAvoidAttack(CharacterStats _targetStats)
    {
        int totalEvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

        if (isShocked)
            totalEvasion += 20;

        if (UnityEngine.Random.Range(0, 100) < totalEvasion)
        {
            _targetStats.OnEvasion();
            return true;
        }
        return false;
    }

    protected bool Cancrit()
    {
        int totalCriticalChance = critChance.GetValue() + agility.GetValue();

        if (UnityEngine.Random.Range(0, 100) < totalCriticalChance)
            return true;
        return false;
    }

    protected int CalculateCriticalDamage(int _damage)
    {
        float totalCritPower = (critpower.GetValue() + strength.GetValue()) * .01f;
        float critiDamage = _damage * totalCritPower;
        return Mathf.RoundToInt(critiDamage);
    }

    public int GetMaxHealthValue()
    {
        return maxHealth.GetValue() + vitality.GetValue() * 10;
    }
    #endregion

    public Stat GetStat(StatType startType)
    {
        switch (startType)
        {
            case global::StatType.strength: return strength;
            case global::StatType.agility: return agility;
            case global::StatType.intelligence: return intelligence;
            case global::StatType.vitality: return vitality;
            case global::StatType.damage: return damage;
            case global::StatType.critChance: return critChance;
            case global::StatType.critPower: return critpower;
            case global::StatType.maxHealth: return maxHealth;
            case global::StatType.armor: return armor;
            case global::StatType.evasion: return evasion;
            case global::StatType.magicResistance: return magicResistance;
            case global::StatType.fireDamage: return fireDamage;
            case global::StatType.iceDamage: return iceDamage;
            case global::StatType.lightningDamage: return lightningDamage;
            default: return null;
        }
    }
}
