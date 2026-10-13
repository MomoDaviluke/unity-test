using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonAttackState（骷髅攻击状态）- 继承自EnemyState
/// 功能：执行攻击动画，动画结束后返回战斗状态
/// 设计思路：攻击期间停止移动，通过动画事件触发伤害判定
/// </summary>
public class SkeletonAttackState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton enemy; // 具体的骷髅敌人引用

    // ==================== 构造函数 ====================
    public SkeletonAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时调用父类逻辑初始化
    // Update中保持静止（SetZeroVelocity），等待动画事件触发后返回战斗状态
    // Exit时记录攻击时间，用于计算下次攻击冷却

    public override void Enter()
    {
        base.Enter();
    }

    /// <summary>
    /// 退出攻击状态：记录攻击时间（用于计算冷却）
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        enemy.lastTimeAttacked = Time.time;
    }

    /// <summary>
    /// 更新攻击状态：保持静止，动画完成后返回战斗状态
    /// </summary>
    public override void Update()
    {
        base.Update();
        enemy.SetZeroVelocity(); // 攻击期间停止移动

        // 动画触发完成（攻击后摇结束），返回战斗状态
        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
