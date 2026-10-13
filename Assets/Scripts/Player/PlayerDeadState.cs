using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerDeadState（玩家死亡状态）- 继承自PlayerState
/// 功能：玩家死亡时进入的状态，停止移动并显示结束画面
/// </summary>
public class PlayerDeadState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerDeadState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 显示游戏结束画面
    // Update: 保持静止

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }

    public override void Enter()
    {
        base.Enter();
        // 修复 B9：改用 UI 单例，不再 GameObject.Find("Canvas")（Canvas 改名即崩、运行时查找昂贵）
        if (UI.instance != null)
            UI.instance.SwitchOnEndScreen();
        else
            Debug.LogError("[PlayerDeadState] UI.instance 为空，无法显示结束画面");
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();
    }
}
