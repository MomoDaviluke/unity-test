using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBattleState : EnemyState
{
    private Enemy_Slime enemy; // 史莱姆敌人引用
    private Transform player; // 玩家位置引用
    private int moveDir; // 移动方向

    // 构造函数：初始化战斗状态
    public SlimeBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // 进入战斗状态
    public override void Enter()
    {
        base.Enter();

        // 获取玩家引用
        player = PlayerManager.instance.player.transform;

        // 如果玩家已死亡，则退出战斗状态
        if (player.GetComponent<PlayerStats>().isDead)
            stateMachine.ChangeState(enemy.moveState);
    }

    // 战斗状态更新
    public override void Update()
    {
        base.Update();

        // 检测玩家，更新战斗计时器
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime; // 重置战斗计时器

            // 如果在攻击范围内且可以攻击，则切换到攻击状态
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }
        }
        else
        {
            // 如果超出战斗时间或距离过远，则退出战斗状态
            if (stateTimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 10)
                stateMachine.ChangeState(enemy.idleState);
        }

        // 确定玩家相对位置，设置移动方向
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        // 如果朝向与移动方向不一致，则翻转敌人
        if (moveDir != enemy.facingDir)
            enemy.Flip();

        // 根据与玩家的距离决定是移动还是停止
        if (Vector2.Distance(player.transform.position, enemy.transform.position) > 1)
            enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.velocity.y);
        else
            enemy.SetZeroVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 检查是否可以攻击（基于冷却时间）
    private bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttacked + enemy.attackCooldown)
        {
            // 随机设置下次攻击冷却时间
            enemy.attackCooldown = Random.Range(enemy.minAttackCooldown, enemy.maxAttackCooldown);
            enemy.lastTimeAttacked = Time.time;
            return true;
        }
        else
            return false;
    }
}
