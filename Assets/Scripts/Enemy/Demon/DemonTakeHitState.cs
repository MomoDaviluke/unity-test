using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonTakeHitState : EnemyState
{
    private Enemy_Demon demon;
    private float knockbackForceX = 5f;
    private float knockbackForceY = 8f;

    public DemonTakeHitState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();

        // 击退效果
        int dir = demon.transform.position.x > PlayerManager.instance.player.transform.position.x ? -1 : 1;
        rb.velocity = new Vector2(knockbackForceX * dir, knockbackForceY);
    }

    public override void Update()
    {
        base.Update();

        // 动画结束或落地后返回战斗状态
        if (stateTimer <= 0 && demon.IsGroundDetected())
        {
            demon.TakeHitFinished();
        }
    }

    public override void Exit()
    {
        base.Exit();
        rb.velocity = Vector2.zero;
    }
}
