using UnityEngine;

/// <summary>
/// SlimeDeathState（史莱姆死亡状态）- Phase 4 迁移至共享基类
/// 停止移动 + 一次性调度销毁全部在 EnemyDeadStateBase，史莱姆无特有差异。
/// </summary>
public class SlimeDeathState : EnemyDeadStateBase
{
    // ==================== 构造函数 ====================
    public SlimeDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
    }
}
