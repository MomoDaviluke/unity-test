using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeMoveState : SlimeGroundedState
{
    // ==================== 构造函数 ====================
    public SlimeMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
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

        // 检测墙壁或悬崖，如有则转向并切换到空闲状态
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip();
            stateTimer = 0.2f; // 设置短暂的空闲时间
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}