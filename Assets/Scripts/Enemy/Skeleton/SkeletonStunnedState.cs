using UnityEngine;

/// <summary>
/// SkeletonStunnedState（骷髅眩晕状态）- Phase 4 迁移至共享基类
/// 闪烁、击退、计时归位全部在 EnemyStunnedStateBase，骷髅无特有差异。
/// </summary>
public class SkeletonStunnedState : EnemyStunnedStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton skeleton;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState IdleState => skeleton.idleState;

    // ==================== 构造函数 ====================
    public SkeletonStunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        skeleton = enemy;
    }
}
