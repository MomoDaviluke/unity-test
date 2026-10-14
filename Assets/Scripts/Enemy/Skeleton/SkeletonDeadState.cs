using UnityEngine;

/// <summary>
/// SkeletonDeadState（骷髅死亡状态）- Phase 4 迁移至共享基类
/// 停止移动 + 一次性调度销毁全部在 EnemyDeadStateBase，骷髅无特有差异。
/// </summary>
public class SkeletonDeadState : EnemyDeadStateBase
{
    // ==================== 构造函数 ====================
    public SkeletonDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
    }
}
