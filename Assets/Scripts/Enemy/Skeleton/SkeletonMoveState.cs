using UnityEngine;

/// <summary>
/// SkeletonMoveState（骷髅移动状态）- Phase 4 迁移至共享基类
/// 巡逻移动、撞墙/边缘转身回待机全部在 EnemyMoveStateBase，骷髅无特有差异。
/// </summary>
public class SkeletonMoveState : EnemyMoveStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton skeleton;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => skeleton.battleState;
    protected override EnemyState IdleState => skeleton.idleState;

    // ==================== 构造函数 ====================
    public SkeletonMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        skeleton = enemy;
    }
}
