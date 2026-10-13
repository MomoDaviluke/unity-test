using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerIdleState（玩家待机状态）- 继承自PlayGroundedState
/// 功能：玩家静止不动时的状态，检测输入切换到移动/攻击/跳跃等状态
/// </summary>
public class PlayerIdleState : PlayGroundedState
{
    // ==================== 构造函数 ====================
    public PlayerIdleState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止移动
    // Update中检测输入：贴墙且向墙移动时保持待机；有水平输入且不忙碌时切换到移动

    public override void Enter()
    {
        base.Enter();
        player.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 贴墙且向墙壁方向移动时保持待机（防止穿墙）
        if (player.IsWallDetected() && xInput * player.facingDir > 0)
            return;

        // 有水平输入且不在忙碌状态，切换到移动状态
        if(xInput !=0 && !player.isBusy)
            stateMachine.ChangeState(player.moveState);
    }
}
