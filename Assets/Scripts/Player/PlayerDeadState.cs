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
        // Phase 3 解耦（B9 最终形态）：死亡经事件总线广播，UI 订阅显示结束画面；
        // Player 状态层不再直接引用 UI（依赖方向：逻辑层 → 总线 ← 表现层）
        GameEvents.PlayerDead();
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
