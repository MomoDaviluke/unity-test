using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeMoveState : SlimeGroundedState
{
    // 构造函数：初始化移动状态
    public SlimeMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 移动状态更新
    public override void Update()
    {
        base.Update();

        // 根据朝向方向移动
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, enemy.rb.velocity.y);

        // 检测墙壁或悬崖，如有则转向并切换到空闲状态
        if (enemy.IsWallDetected() || !enemy.IsGroundDetected())
        {
            enemy.Flip(); // 转向
            stateTimer = 0.2f; // 设置短暂的空闲时间
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}