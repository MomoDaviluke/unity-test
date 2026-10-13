using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyStateMachine（敌人状态机）- 管理敌人AI所有状态的切换
/// 与PlayerStateMachine结构相同，但管理EnemyState类型
/// </summary>
public class EnemyStateMachine
{
    // ==================== 当前状态存储 ====================
    // 使用属性封装，外部只读，防止外部直接修改导致状态不一致
    public EnemyState currentState { get; private set; } // 当前状态

    // ==================== 状态机控制接口 ====================
    // Initialize用于启动状态机，设置初始状态并调用Enter
    // ChangeState用于切换状态：先Exit当前状态，再Enter新状态，确保状态转换的原子性

    // 初始化状态机，设置起始状态
    public void Initialize(EnemyState _startState)
    {
        currentState = _startState;
        currentState.Enter(); // 调用进入逻辑
    }

    // 切换到新状态（先退出当前状态，再进入新状态）
    public void ChangeState(EnemyState _newState)
    {
        if (_newState == null) return; // 安全检查
        currentState.Exit();  // 退出当前状态
        currentState = _newState;
        currentState.Enter(); // 进入新状态
    }
}
