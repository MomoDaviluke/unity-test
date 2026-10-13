using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonMoveState（骷髅移动状态）- 地面巡逻，碰到墙壁或边缘时转身
/// </summary>
public class SkeletonMoveState : SkeletonGroundState
{
    // ==================== 构造函数 ====================
    // 直接调用父类构造函数
    public SkeletonMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
    }

    // ==================== 巡逻逻辑 ====================
    // 以moveSpeed沿面朝方向移动
    // 检测到墙壁或到达边缘时转身并切换回待机状态

    public override void Update()
    {
        base.Update();
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);

        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
