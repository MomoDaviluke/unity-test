using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBattleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Slime enemy;
    private Transform player;
    private int moveDir;

    // ==================== 构造函数 ====================
    public SlimeBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时获取玩家引用，若玩家已死亡则返回移动状态
    // Update中处理：
    //   1. 检测到玩家时重置战斗计时器，进入攻击距离则攻击
    //   2. 丢失玩家或距离过远则返回待机
    //   3. 始终面朝玩家，距离>1时移动，<=1时停止

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

        // ---- 检测到玩家：处理攻击逻辑 ----
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;

            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }
        }
        // ---- 丢失玩家或距离过远 ----
        else
        {
            if (stateTimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 10)
                stateMachine.ChangeState(enemy.idleState);
        }

        // ---- 面朝控制 ----
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        if (moveDir != enemy.facingDir)
            enemy.Flip();

        // ---- 移动控制 ----
        // 距离>1时向玩家移动，<=1时停止
        if (Vector2.Distance(player.transform.position, enemy.transform.position) > 1)
            enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);
        else
            enemy.SetZeroVelocity();
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
        else
            return false;
    }
}
