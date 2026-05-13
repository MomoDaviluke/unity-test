using System.Collections;
using UnityEngine;

public class Player : Entity
{
    [Header("攻击")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = 0.2f;
    public bool isBusy { get; private set; }
    [HideInInspector] public Transform lastDamageSource;//上次受到伤害的来源
    [Header("移动")]
    public float moveSpeed = 12f;
    public float jumpForce;
    public float swordReturnImpact;
    public float acceleration = 10f;        // 移动加速度
    public float deceleration = 10f;        // 移动减速度
    public float airControlMultiplier = 0.8f; // 空中控制倍率
    private float defualtMoveSpeed;
    private float defualtJumpForce;

    [Header("跳跃优化")]
    public float coyoteTime = 0.1f;         // 土狼时间：离开平台后仍可跳跃的时间
    public float jumpBufferTime = 0.1f;     // 跳跃缓冲：提前按跳的有效时间
    public float variableJumpHeightMult = 0.5f; // 变量跳跃高度倍率（松开跳键减速）
    public float coyoteTimer { get; private set; }  // 土狼计时器
    public float jumpBufferTimer { get; set; }      // 跳跃缓冲计时器

    [Header("冲刺")]
    public float dashSpeed;
    public float dashDuration;
    private float defaultDashSpeed;
    public float dashDir { get; private set; }

    [Header("闪避")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.2f;
    public float dodgeCooldown = 1f;
    public float lastDodgeTime { get; set; } = -999f;

    public SkillManager skill { get; private set; }
    public GameObject sword { get; private set; }
    public PlayerFX fX { get; private set; }

    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerIMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayDashState dashState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerPrimaryAttackState primaryAttackState { get; private set; }
    public PlayCounterAttackState counterAttackState { get; private set; }
    public PlayerAimSwordState aimSowrd { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerBlackholeState blackhole { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public PlayerDodgeState dodgeState { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerIMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayDashState(this, stateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttackState = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttackState = new PlayCounterAttackState(this, stateMachine, "CounterAttack");

        aimSowrd = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        blackhole = new PlayerBlackholeState(this, stateMachine, "Jump");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
        dodgeState = new PlayerDodgeState(this, stateMachine, "Dodge");
    }

    protected override void Start()
    {
        base.Start();

        fX = GetComponent<PlayerFX>();
        skill = SkillManager.instance;

        stateMachine.Initialize(idleState);
        defualtMoveSpeed = moveSpeed;
        defualtJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }


    protected override void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        base.Update();

        stateMachine.currentState.Update();

        // 更新土狼时间计时器
        UpdateCoyoteTime();

        // 更新跳跃缓冲计时器
        UpdateJumpBuffer();

        checkForDashInput();
        if (Input.GetKeyDown(KeyCode.F) && skill.crystal.crystalUnlocked)
        {
            skill.crystal.CanUseSkill();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Inventory.instance.UseFlask();
        }
    }

    // 更新土狼时间：在地面时重置，离开地面后倒计时
    private void UpdateCoyoteTime()
    {
        if (IsGroundDetected())
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    // 更新跳跃缓冲：倒计时
    private void UpdateJumpBuffer()
    {
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    // 检查是否可以跳跃（土狼时间内）
    public bool CanJump()
    {
        return coyoteTimer > 0 && jumpBufferTimer > 0;
    }

    // 使用跳跃（重置计时器）
    public void UseJump()
    {
        coyoteTimer = 0;
        jumpBufferTimer = 0;
    }
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        jumpForce = jumpForce * (1 - _slowPercentage);
        dashSpeed = dashSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);

        Invoke("ReturnDefaultSpeed", _slowDuration);
    }
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defualtMoveSpeed;
        jumpForce = defualtJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword);
    }

    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    private void checkForDashInput()
    {
        // 如果技能未解锁，直接返回
        if (skill.dash.dashUnlocked == false)
        {
            return;
        }

        // 如果检测到墙壁，直接返回
        if (IsWallDetected())
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.Q) && SkillManager.instance.dash.CanUseSkill())
        {

            dashDir = Input.GetAxisRaw("Horizontal");

            if (dashDir == 0)
            {
                dashDir = facingDir;
            }
            stateMachine.ChangeState(dashState);
        }
    }
    public override void Die()
    {
        base.Die();
        // 在玩家死亡时立即保存游戏状态
        SaveManager.instance.SaveGame();
        stateMachine.ChangeState(deadState);
    }
    protected override void SetupZeroKnockbackPower()
    {
        knockbackPower = new Vector2(0, 0);
        // 确保在击退结束后重置玩家速度
        if (!isKnocked)
            rb.velocity = Vector2.zero;
    }
}