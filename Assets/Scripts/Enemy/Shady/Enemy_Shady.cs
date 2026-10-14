using System.Collections;
using UnityEngine;

public class Enemy_Shady : Enemy
{
    // ==================== Shady专属参数 ====================
    // battleStateMoveSpeed: 战斗状态下的移动速度（通常比巡逻快）
    // explosivePrefab: 自爆预制体，死亡时生成
    // growSpeed/maxSize: 控制爆炸物的生长速度和最大尺寸
    [Header("Shady specifics")]
    public float battleStateMoveSpeed;

    [SerializeField] private GameObject explosivePrefab;
    [SerializeField] private float growSpeed;
    [SerializeField] private float maxSize;

    // ==================== 状态机声明 ====================
    // Shady拥有5个状态：待机、移动、战斗、眩晕、死亡
    // 注意：Shady没有独立的攻击状态，进入攻击范围直接自爆
    #region States
    public ShadyIdleState idleState { get; private set; }
    public ShadyMoveState moveState { get; private set; }
    public ShadyDeadState deadState { get; private set; }
    public ShadyStunnedState stunnedState { get; private set; }
    public ShadyBattleState battleState { get; private set; }
    #endregion

    // ==================== 生命周期 ====================
    // Awake中创建所有状态实例，Start时初始化状态机为待机状态

    protected override void Awake()
    {
        base.Awake();

        idleState = new ShadyIdleState(this, stateMachine, "Idle", this);
        moveState = new ShadyMoveState(this, stateMachine, "Move", this);
        deadState = new ShadyDeadState(this, stateMachine, "Dead", this);
        stunnedState = new ShadyStunnedState(this, stateMachine, "Stunned", this);
        battleState = new ShadyBattleState(this, stateMachine, "MoveFast", this);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
    }

    // ==================== 状态切换接口 ====================
    // CanbeStunned: 检查是否可以被眩晕，可以则切换到眩晕状态
    // Die: 死亡时切换到死亡状态（死亡状态会触发自爆）

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

    // ==================== 特殊攻击：自爆 ====================
    // 由动画事件调用：生成爆炸物并设置参数，同时禁用碰撞和重力
    // 这是Shady的核心机制：接近玩家后自爆造成范围伤害
    public override void AnimationSpecialAttackTrigger()
    {
        GameObject newExplosive = Instantiate(explosivePrefab, attackCheck.position, Quaternion.identity);
        newExplosive.GetComponent<Explosive_Controller>().SetupExplosive(stats, growSpeed, maxSize, attackCheckRadius);

        cd.enabled = false;
        rb.gravityScale = 0;
    }

    // ==================== 自毁接口 ====================
    // 由死亡状态调用，在动画播放完毕后销毁自身
    public void SelfDestroy() => Destroy(gameObject);
}
