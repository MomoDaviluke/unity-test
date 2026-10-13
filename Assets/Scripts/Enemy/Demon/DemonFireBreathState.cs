using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonFireBreathState（恶魔Boss喷火状态）- 对前方玩家造成持续火焰伤害
/// </summary>
public class DemonFireBreathState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;

    // ==================== 构造函数 ====================
    public DemonFireBreathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止移动
    // Update中动画完成后返回行走状态

    public override void Enter()
    {
        base.Enter();
        demon.SetZeroVelocity();
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled)
            demon.SkillFinished();
    }

    // ==================== 喷火触发 ====================
    // 由动画事件调用，触发喷火技能
    public void TriggerFireBreath()
    {
        demon.CastFireBreath();
    }
}
