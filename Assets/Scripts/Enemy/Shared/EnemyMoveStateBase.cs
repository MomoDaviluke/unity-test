using UnityEngine;

/// <summary>
/// EnemyMoveStateBase（巡逻移动状态基类）- Phase 4 共享状态基类
/// 继承地面基类。以 moveSpeed 沿面朝方向巡逻，撞墙或到边缘时转身并回待机。
/// 注：Slime 原实现转身时设 stateTimer = 0.2f，但 IdleState.Enter 会立即用
/// idleTime 覆盖该值，属死代码，迁移时删除（行为等价）。
/// </summary>
public abstract class EnemyMoveStateBase : EnemyGroundedStateBase
{
    // ==================== 子类必须提供的状态 ====================
    protected abstract EnemyState IdleState { get; }     // 受阻后切换的待机状态

    // ==================== 构造函数 ====================
    protected EnemyMoveStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    public override void Update()
    {
        base.Update();   // 先执行玩家检测（发现玩家优先于巡逻）

        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);

        // 撞墙或到达边缘：转身并回到待机
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            stateMachine.ChangeState(IdleState);
        }
    }
}
