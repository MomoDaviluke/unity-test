using System.Collections;
using UnityEngine;

/// <summary>
/// Player（玩家控制器）- 继承自Entity，实现玩家特有的移动、技能系统
/// 核心功能：状态机管理、跳跃优化（土狼时间/跳跃缓冲）、冲刺、闪避
/// Phase 5：输入读取全部经 InputReader；攻击/剑相关职责拆分至 PlayerCombat（partial）；
/// 死亡存档改由 GameEvents.OnPlayerDead 驱动（SaveManager 订阅）
/// </summary>
public partial class Player : Entity
{
    // ==================== 移动与物理参数 ====================
    // 使用加速度/减速度模拟真实的角色移动手感，而非瞬间变速
    // airControlMultiplier模拟空中惯性，使平台跳跃更有手感
    [Header("移动参数")]
    public float moveSpeed = 12f;                       // 基础移动速度
    public float jumpForce;                             // 跳跃力度（Inspector中设置，决定跳多高）
    public float swordReturnImpact;                     // 剑返回时的冲击力（接剑时的后坐力）
    public float acceleration = 10f;                    // 移动加速度（值越大，从静止到全速越快）
    public float deceleration = 10f;                    // 移动减速度（值越大，松手后停下越快）
    public float airControlMultiplier = 0.8f;           // 空中控制倍率（空中只能发挥80%的移速，模拟惯性）
    private float defaultMoveSpeed;                     // 默认移动速度（被减速后用于恢复）
    private float defaultJumpForce;                     // 默认跳跃力度（被减速后用于恢复）

    // ==================== 跳跃优化系统 ====================
    // 土狼时间（Coyote Time）：离开平台后短暂时间内仍可跳跃，防止边缘掉落 frustration
    // 跳跃缓冲（Jump Buffer）：提前按下跳跃键会缓存输入，落地瞬间自动跳跃
    // 可变跳跃高度：按住跳得高，松开重力加倍实现短跳，增加操作深度
    [Header("跳跃优化参数")]
    public float coyoteTime = 0.1f;                     // 土狼时间：离开平台后仍可跳跃的缓冲时间
    public float jumpBufferTime = 0.1f;                 // 跳跃缓冲：提前按下跳跃键的有效时间
    public float variableJumpHeightMult = 0.5f;         // 可变跳跃高度倍率（松开跳跃键后重力加倍，实现短跳/长跳）
    public float coyoteTimer { get; private set; }      // 土狼时间计时器（当前剩余时间，>0表示可以跳）
    public float jumpBufferTimer { get; set; }          // 跳跃缓冲计时器（当前剩余时间，>0表示按键有效）

    // ==================== 冲刺与闪避参数 ====================
    // 冲刺（Dash）是技能系统的一部分，需要解锁；闪避（Dodge）是基础动作
    // dashDir在输入时确定，若无输入则使用当前朝向，确保冲刺总有方向
    [Header("冲刺参数")]
    public float dashSpeed;                             // 冲刺速度（比跑步快很多）
    public float dashDuration;                          // 冲刺持续时间（这段时间内无敌且快速移动）
    private float defaultDashSpeed;
    public float dashDir { get; private set; }          // 冲刺方向（1=右，-1=左）

    [Header("闪避")]
    public float dodgeSpeed = 15f;                      // 闪避移动速度
    public float dodgeDuration = 0.2f;                  // 闪避持续时间
    public float dodgeCooldown = 1f;                    // 闪避冷却时间
    public float lastDodgeTime { get; set; } = -999f;   // 上次闪避时间（用于计算冷却）

    // ==================== 外部组件引用 ====================
    // skill通过SkillManager单例获取，统一管理所有技能的解锁和冷却
    public SkillManager skill { get; private set; }     // 技能管理器引用
    public PlayerFX fX { get; private set; }            // 玩家特效组件

    // ==================== 玩家状态机 ====================
    // 使用状态模式管理玩家所有行为状态：Idle、Move、Jump、Attack、Dash等
    // 每个状态封装进入、更新、退出的逻辑，避免大量if-else判断
    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    public PlayerCounterAttackState counterAttackState { get; private set; }
    public PlayerAimSwordState aimSowrd { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerBlackholeState blackhole { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public PlayerDodgeState dodgeState { get; private set; }
    #endregion

    // ==================== 生命周期方法 ====================
    // Awake创建所有状态实例（状态需要引用Player和StateMachine）
    // Start获取外部组件并初始化状态机为Idle状态
    // Update驱动状态机、更新跳跃优化计时器、检测技能输入

    /// <summary>
    /// 初始化 - 创建状态机实例和所有状态对象
    /// Awake()在场景加载时调用，比Start()更早，确保状态提前准备好
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttackState = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttackState = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSowrd = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        blackhole = new PlayerBlackholeState(this, stateMachine, "Jump");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
        dodgeState = new PlayerDodgeState(this, stateMachine, "Dodge");
    }

    /// <summary>
    /// 启动 - 获取组件引用并初始化状态机
    /// Start()在Awake()之后调用，用于获取外部引用（如SkillManager.instance）
    /// </summary>
    protected override void Start()
    {
        base.Start();

        fX = GetComponent<PlayerFX>();
        skill = SkillManager.instance;

        stateMachine.Initialize(idleState);
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }

    /// <summary>
    /// 每帧更新 - 处理输入、更新状态机和计时器
    /// Phase 5 跳跃收敛：计时器更新先于状态机驱动，使地面状态的 CanJump()
    /// 在按键当帧即可读到最新缓冲/土狼值——消除原"状态直判 Space + Player 写缓冲"
    /// 的双路径判定（同一帧两次 ChangeState），且无额外帧延迟。
    /// </summary>
    protected override void Update()
    {
        if (Time.timeScale == 0)
            return;

        base.Update();

        UpdateCoyoteTime();
        UpdateJumpBuffer();

        stateMachine.currentState.Update();
        CheckForDashInput();

        if (InputReader.instance.CrystalPressed && skill.crystal.crystalUnlocked)
            skill.crystal.CanUseSkill();

        if (InputReader.instance.FlaskPressed)
            Inventory.instance.UseFlask();
    }

    // ==================== 跳跃优化系统实现 ====================
    // 土狼时间和跳跃缓冲是两个独立的计时器，同时满足时才允许跳跃
    // UseJump在跳跃瞬间重置两个计时器，防止同一帧内多次触发

    /// <summary>
    /// 更新土狼时间计时器 - 离开平台后仍可跳跃的缓冲时间
    /// </summary>
    private void UpdateCoyoteTime()
    {
        if (IsGroundDetected())
            coyoteTimer = coyoteTime;       // 在地面上时重置计时器
        else
            coyoteTimer -= Time.deltaTime;  // 空中时倒计时
    }

    /// <summary>
    /// 更新跳跃缓冲计时器 - 提前按下跳跃键的有效时间（输入经 InputReader）
    /// </summary>
    private void UpdateJumpBuffer()
    {
        if (InputReader.instance.JumpPressed)
            jumpBufferTimer = jumpBufferTime;   // 按下跳跃键时重置计时器
        else
            jumpBufferTimer -= Time.deltaTime;  // 持续倒计时
    }

    /// <summary>
    /// 检查是否可以跳跃 - 需要同时满足土狼时间和跳跃缓冲两个条件
    /// </summary>
    public bool CanJump()
    {
        return coyoteTimer > 0 && jumpBufferTimer > 0;
    }

    /// <summary>
    /// 执行跳跃 - 重置计时器防止重复触发
    /// </summary>
    public void UseJump()
    {
        coyoteTimer = 0;
        jumpBufferTimer = 0;
    }

    // ==================== 速度与减速管理 ====================
    // 减速同时影响移动、跳跃、冲刺三个速度参数，恢复时统一还原
    // 使用Invoke延迟恢复，简化计时逻辑

    /// <summary>
    /// 减速效果 - 被技能或陷阱减速时调用，持续指定时间后自动恢复
    /// </summary>
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        base.SlowEntityBy(_slowPercentage, _slowDuration);
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);
        Invoke("ReturnDefaultSpeed", _slowDuration);
    }

    /// <summary>
    /// 恢复默认速度 - 由Invoke延迟调用
    /// </summary>
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    // ==================== 动画触发 ====================
    // AnimationTrigger由动画事件调用，通知当前状态动画关键帧已到达

    /// <summary>
    /// 动画触发器 - 由动画事件调用，通知当前状态动画已播放到关键帧
    /// </summary>
    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    // ==================== 冲刺输入检测 ====================
    // 检查解锁状态、墙壁检测（防止穿墙）、技能冷却，然后确定冲刺方向
    // 若无水平输入，使用当前朝向作为默认方向

    /// <summary>
    /// 检测冲刺输入 - 经 InputReader 读取，检查解锁状态、墙壁检测、冷却和方向
    /// </summary>
    private void CheckForDashInput()
    {
        if (skill.dash.dashUnlocked == false)
            return;

        if (IsWallDetected())
            return;

        if (InputReader.instance.DashPressed && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = InputReader.instance.MoveX;
            if (dashDir == 0)
                dashDir = facingDir;    // 如果没有输入方向，使用当前朝向
            stateMachine.ChangeState(dashState);
        }
    }

    // ==================== 死亡与击退重置 ====================
    // 死亡时切换到死亡状态（存档由 SaveManager 订阅 OnPlayerDead 完成），
    // SetupZeroKnockbackPower在死亡/复活时清理物理状态

    /// <summary>
    /// 死亡处理 - 切换到死亡状态（PlayerDeadState.Enter 广播 OnPlayerDead，
    /// SaveManager/UI 各自订阅响应，玩家逻辑不再直接引用存档系统）
    /// </summary>
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }

    /// <summary>
    /// 重置击退力度（死亡/复活时调用）
    /// </summary>
    protected override void SetupZeroKnockbackPower()
    {
        knockbackPower = new Vector2(0, 0);
        if (!isKnocked)
            rb.velocity = Vector2.zero;
    }
}
