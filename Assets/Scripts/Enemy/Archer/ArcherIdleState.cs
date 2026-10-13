using System.Collections;
using UnityEngine;

public class ArcherIdleState : ArcherGroundState
{
    // ==================== 构造函数 ====================
    public ArcherIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName, _enemy)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时设置待机计时器
    // Update中计时器归零后切换到移动状态
    // Exit时播放音效

    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();

        AudioManager.instance.PlaySFX(24, enemy.transform);
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