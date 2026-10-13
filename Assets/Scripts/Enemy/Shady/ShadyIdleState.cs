using System.Collections;
using UnityEngine;

public class ShadyIdleState : ShadyGroundedState
{
    // ==================== 构造函数 ====================
    public ShadyIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Shady _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时设置待机计时器
    // Update中计时器归零后切换到移动状态进行巡逻

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
            stateMachine.ChangeState(enemy.moveState);
    }
}
