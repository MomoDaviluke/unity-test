using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonIdleState（恶魔Boss待机状态）- 技能释放后的短暂停顿
/// </summary>
public class DemonIdleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;
    private float defaultIdleTime = 1.5f;

    // ==================== 构造函数 ====================
    public DemonIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // 技能释放后的短暂停顿，停止移动，计时结束后进入行走状态

    public override void Enter()
    {
        base.Enter();
        stateTimer = defaultIdleTime;
        rb.velocity = Vector2.zero;
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer <= 0)
            stateMachine.ChangeState(demon.walkState);
    }
}
