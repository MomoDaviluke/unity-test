using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodgeState : PlayerState
{
    private float dodgeDirection;

    public PlayerDodgeState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // 设置闪避时间
        stateTimer = player.dodgeDuration;
        
        // 直接读取当前输入，避免使用上一帧的输入值
        float currentXInput = Input.GetAxisRaw("Horizontal");
        
        // 记录当前输入方向作为闪避方向，无输入则使用朝向方向
        dodgeDirection = currentXInput != 0 ? currentXInput : player.facingDir;
        
        // 记录闪避时间
        player.lastDodgeTime = Time.time;
        
        // 应用闪避速度
        player.SetVelocity(player.dodgeSpeed * dodgeDirection, 0f);
        
        // 创建残影效果
        player.fX.CreateAfterImage();
    }

    public override void Update()
    {
        base.Update();
        
        // 保持闪避速度
        player.SetVelocity(player.dodgeSpeed * dodgeDirection, rb.velocity.y);
        
        // 闪避时持续创建残影
        player.fX.CreateAfterImage();
        
        // 闪避时间结束后返回相应状态
        if (stateTimer < 0)
        {
            if (player.IsGroundDetected())
            {
                if (xInput != 0)
                    stateMachine.ChangeState(player.moveState);
                else
                    stateMachine.ChangeState(player.idleState);
            }
            else
            {
                stateMachine.ChangeState(player.airState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}