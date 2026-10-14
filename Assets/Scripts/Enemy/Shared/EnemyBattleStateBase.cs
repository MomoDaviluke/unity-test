using UnityEngine;

/// <summary>
/// EnemyBattleStateBase（战斗状态基类）- Phase 4 共享状态基类
/// 收敛内容：
/// - Enter 缓存玩家 + 玩家死亡放弃追击（常规敌人行为；Boss 覆写钩子关闭）；
/// - CanAttack() 攻击冷却判定（原 Skeleton/Slime/Archer/DeathBringer 四份完全相同的拷贝）。
/// 移动/攻击节奏差异大（Skeleton 追击、Slime 逼近、Archer 后跳射箭、DeathBringer 死追），
/// Update 模板不强行统一，由子类实现并调用基类 CanAttack。
/// </summary>
public abstract class EnemyBattleStateBase : EnemyState
{
    // ==================== 核心引用 ====================
    protected Enemy enemy;              // 敌人基类引用
    protected Transform player;         // 玩家 Transform（缓存）
    protected int moveDir;              // 移动方向（1 右 / -1 左）

    // ==================== 子类必须提供的状态 ====================
    protected abstract EnemyState MoveState { get; }   // 玩家死亡后返回的巡逻状态

    // ==================== 可覆写钩子 ====================
    // 玩家死亡是否放弃追击：常规敌人 true；Boss（DeathBringer）无此逻辑，覆写 false
    protected virtual bool GiveUpIfPlayerDead => true;

    // ==================== 构造函数 ====================
    protected EnemyBattleStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemyBase;
    }

    // ==================== 状态生命周期 ====================
    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.instance.player.transform;

        if (GiveUpIfPlayerDead && player.GetComponent<PlayerStats>().isDead)
            stateMachine.ChangeState(MoveState);
    }

    // ==================== 攻击冷却判定（四胞胎收敛为一份） ====================
    // 检查当前时间是否超过上次攻击 + 冷却；通过后随机化下次冷却，避免节奏机械化
    protected bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown)
        {
            enemy.attackCooldown = Random.Range(enemy.minAttackCooldown, enemy.maxAttackCooldown);
            enemy.lastTimeAttacked = Time.time;
            return true;
        }
        return false;
    }
}
