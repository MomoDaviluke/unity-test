using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyState（敌人状态基类）- 所有敌人AI状态的父类
/// 与PlayerState结构类似，但处理AI逻辑而非玩家输入
/// </summary>
public class EnemyState
{
    // ==================== 核心引用 ====================
    // 状态机引用用于状态切换，enemyBase获取敌人组件，rb用于物理控制
    protected EnemyStateMachine stateMachine;   // 状态机引用（用于切换状态）
    protected Enemy enemyBase;                  // 敌人引用
    protected Rigidbody2D rb;                   // 刚体引用

    // ==================== 状态控制变量 ====================
    // triggerCalled由动画事件设置，标记动画是否播放完毕
    // animBoolName对应Animator中的布尔参数名，用于控制动画切换
    // stateTimer用于状态持续时间倒计时（如攻击后摇、眩晕时间）
    protected bool triggerCalled;               // 动画是否已触发完成
    private string animBoolName;                // 动画布尔参数名
    protected float stateTimer;                 // 状态持续时间计时器

    // ==================== 构造函数 ====================
    // 保存敌人、状态机和动画参数名的引用，供子类使用
    public EnemyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
    {
        this.enemyBase = _enemyBase;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    // ==================== 状态生命周期 ====================
    // Update每帧调用，子类重写实现具体AI逻辑（如检测玩家、移动、攻击判定）
    // Enter在状态开始时调用，初始化变量并播放动画
    // Exit在状态结束时调用，清理并停止动画
    // AnimationFinishTrigger由动画事件调用，标记动画播放完成

    // 每帧更新：更新计时器（子类重写实现AI逻辑）
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime; // 倒计时
    }

    // 进入状态时调用：播放对应动画，初始化变量
    public virtual void Enter()
    {
        triggerCalled = false;                      // 重置动画触发标记
        rb = enemyBase.rb;                          // 获取刚体引用
        enemyBase.anim.SetBool(animBoolName, true); // 设置动画参数为true
    }

    // 退出状态时调用：停止对应动画
    public virtual void Exit()
    {
        enemyBase.anim.SetBool(animBoolName, false); // 设置动画参数为false
    }

    // 动画事件触发时调用（由动画关键帧调用）
    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
