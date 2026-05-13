using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

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
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
        if (player.IsWallDetected())
        {
            stateMachine.ChangeState(player.wallSlideState);
        }

        // 优化空中控制：使用加速系统，让空中移动更平滑
        if (xInput != 0)
        {
            float targetSpeed = xInput * player.moveSpeed * player.airControlMultiplier;
            float speedDiff = targetSpeed - rb.velocity.x;
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * player.acceleration * player.airControlMultiplier, 0.9f) * Mathf.Sign(speedDiff);
            rb.velocity = new Vector2(rb.velocity.x + movement * Time.deltaTime, rb.velocity.y);
            
            // 空中转向
            player.FlipController(xInput);
        }
        
        // 空中闪避：按Ctrl键
        if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time - player.lastDodgeTime > player.dodgeCooldown)
        {
            stateMachine.ChangeState(player.dodgeState);
        }
    }
}
