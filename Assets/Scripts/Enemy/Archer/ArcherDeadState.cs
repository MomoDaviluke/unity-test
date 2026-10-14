using UnityEngine;

/// <summary>
/// ArcherDeadState（弓箭手死亡状态）- Phase 4 迁移至共享基类
/// 停止移动 + 一次性调度销毁全部在 EnemyDeadStateBase，弓箭手无特有差异。
/// </summary>
public class ArcherDeadState : EnemyDeadStateBase
{
    // ==================== 构造函数 ====================
    public ArcherDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
    }
}
