using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonTeleportState : EnemyState
{
    private Enemy_Demon demon;
    private bool hasTeleported;

    public DemonTeleportState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();
        
        demon.SetZeroVelocity();
        hasTeleported = false;
    }

    public override void Update()
    {
        base.Update();

        // 立即执行瞬移（无动画，只有特效）
        if (!hasTeleported)
        {
            hasTeleported = true;
            demon.FindSafeTeleportPosition();
            demon.Teleport();
            demon.SkillFinished();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
