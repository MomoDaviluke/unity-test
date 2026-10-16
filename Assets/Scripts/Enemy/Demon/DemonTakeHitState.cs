using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonTakeHitState（恶魔Boss受击状态）- 受击动画和击退效果
/// </summary>
public class DemonTakeHitState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;
    private float knockbackForceX = 5f;
    private float knockbackForceY = 8f;

    // ==================== 构造函数 ====================
    public DemonTakeHitState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时根据玩家位置计算击退方向并施加击退力
    // Update中计时结束且落地后返回行走状态
    // Exit时停止移动

    public override void Enter()
    {
        base.Enter();
        int dir = demon.transform.position.x > PlayerManager.instance.player.transform.position.x ? -1 : 1;
        rb.linearVelocity = new Vector2(knockbackForceX * dir, knockbackForceY);
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer <= 0 && demon.IsGroundDetected())
            demon.TakeHitFinished();
    }

    public override void Exit()
    {
        base.Exit();
        rb.linearVelocity = Vector2.zero;
    }
}
