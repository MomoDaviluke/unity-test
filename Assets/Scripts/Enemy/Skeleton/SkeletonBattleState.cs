using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonBattleState（骷髅战斗状态）- 追击玩家并攻击
/// </summary>
public class SkeletonBattleState : EnemyState
{
    // ==================== 核心引用 ====================
    // player缓存玩家位置，enemy指向具体骷髅，moveDir记录移动方向（1右/-1左）
    private Transform player;
    private Enemy_Skeleton enemy;
    private int moveDir;

    // ==================== 构造函数 ====================
    public SkeletonBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时获取玩家引用，若玩家已死亡则返回移动状态
    // Update中分两种情况：检测到玩家时追击并尝试攻击，未检测到时返回巡逻
    // 通过CanAttack检查冷却时间，随机化下次攻击间隔增加变化性

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
        if (player.GetComponent<PlayerStats>().isDead)
            stateMachine.ChangeState(enemy.moveState);
    }

    public override void Update()
    {
        base.Update();

        // ---- 玩家检测到时：追击并尝试攻击 ----
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;
            float distanceToPlayer = Vector2.Distance(enemy.transform.position, player.position);

            // 距离极近时直接攻击（无需射线检测）
            if (distanceToPlayer < enemy.attackDistance - 0.3f)
            {
                enemy.SetZeroVelocity();
                stateMachine.ChangeState(enemy.attackState);
                return;
            }

            // 射线检测到玩家且在攻击范围内，检查冷却
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }

            enemy.SetVelocity(enemy.chaseSpeed * moveDir, rb.velocity.y);
        }
        // ---- 未检测到玩家：减速巡逻，超时或距离过远时放弃追击 ----
        else
        {
            enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);

            if (stateTimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 10)
                stateMachine.ChangeState(enemy.idleState);
        }

        // 根据玩家位置更新面朝方向
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;
    }

    // ==================== 攻击冷却判定 ====================
    // 检查当前时间是否超过上次攻击时间+冷却时间
    // 通过随机范围设置下次冷却，避免攻击节奏过于规律
    private bool CanAttack()
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
