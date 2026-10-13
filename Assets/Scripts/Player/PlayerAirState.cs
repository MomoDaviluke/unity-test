using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerAirState（玩家空中状态）- 继承自PlayerState
/// 功能：处理玩家在空中时的行为，包括空中移动控制、墙壁检测、闪避
/// 设计思路：空中移动使用较低的控制倍率，模拟惯性效果
/// </summary>
public class PlayerAirState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Update中处理：
    //   1. 着陆检测（回待机）
    //   2. 墙壁检测（回滑墙）
    //   3. 空中移动（使用较低的控制倍率模拟惯性）
    //   4. 空中闪避

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

        // ---- 着陆检测 ----
        if (player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }

        // ---- 墙壁检测 ----
        if (player.IsWallDetected())
        {
            stateMachine.ChangeState(player.wallSlideState);
        }

        // ---- 空中移动控制 ----
        // 使用加速系统但应用空中控制倍率，模拟惯性效果
        if (xInput != 0)
        {
            float targetSpeed = xInput * player.moveSpeed * player.airControlMultiplier;
            float speedDiff = targetSpeed - rb.velocity.x;
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * player.acceleration * player.airControlMultiplier, 0.9f) * Mathf.Sign(speedDiff);
            rb.velocity = new Vector2(rb.velocity.x + movement * Time.deltaTime, rb.velocity.y);
            player.FlipController(xInput);
        }

        // ---- 空中闪避 ----
        if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time - player.lastDodgeTime > player.dodgeCooldown)
        {
            stateMachine.ChangeState(player.dodgeState);
        }
    }
}
