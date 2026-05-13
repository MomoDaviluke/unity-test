using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIMoveState : PlayGroundedState
{
    public PlayerIMoveState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        AudioManager.instance.PlaySFX(14, null);
    }

    public override void Exit()
    {
        base.Exit();
        AudioManager.instance.StopSFX(14);
    }

    public override void Update()
    {
        base.Update();

        // 使用加速/减速系统，让移动更平滑
        float targetSpeed = xInput * player.moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        
        // 根据是否有输入选择加速度或减速度
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? player.acceleration : player.deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);
        
        rb.velocity = new Vector2(rb.velocity.x + movement * Time.deltaTime, rb.velocity.y);

        // 根据输入方向转向
        if (xInput != 0)
        {
            player.FlipController(xInput);
        }

        if (xInput == 0 || player.IsWallDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
