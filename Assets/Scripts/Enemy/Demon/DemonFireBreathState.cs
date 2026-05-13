using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonFireBreathState : EnemyState
{
    private Enemy_Demon demon;

    public DemonFireBreathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();
        
        demon.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        // 动画结束，触发技能完成
        if (triggerCalled)
        {
            demon.SkillFinished();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 动画帧事件 - 喷火
    public void TriggerFireBreath()
    {
        demon.CastFireBreath();
    }
}
