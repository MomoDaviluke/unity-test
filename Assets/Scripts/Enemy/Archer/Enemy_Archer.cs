using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy_Archer : Enemy
{
    // ==================== 弓箭手专属参数 ====================
    // arrowPrefab: 箭矢预制体
    // arrowSpeed/arrowDamage: 箭矢速度和伤害
    // jumpVelocity: 后跳时的速度向量（与面朝方向相反）
    // jumpCooldown: 跳跃冷却时间
    // safeDistance: 安全距离，玩家进入此范围会触发后跳
    [Header("弓箭手特殊信息")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private int arrowDamage;

    public Vector2 jumpVelocity;
    public float jumpCooldown;
    public float safeDistance;//安全距离
    [HideInInspector] public float lastTimeJumped;

    // ==================== 环境检测参数 ====================
    // groundBehindCheck: 检测身后是否有地面的位置
    // groundBehindCheckSize: 检测范围大小
    // 用于判断后跳是否安全（避免跳崖）
    [Header("额外的碰撞检查")]//Additional collision check
    [SerializeField] private Transform groundBehindCheck;
    [SerializeField] private Vector2 groundBehindCheckSize;

    // ==================== 状态机声明 ====================
    // Archer拥有7个状态：待机、移动、战斗、攻击、死亡、眩晕、跳跃（后跳）
    #region States
    public ArcherIdleState idleState { get; private set; }
    public ArcherMoveState moveState { get; private set; }
    public ArcherBattleState battleState { get; private set; }
    public ArcherAttackState attackState { get; private set; }
    public ArcherDeadState deadState { get; private set; }
    public ArcherStunnedState stunnedState { get; private set; }
    public ArcherJumpState ArcherJumpState { get; private set; }
    #endregion

    // ==================== 生命周期 ====================
    // Awake中创建所有状态实例
    // Start时初始化状态机为待机状态

    protected override void Awake()
    {
        base.Awake();

        idleState = new ArcherIdleState(this, stateMachine, "Idle", this);
        moveState = new ArcherMoveState(this, stateMachine, "Move", this);
        battleState = new ArcherBattleState(this, stateMachine, "Idle", this);
        attackState = new ArcherAttackState(this, stateMachine, "Attack", this);
        deadState = new ArcherDeadState(this, stateMachine, "Dead", this);
        stunnedState = new ArcherStunnedState(this, stateMachine, "Stun", this);
        ArcherJumpState = new ArcherJumpState(this, stateMachine, "Jump", this);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    // ==================== 调试功能 ====================
    // 按U键强制眩晕（调试用）
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.U))
        {
            stateMachine.ChangeState(stunnedState);
        }
    }

    // ==================== 状态切换接口 ====================
    public override bool CanbeStunned()
    {
        if (base.CanbeStunned())
        {
            stateMachine.ChangeState(stunnedState);
            return true;
        }

        return false;
    }

    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deadState);
    }

    // ==================== 射箭攻击 ====================
    // 由动画事件调用：生成箭矢并设置速度和伤害
    // 箭矢方向与敌人面朝方向一致
    public override void AnimationSepcoalAttackTrigger()
    {
        GameObject newArrow = Instantiate(arrowPrefab, attackCheck.position, Quaternion.identity);

        newArrow.GetComponent<Arrow_Controller>().SetUpArrow(arrowSpeed * facingDir, stats);
    }

    // ==================== 环境检测方法 ====================
    // GroundBenhundCheck: 检测身后是否有地面（后跳安全检查）
    // WallBehind: 检测身后是否有墙壁（后跳安全检查）
    public bool GroundBenhundCheck() => Physics2D.BoxCast(groundBehindCheck.position, groundBehindCheckSize, 0, Vector2.zero, 0, isGround);

    public bool WallBehind() => Physics2D.Raycast(wallCheck.position, Vector2.right * -facingDir, wallCheckDistance + 2, isGround);

    // ==================== Gizmos绘制 ====================
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireCube(groundBehindCheck.position, groundBehindCheckSize);
    }
}