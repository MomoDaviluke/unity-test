using UnityEngine;

/// <summary>
/// SlimeIdleState（史莱姆待机状态）- Phase 4 迁移至共享基类
/// 公共逻辑在 EnemyIdleStateBase；史莱姆无 Enter/Exit 附加差异，
/// 特有差异：原实现硬编码仇恨距离 2.5（非 agroDistance 字段），覆写保持行为等价。
/// </summary>
public class SlimeIdleState : EnemyIdleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Slime slime;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => slime.battleState;
    protected override EnemyState MoveState => slime.moveState;
    protected override float AgroDistance => 2.5f;   // 原 SlimeGroundedState 硬编码值（Phase 7 可数据化）

    // ==================== 构造函数 ====================
    public SlimeIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        slime = enemy;
    }
}
