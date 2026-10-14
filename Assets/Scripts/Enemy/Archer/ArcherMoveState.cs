using UnityEngine;

/// <summary>
/// ArcherMoveState（弓箭手移动状态）- Phase 4 迁移至共享基类
/// 巡逻移动、撞墙转身回待机全部在 EnemyMoveStateBase，弓箭手无特有差异。
/// </summary>
public class ArcherMoveState : EnemyMoveStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Archer archer;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => archer.battleState;
    protected override EnemyState IdleState => archer.idleState;

    // ==================== 构造函数 ====================
    public ArcherMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        archer = enemy;
    }
}
