using UnityEngine;

/// <summary>
/// SkeletonIdleState（骷髅待机状态）- Phase 4 迁移至共享基类
/// 公共逻辑（计时待机、玩家检测、切移动状态）在 EnemyIdleStateBase；
/// 骷髅特有：Enter 关闭反击窗口、Exit 播放音效 24。
/// </summary>
public class SkeletonIdleState : EnemyIdleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton skeleton;   // 类型化引用（访问骷髅特有状态属性）

    // ==================== 基类钩子实现 ====================
    protected override EnemyState BattleState => skeleton.battleState;
    protected override EnemyState MoveState => skeleton.moveState;

    // ==================== 构造函数 ====================
    public SkeletonIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        skeleton = enemy;
    }

    // ==================== 骷髅特有逻辑 ====================
    protected override void OnEnterExtra()
    {
        skeleton.CloseCountAttackWindow();
    }

    protected override void OnExitExtra()
    {
        AudioManager.instance.PlaySFX(24, skeleton.transform);
    }
}
