using UnityEngine;

/// <summary>
/// SlimeMoveState（史莱姆移动状态）- Phase 4 迁移至共享基类
/// 公共逻辑在 EnemyMoveStateBase；特有差异：仇恨距离硬编码 2.5。
/// 注：原实现撞墙时设 stateTimer = 0.2f，但 IdleState.Enter 立即用 idleTime 覆盖，
/// 属死代码，迁移时删除（行为等价）。
/// </summary>
public class SlimeMoveState : EnemyMoveStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Slime slime;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => slime.battleState;
    protected override EnemyState IdleState => slime.idleState;
    protected override float AgroDistance => 2.5f;   // 原 SlimeGroundedState 硬编码值（Phase 7 可数据化）

    // ==================== 构造函数 ====================
    public SlimeMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        slime = enemy;
    }
}
