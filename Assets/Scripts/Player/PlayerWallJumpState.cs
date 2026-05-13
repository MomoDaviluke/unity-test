using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    private float wallJumpForce = 5f; // 可调整的蹬墙跳力度
    private float jumpTime = 0.4f;    // 可调整的跳跃时间

    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = jumpTime;
        
        // 优化蹬墙跳的力度和方向
        float jumpDirection = -player.facingDir;
        player.SetVelocity(wallJumpForce * jumpDirection, player.jumpForce);
        
        // 翻转角色朝向
        player.Flip();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.airState);
        }

        // 允许玩家在空中稍微控制移动
        if (xInput != 0)
        {
            float moveSpeed = player.moveSpeed * 0.5f; // 空中移动速度减半
            rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);
        }

        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
