using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonTeleportState（恶魔Boss瞬移状态）- 瞬移到安全位置
/// </summary>
public class DemonTeleportState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;
    private bool hasTeleported;

    // ==================== 构造函数 ====================
    public DemonTeleportState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止移动并重置瞬移标志
    // Update中只执行一次瞬移，完成后返回行走状态

    public override void Enter()
    {
        base.Enter();
        demon.SetZeroVelocity();
        hasTeleported = false;
    }

    public override void Update()
    {
        base.Update();

        // 只执行一次瞬移
        if (!hasTeleported)
        {
            hasTeleported = true;
            demon.FindSafeTeleportPosition();
            demon.Teleport();
            demon.SkillFinished();
        }
    }
}
