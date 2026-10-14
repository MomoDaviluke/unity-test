using UnityEngine;

/// <summary>
/// ArcherIdleState（弓箭手待机状态）- Phase 4 迁移至共享基类
/// 公共逻辑在 EnemyIdleStateBase；弓箭手特有：Exit 播放音效 24。
/// </summary>
public class ArcherIdleState : EnemyIdleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Archer archer;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => archer.battleState;
    protected override EnemyState MoveState => archer.moveState;

    // ==================== 构造函数 ====================
    public ArcherIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        archer = enemy;
    }

    // ==================== 弓箭手特有逻辑 ====================
    protected override void OnExitExtra()
    {
        AudioManager.instance.PlaySFX(24, archer.transform);
    }
}
