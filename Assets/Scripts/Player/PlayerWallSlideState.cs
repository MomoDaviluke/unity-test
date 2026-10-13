using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerWallSlideState（玩家滑墙状态）- 继承自PlayerState
/// 功能：玩家贴墙缓慢下滑，可以蹬墙跳
/// 设计思路：贴墙时降低下落速度，提供平台跳跃游戏中的经典手感
/// </summary>
public class PlayerWallSlideState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerWallSlideState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Update中处理：
    //   1. 离开墙壁检测
    //   2. 蹬墙跳输入
    //   3. 按相反方向离开墙壁
    //   4. 控制下滑速度（按住下键快速下滑）
    //   5. 接触地面检测

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 离开墙壁时切换到空中状态
        if (player.IsWallDetected() == false)
            stateMachine.ChangeState(player.airState);

        // 蹬墙跳：按空格且按住朝向墙壁的方向
        if (Input.GetKeyDown(KeyCode.Space) && xInput == player.facingDir)
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }

        // 按相反方向离开墙壁
        if (xInput != 0 && player.facingDir != xInput)
            stateMachine.ChangeState(player.idleState);

        // 控制下滑速度：按住下键快速下滑，否则缓慢下滑
        if (yInput < 0)
            rb.velocity = new Vector2(0, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y * 0.5f);

        // 接触地面时切换回待机状态
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
