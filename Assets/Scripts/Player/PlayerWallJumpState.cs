using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerWallJumpState（玩家蹬墙跳状态）- 继承自PlayerState
/// 功能：从墙壁向反方向跳跃，实现平台跳跃中的连续攀爬
/// 设计思路：蹬墙跳提供水平反冲力和垂直跳跃力，自动翻转朝向
/// </summary>
public class PlayerWallJumpState : PlayerState
{
    // ==================== 蹬墙跳参数 ====================
    private float wallJumpForce = 5f; // 水平反冲力度
    private float jumpTime = 0.4f;    // 蹬墙跳持续时间（期间锁定状态）

    // ==================== 构造函数 ====================
    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 向反方向施加力并翻转朝向
    // Update: 蹬墙跳期间允许轻微空中控制，检测着陆

    public override void Enter()
    {
        base.Enter();
        stateTimer = jumpTime;

        // 计算蹬墙跳方向：与当前朝向相反
        float jumpDirection = -player.facingDir;
        player.SetVelocity(wallJumpForce * jumpDirection, player.jumpForce);

        // 翻转角色朝向（面向跳跃方向）
        player.Flip();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 蹬墙跳时间结束，切换到空中状态
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.airState);
        }

        // 允许玩家在空中稍微控制移动（速度减半）
        if (xInput != 0)
        {
            float moveSpeed = player.moveSpeed * 0.5f;
            rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
        }

        // 接触地面时切换回待机状态
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
