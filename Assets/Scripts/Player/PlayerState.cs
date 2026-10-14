using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerState（玩家状态基类）- 所有玩家状态的父类
/// 子类重写Enter/Update/Exit实现具体行为（Idle/Move/Jump/Attack等）
/// </summary>
public class PlayerState
{
    // ==================== 核心引用 ====================
    protected PlayerStateMachine stateMachine;
    protected Player player;
    protected Rigidbody2D rb;

    // ==================== 输入参数 ====================
    protected float xInput; // 水平输入（-1=左，0=无，1=右）
    protected float yInput; // 垂直输入
    private string animBoolName; // 动画布尔参数名

    // ==================== 状态控制 ====================
    protected float stateTimer; // 状态持续时间计时器
    protected bool triggerCalled; // 动画是否已触发完成

    // ==================== 构造函数 ====================
    public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
    {
        this.player = _player;
        this.stateMachine = _stateMachine;
        this.animBoolName = _animBoolName;
    }

    // ==================== 状态生命周期 ====================
    // Enter: 设置动画参数为true，获取刚体引用，重置触发标记
    // Update: 倒计时、读取输入、更新动画Y轴速度
    // Exit: 设置动画参数为false

    public virtual void Enter()
    {
        player.anim.SetBool(animBoolName, true);
        rb = player.rb;
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        // Phase 5：输入经 InputReader 集中读取（改键在 Inspector 生效）
        xInput = InputReader.instance.MoveX;
        yInput = InputReader.instance.MoveY;
        player.anim.SetFloat("yVelocity", rb.velocity.y);
    }

    public virtual void Exit()
    {
        player.anim.SetBool(animBoolName, false);
    }

    // ==================== 动画事件回调 ====================
    // 由动画关键帧调用，标记动画已完成
    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}
