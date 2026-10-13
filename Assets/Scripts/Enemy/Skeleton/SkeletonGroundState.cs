using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonGroundState（骷髅地面状态基类）- 继承自EnemyState
/// 功能：骷髅在地面时的共同逻辑（检测玩家并切换到战斗状态）
/// 子类：SkeletonIdleState, SkeletonMoveState
/// </summary>
public class SkeletonGroundState : EnemyState
{
    // ==================== 核心引用 ====================
    // enemy指向具体的骷髅敌人，player缓存玩家Transform避免每帧查找
    protected Enemy_Skeleton enemy; // 具体的骷髅敌人引用
    protected Transform player; // 玩家Transform（缓存以提高性能）

    // ==================== 构造函数 ====================
    // 调用父类构造函数保存基础引用，额外保存骷髅敌人引用
    public SkeletonGroundState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时缓存玩家引用，Update时检测玩家是否在仇恨范围内
    // 检测到玩家或距离小于agroDistance时切换到战斗状态

    /// <summary>
    /// 进入地面状态 - 获取玩家引用
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>
    /// 更新地面状态 - 检测玩家是否进入仇恨范围
    /// </summary>
    public override void Update()
    {
        base.Update();
        // 如果检测到玩家或玩家接近，立即进入战斗状态
        float distanceToPlayer = Vector2.Distance(enemy.transform.position, player.position);
        if (enemy.IsPlayerDetected() || distanceToPlayer < enemy.agroDistance)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
