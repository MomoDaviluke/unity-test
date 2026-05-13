using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 表示敌人Skeleton的战斗状态类
public class SkeletonBattleState : EnemyState
{
    private Transform player;
    private Enemy_Skeleton enemy;
    private int moveDir;
    // 构造函数，初始化敌人状态机和动画布尔名称，并指定具体的Skeleton敌人
    public SkeletonBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // 进入战斗状态时执行的逻辑
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
        if (player.GetComponent<PlayerStats>().isDead)
        {
            stateMachine.ChangeState(enemy.moveState);
        }
    }

    // 退出战斗状态时执行的逻辑
    public override void Exit()
    {
        base.Exit();
    }

    // 战斗状态下的每一帧更新逻辑
    public override void Update()
    {
        base.Update();
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;
            float distanceToPlayer = Vector2.Distance(enemy.transform.position, player.position);

            // 新增：当距离小于攻击距离时停止移动
            if (distanceToPlayer < enemy.attackDistance - 0.3f)
            {
                enemy.SetZeroVelocity();
                stateMachine.ChangeState(enemy.attackState);
                return;
            }

            // 当检测到玩家距离在攻击距离内时，进行攻击
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }
            // 检测到玩家时使用追逐速度
            float currentSpeed = enemy.chaseSpeed;
            enemy.SetVelocity(currentSpeed * moveDir, rb.velocity.y);
        }
        else
        {
            // 未检测到玩家时使用移动速度
            enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
            // 当战斗计时器结束或玩家离开一定距离时，切换回空闲状态
            if (stateTimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 10)
            {
                stateMachine.ChangeState(enemy.idleState);
            }
        }

        // 根据玩家位置调整移动方向
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
        {
            moveDir = -1;
        }
    }
    // 判断是否可以攻击，根据攻击冷却时间来决定
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
