using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonDeadState（骷髅死亡状态）- 继承自EnemyState
/// 功能：播放死亡动画，延迟销毁敌人对象
/// 设计思路：死亡后停止物理，延迟销毁给玩家反馈时间
/// </summary>
public class SkeletonDeadState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton enemy; // 具体的骷髅敌人引用

    // ==================== 构造函数 ====================
    public SkeletonDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止所有物理移动
    // Update中延迟2秒销毁敌人对象，给死亡动画足够的播放时间

    /// <summary>
    /// 进入死亡状态：停止移动
    /// </summary>
    public override void Enter()
    {
        base.Enter();
        rb.velocity = Vector2.zero; // 停止所有移动
    }

    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>
    /// 更新死亡状态：延迟销毁敌人对象
    /// </summary>
    public override void Update()
    {
        base.Update();

        // 2秒后销毁敌人对象（给死亡动画播放时间）
        GameObject.Destroy(enemy.gameObject, 2f);
    }
}
