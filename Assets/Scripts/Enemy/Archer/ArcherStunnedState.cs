using UnityEngine;

/// <summary>
/// ArcherStunnedState（弓箭手眩晕状态）- Phase 4 迁移至共享基类
/// 闪烁/击退/计时归位全部在 EnemyStunnedStateBase，弓箭手无特有差异。
/// </summary>
public class ArcherStunnedState : EnemyStunnedStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Archer archer;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState IdleState => archer.idleState;

    // ==================== 构造函数 ====================
    public ArcherStunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        archer = enemy;
    }
}
