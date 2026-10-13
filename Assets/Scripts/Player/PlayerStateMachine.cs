using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerStateMachine（玩家状态机）- 管理玩家所有状态的切换
/// 使用状态模式（State Pattern）：每个行为封装为独立类，通过切换状态改变玩家行为
/// </summary>
public class PlayerStateMachine
{
    // ==================== 当前状态 ====================
    public PlayerState currentState { get; private set; }

    // ==================== 状态机操作 ====================
    // Initialize: 设置起始状态并调用Enter
    // ChangeState: 先Exit当前状态，再Enter新状态

    public void Initialize(PlayerState _startState)
    {
        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(PlayerState _newState)
    {
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}
