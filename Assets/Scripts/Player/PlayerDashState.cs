using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerDashState（玩家冲刺状态）- 继承自PlayerState
/// 功能：快速水平位移并无敌，可穿越敌人
/// 设计思路：冲刺期间锁定垂直速度，提供短暂无敌帧
/// </summary>
public class PlayerDashState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 创建冲刺分身、设置无敌和持续时间
    // Update: 保持冲刺速度、检测墙壁和计时结束、创建残影
    // Exit: 创建到达分身、停止水平移动、取消无敌

    public override void Enter()
    {
        base.Enter();
        player.skill.dash.CloneOnDash();
        stateTimer = player.dashDuration;
        player.stats.MakeInvincible(true);
    }

    public override void Exit()
    {
        base.Exit();
        player.skill.dash.CloneOnArrival();
        player.SetVelocity(0, rb.linearVelocity.y);
        player.stats.MakeInvincible(false);
    }

    public override void Update()
    {
        base.Update();

        // 空中碰到墙壁时切换到滑墙状态
        if (!player.IsGroundDetected() && player.IsWallDetected())
        {
            stateMachine.ChangeState(player.wallSlideState);
        }

        // 保持冲刺速度（水平方向）
        player.SetVelocity(player.dashSpeed * player.dashDir, 0);

        // 冲刺时间结束，切换回待机状态
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.idleState);
        }

        // 创建残影特效
        player.fX.CreateAfterImage();
    }
}
