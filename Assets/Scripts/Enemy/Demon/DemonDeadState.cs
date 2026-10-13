using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonDeadState（恶魔Boss死亡状态）- 停止移动并播放死亡动画
/// </summary>
public class DemonDeadState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;

    // ==================== 构造函数 ====================
    public DemonDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止所有移动

    public override void Enter()
    {
        base.Enter();
        rb.velocity = Vector2.zero;
    }
}
