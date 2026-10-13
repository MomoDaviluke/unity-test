using System.Collections;
using UnityEngine;

public class ArcherMoveState : ArcherGroundState
{
    // ==================== 构造函数 ====================
    public ArcherMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    // ==================== 状态生命周期 ====================
    // Update中沿面朝方向巡逻，遇到墙壁或边缘时转身并切换回待机

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, enemy.rb.velocity.y);

        // 撞墙或者没有路时转身
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}