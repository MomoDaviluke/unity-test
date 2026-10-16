using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerDodgeState（玩家闪避状态）- 继承自PlayerState
/// 功能：快速向指定方向闪避，期间可以穿过敌人
/// 设计思路：闪避提供短暂无敌和位移，用于躲避敌人攻击
/// </summary>
public class PlayerDodgeState : PlayerState
{
    // ==================== 闪避参数 ====================
    private float dodgeDirection; // 闪避方向：1=右，-1=左

    // ==================== 构造函数 ====================
    public PlayerDodgeState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 设置闪避方向（优先输入方向，无输入则用朝向）、记录冷却时间、应用速度、创建残影
    // Update: 保持闪避速度并持续创建残影，时间结束后根据状态返回对应状态

    public override void Enter()
    {
        base.Enter();

        stateTimer = player.dodgeDuration;

        // 直接读取当前输入，避免使用上一帧的输入值
        float currentXInput = InputReader.instance.MoveX;

        // 记录当前输入方向作为闪避方向，无输入则使用朝向方向
        dodgeDirection = currentXInput != 0 ? currentXInput : player.facingDir;

        // 记录闪避时间（用于计算冷却）
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
        player.SetVelocity(player.dodgeSpeed * dodgeDirection, rb.linearVelocity.y);

        // 闪避时持续创建残影
        player.fX.CreateAfterImage();

        // 闪避时间结束后根据状态返回相应状态
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
