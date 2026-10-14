using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerJumpState（玩家跳跃状态）- 继承自PlayerState
/// 功能：处理跳跃起跳和可变跳跃高度（按住跳得高，松开跳得低）
/// </summary>
public class PlayerJumpState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时施加向上的跳跃力，保持水平速度
    // Update中实现可变跳跃高度（按住跳得高，松开跳得低）
    // 开始下落时切换到空中状态

    public override void Enter()
    {
        base.Enter();
        rb.velocity = new Vector2(rb.velocity.x, player.jumpForce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // ---- 可变跳跃高度 ----
        // 松开跳键时减速，实现低跳（类似马里奥的跳跃手感）
        if (InputReader.instance.JumpReleased && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * player.variableJumpHeightMult);
        }

        // 开始下落时切换到空中状态
        if (rb.velocity.y < 0)
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
