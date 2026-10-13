using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlimeIdleState : SlimeGroundedState
{
    // ==================== 构造函数 ====================
    public SlimeIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时设置待机计时器
    // Update中计时器归零后切换到移动状态

    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }
}