using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerPrimaryAttackState（玩家普通攻击状态）- 继承自PlayerState
/// 功能：处理三段连击攻击，每段有不同的位移和伤害
/// 设计思路：使用comboCounter记录当前段数，超过时间窗口则重置
/// </summary>
public class PlayerPrimaryAttackState : PlayerState
{
    // ==================== 连击参数 ====================
    public int comboCounter { get; private set; } // 当前连击段数（0-2）
    private float lastTimeAttacked; // 上次攻击时间
    private float comboWindow = 2;  // 连击时间窗口

    // ==================== 构造函数 ====================
    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 重置连击计数（超时后）、设置攻击方向和位移
    // Update: 前摇结束后停止移动，动画完成后切换状态
    // Exit: 增加连击计数、记录攻击时间、进入忙碌状态

    public override void Enter()
    {
        base.Enter();

        xInput = 0; // 攻击时清除水平输入

        // 连击重置条件：超过3段或超出时间窗口
        if (comboCounter > 2 || Time.time >= lastTimeAttacked + comboWindow)
        {
            comboCounter = 0;
        }
        player.anim.SetInteger("ComboCounter", comboCounter);

        // ---- 选择攻击方向 ----
        // 优先使用输入方向，无输入则使用当前朝向
        float attackDir = player.facingDir;
        if (xInput != 0)
            attackDir = xInput;
        else
            attackDir = player.facingDir;

        // 应用攻击位移：每段攻击有不同的向前突进距离
        player.SetVelocity(player.attackMovement[comboCounter].x * attackDir, player.attackMovement[comboCounter].y);

        stateTimer = .1f; // 攻击前摇时间
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine("BusyFor", .12f); // 攻击后短暂无法移动
        comboCounter++;
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        // 前摇时间结束后停止移动
        if (stateTimer < 0)
        {
            player.SetZeroVelocity();
        }

        // 动画触发完成（攻击后摇结束），切换回待机状态
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
