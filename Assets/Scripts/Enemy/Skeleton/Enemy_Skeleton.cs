using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy_Skeleton（骷髅敌人）- 继承自Enemy，实现骷髅特有的AI行为
/// 功能：巡逻、追击、攻击、眩晕、死亡
/// 设计思路：在Awake中创建所有状态，Start时初始化状态机
/// </summary>
public class Enemy_Skeleton : Enemy
{
    #region States
    public SkeletonIdleState idleState { get; private set; }     // 待机状态：原地停留
    public SkeletonMoveState moveState { get; private set; }     // 移动状态：巡逻移动
    public SkeletonBattleState battleState { get; private set; } // 战斗状态：追击玩家
    public SkeletonAttackState attackState { get; private set; } // 攻击状态：执行攻击
    public SkeletonStunnedState stunnedState { get; private set; } // 眩晕状态：被反击后
    public SkeletonDeadState deadState { get; private set; }     // 死亡状态：播放死亡动画
    #endregion

    /// <summary>
    /// 初始化：创建所有状态实例
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        idleState = new SkeletonIdleState(this, stateMachine, "Idle", this);
        moveState = new SkeletonMoveState(this, stateMachine, "Move", this);
        battleState = new SkeletonBattleState(this, stateMachine, "Move", this);
        attackState = new SkeletonAttackState(this, stateMachine, "Attack", this);
        stunnedState = new SkeletonStunnedState(this, stateMachine, "Stunned", this);
        deadState = new SkeletonDeadState(this, stateMachine, "Die", this);
    }

    /// <summary>
    /// 启动：初始化状态机为待机状态
    /// </summary>
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    /// <summary>
    /// 每帧更新：调试功能（按U键强制眩晕）
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.U))
        {
            stateMachine.ChangeState(stunnedState);
        }
    }

    /// <summary>
    /// 检查是否可以被眩晕：可以则切换到眩晕状态
    /// </summary>
    public override bool CanbeStunned()
    {
        if (base.CanbeStunned())
        {
            stateMachine.ChangeState(stunnedState);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 死亡处理：切换到死亡状态
    /// </summary>
    public override void Die()
    {
        base.Die();
        stateMachine.ChangeState(deadState);
    }
}
