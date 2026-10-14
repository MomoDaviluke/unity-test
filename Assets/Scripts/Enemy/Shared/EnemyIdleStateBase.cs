using UnityEngine;

/// <summary>
/// EnemyIdleStateBase（待机状态基类）- Phase 4 共享状态基类
/// 继承地面基类（待机时同样检测玩家）。
/// Enter 设 idleTime 计时、Update 计时归零切移动状态。
/// 差异钩子：OnEnterExtra（Skeleton 关闭反击窗口）、OnExitExtra（Skeleton/Archer 播放音效 24）。
/// </summary>
public abstract class EnemyIdleStateBase : EnemyGroundedStateBase
{
    // ==================== 子类必须提供的状态 ====================
    protected abstract EnemyState MoveState { get; }     // 计时结束后的巡逻移动状态

    // ==================== 可覆写钩子 ====================
    protected virtual void OnEnterExtra() { }            // Enter 附加逻辑（计时器设置之前）
    protected virtual void OnExitExtra() { }             // Exit 附加逻辑

    // ==================== 构造函数 ====================
    protected EnemyIdleStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    public override void Enter()
    {
        base.Enter();
        OnEnterExtra();
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
        OnExitExtra();
    }

    public override void Update()
    {
        base.Update();   // 先执行地面检测（发现玩家优先于计时）
        if (stateTimer < 0)
            stateMachine.ChangeState(MoveState);
    }
}
