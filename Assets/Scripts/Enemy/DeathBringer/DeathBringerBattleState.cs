using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerBattleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;
    private Transform player;
    private int moveDir;

    // ==================== 构造函数 ====================
    public DeathBringerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时获取玩家引用
    // Update中分两种情况：
    //   1. 检测到玩家：重置战斗计时器，进入攻击距离时尝试攻击（冷却中则待机）
    //   2. 未检测到玩家：继续移动（DeathBringer没有巡逻状态，会一直追击）
    // 面朝方向始终朝向玩家

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.instance.player.transform;
    }

    public override void Update()
    {
        base.Update();

        // ---- 检测到玩家时：追击并尝试攻击 ----
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;

            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
                else
                    stateMachine.ChangeState(enemy.idleState);
            }
        }

        // 根据玩家位置更新面朝方向
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        // 已进入攻击范围时停止移动（避免穿模）
        if (enemy.IsPlayerDetected() && enemy.IsPlayerDetected().distance < enemy.attackDistance - .1f)
            return;

        enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }

    // ==================== 攻击冷却判定 ====================
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
