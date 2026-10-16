using UnityEngine;

/// <summary>
/// EnemyDeadStateBase（死亡状态基类）- Phase 4 共享状态基类
/// 消灭 Skeleton/Slime/Archer 三份复制：Enter 停止移动并一次性调度销毁（B10 修复后的形态）。
/// Boss（DeathBringer/Demon）死亡表现特化（动画冻结/击飞/无自动销毁），不继承本基类。
/// </summary>
public abstract class EnemyDeadStateBase : EnemyState
{
    // ==================== 核心引用 ====================
    protected Enemy enemy;

    // ==================== 构造函数 ====================
    protected EnemyDeadStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemyBase;
    }

    // ==================== 状态生命周期 ====================
    public override void Enter()
    {
        base.Enter();
        rb.linearVelocity = Vector2.zero;   // 停止所有移动

        // 修复 B10：销毁调度只做一次（Enter 仅执行一次），给死亡动画 2 秒播放时间
        GameObject.Destroy(enemy.gameObject, 2f);
    }
}
