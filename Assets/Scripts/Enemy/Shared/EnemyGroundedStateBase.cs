using UnityEngine;

/// <summary>
/// EnemyGroundedStateBase（地面巡逻状态基类）- Phase 4 共享状态基类
/// 消灭 SkeletonGroundState / SlimeGroundedState / ArcherGroundState 三份复制：
/// Enter 缓存玩家；Update 射线检测或距离小于仇恨距离时切换战斗状态。
/// 差异钩子：BattleState 抽象属性（各敌人战斗状态不同）、AgroDistance 虚属性
/// （Slime 原实现硬编码 2.5，子类覆写保持行为等价）。
/// </summary>
public abstract class EnemyGroundedStateBase : EnemyState
{
    // ==================== 核心引用 ====================
    protected Enemy enemy;          // 敌人基类引用（公共字段：agroDistance/moveSpeed 等）
    protected Transform player;     // 玩家 Transform（缓存避免每帧查找）

    // ==================== 子类必须提供的状态 ====================
    protected abstract EnemyState BattleState { get; }   // 检测到玩家后切换的战斗状态

    // ==================== 可覆写钩子 ====================
    // 仇恨距离：默认读字段；Slime 原实现硬编码 2.5，由其子类覆写保持行为
    protected virtual float AgroDistance => enemy.agroDistance;

    // ==================== 构造函数 ====================
    protected EnemyGroundedStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemyBase;
    }

    // ==================== 状态生命周期 ====================
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
    }

    public override void Update()
    {
        base.Update();

        // 射线检测到玩家、或玩家进入仇恨距离 → 切换战斗状态
        if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.position) < AgroDistance)
            stateMachine.ChangeState(BattleState);
    }
}
