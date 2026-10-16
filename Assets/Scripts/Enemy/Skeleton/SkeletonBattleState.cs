using UnityEngine;

/// <summary>
/// SkeletonBattleState（骷髅战斗状态）- Phase 4 迁移至共享基类
/// 公共逻辑（Enter 缓存玩家、玩家死亡放弃、CanAttack 冷却随机化）在 EnemyBattleStateBase；
/// 骷髅特有：chaseSpeed 追击、近距离直接攻击分支、丢失目标超时回待机。
/// </summary>
public class SkeletonBattleState : EnemyBattleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton skeleton;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState MoveState => skeleton.moveState;

    // ==================== 构造函数 ====================
    public SkeletonBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        skeleton = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Update：检测到玩家时追击并尝试攻击；未检测到时减速巡逻，超时或距离过远放弃追击
    public override void Update()
    {
        base.Update();

        // ---- 玩家检测到时：追击并尝试攻击 ----
        if (skeleton.IsPlayerDetected())
        {
            stateTimer = skeleton.battleTime;
            float distanceToPlayer = Vector2.Distance(skeleton.transform.position, player.position);

            // 距离极近时直接攻击（无需射线检测）
            if (distanceToPlayer < skeleton.attackDistance - 0.3f)
            {
                skeleton.SetZeroVelocity();
                stateMachine.ChangeState(skeleton.attackState);
                return;
            }

            // 射线检测到玩家且在攻击范围内，检查冷却
            if (skeleton.IsPlayerDetected().distance < skeleton.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(skeleton.attackState);
            }

            skeleton.SetVelocity(skeleton.chaseSpeed * moveDir, rb.linearVelocity.y);
        }
        // ---- 未检测到玩家：减速巡逻，超时或距离过远时放弃追击 ----
        else
        {
            skeleton.SetVelocity(skeleton.moveSpeed * moveDir, rb.linearVelocity.y);

            if (stateTimer < 0 || Vector2.Distance(player.position, skeleton.transform.position) > 10)
                stateMachine.ChangeState(skeleton.idleState);
        }

        // 根据玩家位置更新移动方向
        if (player.position.x > skeleton.transform.position.x)
            moveDir = 1;
        else if (player.position.x < skeleton.transform.position.x)
            moveDir = -1;
    }
}
