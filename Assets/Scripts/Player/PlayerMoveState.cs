using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerMoveState（玩家移动状态）- 继承自PlayerGroundedState
/// 功能：处理玩家在地面上的移动，使用加速/减速系统让移动更平滑
/// </summary>
public class PlayerMoveState : PlayerGroundedState
{
    // ==================== 构造函数 ====================
    public PlayerMoveState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时播放脚步声
    // Update中使用加速/减速系统实现平滑移动（类似Celeste手感）
    // Exit时停止脚步声

    public override void Enter()
    {
        base.Enter();
        AudioManager.instance.PlaySFX(SfxIds.Footsteps, null);
    }

    public override void Exit()
    {
        base.Exit();
        AudioManager.instance.StopSFX(SfxIds.Footsteps);
    }

    public override void Update()
    {
        base.Update();

        // ---- 平滑移动系统 ----
        // 计算目标速度与当前速度的差值，使用加速度或减速度平滑过渡
        float targetSpeed = xInput * player.moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? player.acceleration : player.deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);
        rb.velocity = new Vector2(rb.velocity.x + movement * Time.deltaTime, rb.velocity.y);

        // 根据输入方向自动翻转角色朝向
        if (xInput != 0)
        {
            player.FlipController(xInput);
        }

        // 没有输入或碰到墙壁时，切换回待机状态
        if (xInput == 0 || player.IsWallDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
