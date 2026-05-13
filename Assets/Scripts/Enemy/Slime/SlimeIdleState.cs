using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeIdleState : SlimeGroundedState
{
    // 构造函数：初始化空闲状态
    public SlimeIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {

    }

    // 进入空闲状态
    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.idleTime; // 设置空闲持续时间
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 空闲状态更新
    public override void Update()
    {
        base.Update();

        // 空闲时间结束后切换到移动状态
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}