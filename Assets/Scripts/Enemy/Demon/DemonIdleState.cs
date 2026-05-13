using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonIdleState : EnemyState
{
    private Enemy_Demon demon;
    private float defaultIdleTime = 1.5f;

    public DemonIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();
        
        stateTimer = defaultIdleTime;
        rb.velocity = Vector2.zero;
    }

    public override void Update()
    {
        base.Update();

        // 倒计时结束，进入行走状态
        if (stateTimer <= 0)
        {
            stateMachine.ChangeState(demon.walkState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
