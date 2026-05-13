using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonDeadState : EnemyState
{
    private Enemy_Demon demon;

    public DemonDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();

        // 停止移动
        rb.velocity = Vector2.zero;
    }

    public override void Update()
    {
        base.Update();

        // 死亡状态可以添加淡出等效果
    }

    public override void Exit()
    {
        base.Exit();
    }
}
