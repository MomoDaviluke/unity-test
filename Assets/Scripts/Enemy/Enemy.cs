using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy（敌人基类）- 继承自Entity，实现敌人AI行为
/// 功能：状态机管理、追击玩家、攻击冷却、眩晕系统、时间冻结
/// 设计思路：将敌人共同行为抽象到基类，具体AI逻辑由子类状态实现
/// 使用场景：所有类型的敌人（骷髅、史莱姆、弓箭手、Boss等）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EntityFX))]
[RequireComponent(typeof(ItemDrop))]
public class Enemy : Entity
{
    // ==================== 检测与交互参数 ====================
    // 定义敌人检测玩家的层级掩码，用于射线检测时过滤非玩家对象
    [Header("检测参数")]
    [SerializeField] protected LayerMask isPlayer;      // 玩家层掩码：用于射线检测过滤玩家

    // ==================== 反击与眩晕系统 ====================
    // 设计思路：敌人在攻击前摇时打开反击窗口，玩家在此窗口内攻击可触发眩晕
    // counterImage用于视觉提示玩家"现在可以反击"，增强战斗反馈
    [Header("眩晕参数")]
    public float stunDuration = 1;                      // 眩晕持续时间：被反击后无法行动的时间
    public Vector2 stunDirection = new Vector2(10, 12); // 眩晕击退方向：X=水平击退，Y=垂直击飞
    protected bool canBeStunned;                        // 是否可以被眩晕：仅在攻击窗口期内为true
    [SerializeField] protected GameObject counterImage; // 反击提示图标：玩家可反击时显示

    // ==================== 移动与巡逻参数 ====================
    // 敌人有两种速度：巡逻时较慢（moveSpeed），发现玩家后加速追击（chaseSpeed）
    // idleTime控制巡逻时的停留，battleTime控制失去目标后的警惕持续时间
    [Header("移动参数")]
    public float moveSpeed = 1.5f;                      // 基础移动速度：巡逻时的速度
    public float chaseSpeed = 3;                        // 追击速度：发现玩家后的移动速度
    public float idleTime = 2;                          // 待机时间：巡逻时停留的时长
    public float battleTime = 7;                        // 战斗状态持续时间：失去玩家视野后保持警惕的时间
    private float defaultMoveSpeed;                     // 默认移动速度：用于减速后恢复

    // ==================== 攻击参数配置 ====================
    // 攻击冷却使用随机范围（min-max），让敌人攻击节奏更自然不机械
    // attackDistance是近战攻击的触发距离，agroDistance是发现玩家的仇恨距离
    [Header("攻击参数")]
    public float attackDistance = 2;                    // 攻击距离：进入此距离开始攻击
    public float attackCooldown = 2;                    // 攻击冷却时间：两次攻击之间的间隔
    public float minAttackCooldown = 1;                 // 最小攻击冷却：随机冷却的下限
    public float maxAttackCooldown = 2;                 // 最大攻击冷却：随机冷却的上限
    public float agroDistance = 3;                      // 仇恨距离：进入此范围开始追击玩家
    [HideInInspector] public float lastTimeAttacked;    // 上次攻击时间：用于计算攻击冷却

    // ==================== 核心组件引用 ====================
    // stateMachine是敌人AI的核心，管理所有状态（Idle、Move、Attack、Stun等）
    // lastAnimBoolName用于状态切换时恢复动画，避免动画中断后无法正确过渡
    public EnemyStateMachine stateMachine { get; private set; } // 敌人状态机：管理所有AI状态
    public EntityFX fX { get; private set; }            // 特效组件：播放受击、死亡等特效
    public string lastAnimBoolName { get; private set; } // 上次动画参数名：用于恢复动画状态

    // ==================== 生命周期方法 ====================
    // Awake创建状态机实例，Start获取组件引用，Update驱动状态机运转
    // 注意：状态机在此仅初始化，具体状态由子类在Start中添加

    /// <summary>
    /// 初始化：创建状态机并保存默认速度
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
        defaultMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// 启动：获取特效组件并隐藏反击提示
    /// </summary>
    protected override void Start()
    {
        base.Start();
        fX = GetComponent<EntityFX>();
        counterImage?.SetActive(false); // 初始状态隐藏反击提示
    }

    /// <summary>
    /// 每帧更新：执行当前状态的Update逻辑
    /// </summary>
    protected override void Update()
    {
        base.Update();
        stateMachine.currentState?.Update();
    }

    // ==================== 动画与速度管理 ====================
    // AssignLastAnimName用于保存动画状态，减速系统支持技能和陷阱效果
    // 减速同时影响移动速度和动画速度，恢复时统一还原

    /// <summary>
    /// 记录当前动画参数名：用于状态切换时恢复动画
    /// </summary>
    public virtual void AssignLastAnimName(string _animBoolName) => lastAnimBoolName = _animBoolName;

    /// <summary>
    /// 减速效果：被技能或陷阱减速时调用
    /// </summary>
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        chaseSpeed = chaseSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);
        Invoke("ReturnDefaultSpeed", _slowDuration);
    }

    /// <summary>
    /// 恢复默认速度：减速效果结束后调用
    /// </summary>
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defaultMoveSpeed;
    }

    // ==================== 时间冻结系统 ====================
    // 被时间停止技能影响时，将移动速度和动画速度设为0实现"冻结"效果
    // 支持永久冻结（FreezeTime）和定时自动解冻（FreezeTimeFor协程）
    #region 时间冻结
    /// <summary>
    /// 冻结/解冻时间：被时间停止技能影响时调用
    /// </summary>
    /// <param name="_timeFrozen">是否冻结</param>
    public virtual void FreezeTime(bool _timeFrozen)
    {
        if (_timeFrozen)
        {
            moveSpeed = 0;      // 停止移动
            anim.speed = 0;     // 暂停动画
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1;
        }
    }

    /// <summary>
    /// 冻结指定时长后自动解冻
    /// </summary>
    public virtual void FreezeTimeFor(float _duration) => StartCoroutine(FreezeTimerCoroutine(_duration));

    /// <summary>
    /// 时间冻结协程：冻结指定时长后恢复
    /// </summary>
    protected virtual IEnumerator FreezeTimerCoroutine(float _seconds)
    {
        FreezeTime(true);
        yield return new WaitForSeconds(_seconds);
        FreezeTime(false);
    }
    #endregion

    // ==================== 反击窗口系统 ====================
    // 设计思路：敌人在攻击前摇（OpenCountAttackWindow）时暴露弱点
    // 玩家此时攻击可触发眩晕，counterImage提供视觉反馈
    // 攻击后摇（CloseCountAttackWindow）时关闭窗口，恢复正常状态
    #region 反击窗口
    /// <summary>
    /// 打开反击窗口：敌人攻击前摇期间，玩家可以反击
    /// </summary>
    public virtual void OpenCountAttackWindow()
    {
        canBeStunned = true;
        counterImage?.SetActive(true); // 显示反击提示图标
    }

    /// <summary>
    /// 关闭反击窗口：敌人攻击后摇期间，无法被反击
    /// </summary>
    public virtual void CloseCountAttackWindow()
    {
        canBeStunned = false;
        counterImage?.SetActive(false); // 隐藏反击提示图标
    }
    #endregion

    // ==================== 眩晕判定与动画触发 ====================
    // CanbeStunned检查当前是否处于反击窗口，如果是则关闭窗口并返回true
    // AnimationFinishTrigger由动画事件调用，通知状态机动画播放完毕

    /// <summary>
    /// 检查是否可以被眩晕：反击窗口期内可以被眩晕
    /// </summary>
    public virtual bool CanbeStunned()
    {
        if (canBeStunned)
        {
            CloseCountAttackWindow();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 动画完成触发：由动画事件调用，通知当前状态动画已结束
    /// </summary>
    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    /// <summary>
    /// 特殊攻击触发：子类重写实现特殊攻击逻辑
    /// </summary>
    public virtual void AnimationSepcoalAttackTrigger() { }

    // ==================== 玩家检测与伤害计算 ====================
    // IsPlayerDetected使用射线检测前方是否有玩家，检测距离50单位
    // DamagePlayer计算总伤害（物理+元素）并应用到玩家，同时记录伤害来源

    /// <summary>
    /// 检测前方是否有玩家：向前发射射线检测
    /// </summary>
    public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(groundCheck.position, Vector2.right * facingDir, 50, isPlayer);

    /// <summary>
    /// 对玩家造成伤害：计算总伤害并应用到玩家
    /// </summary>
    protected virtual void DamagePlayer()
    {
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            player.lastDamageSource = transform; // 记录伤害来源（用于反弹等效果）
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
                playerStats.TakeDamage(CalculateTotalDamage());
        }
    }

    /// <summary>
    /// 计算总伤害：基础伤害 + 所有元素伤害
    /// </summary>
    private int CalculateTotalDamage()
    {
        EnemyStats stats = GetComponent<EnemyStats>();
        if (stats == null) return 0;
        return stats.damage.GetValue() + stats.fireDamage.GetValue() +
               stats.iceDamage.GetValue() + stats.lightningDamage.GetValue();
    }

    // ==================== 调试辅助 ====================
    // 在Scene视图中绘制黄色线条表示攻击距离，便于调整平衡性
    /// <summary>
    /// 绘制攻击范围辅助线（Scene视图可见）
    /// </summary>
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
    }
}
