using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
public enum StatType
{
    strength,
    agility,
    intelligence,
    vitality,
    damage,
    critChance,
    critPower,
    maxHealth,
    armor,
    evasion,
    magicResistance,
    fireDamage,
    iceDamage,
    lightningDamage
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Heal
}

// 角色状态类，用于管理角色的属性和生命值
public class CharacterStats : MonoBehaviour
{
    private EntityFX fx;

    [Header("主要属性")]
    public Stat strength;     // 力量：每点增加1点物理伤害，1%暴击伤害，1点物理抗性
    public Stat agility;      // 敏捷：每点增加1%闪避率，1%暴击率
    public Stat intelligence; // 智力：每点增加1点魔法伤害，3点魔法抗性
    public Stat vitality;     // 体力：每点增加10点最大生命值

    [Header("进攻属性")]
    public Stat damage;      // 基础伤害
    public Stat critChance;  // 暴击率
    public Stat critpower;   // 暴击伤害倍率（默认150%）

    [Header("防御属性")]
    public Stat maxHealth;        // 最大生命值
    public Stat armor;            // 物理护甲
    public Stat evasion;          // 闪避值
    public Stat magicResistance;  // 魔法抗性

    [Header("魔法伤害")]
    public Stat fireDamage;      // 火焰伤害
    public Stat iceDamage;       // 冰冻伤害
    public Stat lightningDamage; // 闪电伤害

    // 状态效果标志
    public bool isIgnited;  // 点燃状态：持续造成伤害
    public bool isChilled;  // 冰冻状态：降低20%护甲
    public bool isShocked;  // 感电状态：降低20%命中

    // 状态效果计时器
    [SerializeField] private float ailmentsDuration = 4;  // 状态效果持续时间
    private float ignitedTimer;    // 点燃持续时间
    private float chilledTimer;    // 冰冻持续时间
    private float shockedTimer;    // 感电持续时间

    // 点燃伤害相关
    private float igniteDamageCooldown = .3f;  // 点燃伤害间隔
    private float igniteDamageTimer;           // 点燃伤害计时器
    private float igniteDamage;                // 点燃伤害值
    [SerializeField] private GameObject shockStrikePrefab;
    private int shockDamage;

    // 事件和生命值
    [SerializeField] public int currentHealth;  // 当前生命值
    public System.Action onHealthChanged;  // 生命值改变事件，用于更新UI

    public bool isDead { get; protected set; }
    public bool isInvincible { get; protected set; }
    private bool isVulnerable;

    // 初始化方法，在游戏开始时设置默认值并初始化当前生命值
    protected virtual void Start()
    {
        critpower.SetDefaultValue(150);//设置默认爆伤
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
            DecreaseHealthBy((int)igniteDamage, false); // 使用统一方法，不显示受击效果
            igniteDamageTimer = igniteDamageCooldown;
        }

    }
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

    // 计算并施加伤害给目标角色
    public virtual void DoDamage(CharacterStats _targetStats)
    {
        //
        if (_targetStats.isInvincible)
            return;

        if (_targetStats == null || _targetStats.isDead)
            return;

        // 如果目标是玩家，设置伤害来源
        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
        {
            targetPlayer.lastDamageSource = transform;
        }

        if (TargetCanAvoidAttack(_targetStats))
            return;
        _targetStats.GetComponent<Entity>().SetupKnockbackDir(transform);

        int totalDamage = damage.GetValue() + strength.GetValue();
        bool isCritical = false;

        //爆伤设置
        if (Cancrit())
        {
            totalDamage = CalculateCriticalDamage(totalDamage);
            isCritical = true;
        }
        // 计算暴击伤害动画
        fx.CreateFitFx(_targetStats.transform, isCritical);

        // 直接调用统一伤害处理方法
        _targetStats.DecreaseHealthBy(totalDamage, true, DamageType.Physical, isCritical);

        DoMagicDamage(_targetStats);
    }

    #region  Magical damage and ailements

    public virtual void DoMagicDamage(CharacterStats _targetStats)
    {
        if (_targetStats == null || _targetStats.isDead)
            return;

        // 如果目标是玩家，设置伤害来源
        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
        {
            targetPlayer.lastDamageSource = transform;
        }

        int _fireDamage = fireDamage.GetValue();
        int _iceDamage = iceDamage.GetValue();
        int _lightningDamage = lightningDamage.GetValue();

        int totalDamage = _fireDamage + _iceDamage + _lightningDamage + intelligence.GetValue();

        // 直接调用统一伤害处理方法
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


    // 应用点燃、冰冻或电击效果到角色上
    public void ApplyAilment(bool _isIgnited, bool _isChilled, bool _isShocked)
    {
        // 检查角色是否可以被点燃
        bool canApplyIgnite = !isIgnited && !isChilled && !isShocked;
        // 检查角色是否可以被冰冻
        bool canApplyChill = !isIgnited && !isChilled && !isShocked;
        // 检查角色是否可以被电击

        bool canApplyShock = !isIgnited && !isChilled;

        // 如果角色可以被点燃并且请求点燃，则应用点燃效果
        if (_isIgnited && canApplyIgnite)
        {
            isIgnited = _isIgnited;
            ignitedTimer = ailmentsDuration; // 设置点燃持续时间为指定的持续时间
            fx.IgniteFxFor(ailmentsDuration); // 触发点燃特效，持续指定的持续时间
        }

        // 如果角色可以被冰冻并且请求冰冻，则应用冰冻效果
        if (_isChilled && canApplyChill)
        {
            isChilled = _isChilled;
            chilledTimer = ailmentsDuration; // 设置冰冻持续时间为指定的持续时间
            fx.ChilFxFor(ailmentsDuration); // 触发冰冻特效，持续指定的持续时间

            float slowPercentage = .2f; // 冰冻效果使角色速度降低20%
            GetComponent<Entity>().SlowEntityBy(slowPercentage, ailmentsDuration); // 应用速度降低效果
        }

        // 如果角色可以被电击并且请求电击，则应用电击效果
        if (_isShocked && canApplyShock)
        {
            if (!isShocked)
            {
                ApplyShock(_isShocked);
            }
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
        if (isShocked)//如果已经处于电击状态，则不再重复触发
            return;

        isShocked = _isShocked;
        shockedTimer = ailmentsDuration; // 设置电击持续时间为指定的持续时间

        fx.ShockFxFor(ailmentsDuration); // 触发电击特效，持续指定的持续时间
    }


    private void HieNearestTargetWithShockStrike()
    {
        // 找到角色周围40单位距离内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 20);

        // 遍历所有碰撞体，检测并影响附近的敌人
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null && hit.transform != transform) // 检查碰撞体是否为敌方单位且不是自己
            {
                // 实例化电击攻击预制体
                GameObject newShockStrike = Instantiate(shockStrikePrefab, transform.position, Quaternion.identity);
                // 设置电击攻击的目标和伤害
                newShockStrike.GetComponent<ShockStrike_Controller>().Setup(shockDamage, hit.GetComponent<CharacterStats>());
            }
        }
    }


    public void SetupIgniteDamage(int _damage) => igniteDamage = _damage;
    public void SetupShockStrikeDamage(int _damage) => shockDamage = _damage;

    #endregion


    // 角色受到伤害时调用的方法
    public virtual void TakeDamage(int _damage)
    {
        if (isDead || isInvincible)
            return;

        DecreaseHealthBy(_damage, true);

    }

    // 统一的生命值减少方法
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
        {
            fx.CreatePopUpText(_damage.ToString(), damageType, isCritical);
        }

        // 只在需要时显示受击效果
        if (showImpact)
        {
            Entity entity = GetComponent<Entity>();
            if (entity != null)
                entity.DamageImpact();

            if (fx != null)
                fx.StartCoroutine("FlashFX");
        }

        // 触发血量变化事件
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

    // 角色死亡时调用的方法
    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 在这里添加死亡时的清理逻辑
        enabled = false;
    }
    public void KillEntity()
    {
        if (!isDead)
            Die();
    }
    public void MakeInvincible(bool _invincible) => isInvincible = _invincible;


    #region Stat calculation
    // 检查目标角色的护甲并计算最终伤害
    protected int CheckTargetArmor(CharacterStats _targetStats, int totleDamage)
    {
        if (_targetStats.isChilled)
        {
            totleDamage -= Mathf.RoundToInt(_targetStats.armor.GetValue() * .8f);
        }
        else
        {
            totleDamage -= _targetStats.armor.GetValue();
        }


        totleDamage = Math.Clamp(totleDamage, 0, int.MaxValue);// 确保伤害值不会小于0（负数伤害会变成治疗）

        return totleDamage;
    }
    //检查魔法抗性
    private int CheckTargetResistance(CharacterStats _targetStats, int totleDamage)
    {
        totleDamage -= _targetStats.magicResistance.GetValue() + (_targetStats.intelligence.GetValue() * 3);
        totleDamage = Math.Clamp(totleDamage, 0, int.MaxValue);
        return totleDamage;
    }
    public virtual void OnEvasion()
    {

    }

    // 检查目标角色是否可以闪避攻击
    protected bool TargetCanAvoidAttack(CharacterStats _targetStats)
    {
        int totalEvasion = _targetStats.evasion.GetValue() + _targetStats.agility.GetValue();

        if (isShocked)
        {
            totalEvasion += 20;
        }

        if (UnityEngine.Random.Range(0, 100) < totalEvasion)
        {
            _targetStats.OnEvasion();
            return true;
        }
        return false;
    }

    // 检查角色是否可以暴击
    protected bool Cancrit()
    {
        int totalCriticalChance = critChance.GetValue() + agility.GetValue();

        if (UnityEngine.Random.Range(0, 100) < totalCriticalChance)
        {
            return true;
        }
        return false;
    }

    // 计算暴击伤害
    protected int CalculateCriticalDamage(int _damage)
    {
        float totalCritPower = (critpower.GetValue() + strength.GetValue()) * .01f;

        float critiDamage = _damage * totalCritPower;

        return Mathf.RoundToInt(critiDamage);
    }
    public int GetMaxHealthValue()
    {

        return maxHealth.GetValue() + vitality.GetValue() * 10;

    }//统计生命值函数
    #endregion
    public Stat GetStat(StatType startType)
    {
        switch (startType)
        {
            case global::StatType.strength:
                return strength;
            case global::StatType.agility:
                return agility;
            case global::StatType.intelligence:
                return intelligence;
            case global::StatType.vitality:
                return vitality;
            case global::StatType.damage:
                return damage;
            case global::StatType.critChance:
                return critChance;
            case global::StatType.critPower:
                return critpower;
            case global::StatType.maxHealth:
                return maxHealth;
            case global::StatType.armor:
                return armor;
            case global::StatType.evasion:
                return evasion;
            case global::StatType.magicResistance:
                return magicResistance;
            case global::StatType.fireDamage:
                return fireDamage;
            case global::StatType.iceDamage:
                return iceDamage;
            case global::StatType.lightningDamage:
                return lightningDamage;
            default:
                return null;
        }
    }
}
