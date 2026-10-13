using System.Collections;
using UnityEngine;

public class ArcherBattleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Transform player;
    private Enemy_Archer enemy;
    private int moveDir;

    // ==================== 构造函数 ====================
    public ArcherBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时获取玩家引用，若玩家已死亡则返回移动状态
    // Update中处理三种情况：
    //   1. 玩家过近（<安全距离）：后跳拉开距离
    //   2. 玩家进入攻击距离：射箭攻击
    //   3. 持续面朝玩家

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

        // ---- 检测到玩家时：处理距离逻辑 ----
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;

            // 玩家过近（小于安全距离）：尝试后跳拉开距离
            if (enemy.IsPlayerDetected().distance < enemy.safeDistance)
            {
                if (CanJump())
                    stateMachine.ChangeState(enemy.ArcherJumpState);
            }

            // 玩家进入攻击距离：尝试射箭
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);
            }
        }

        // 始终面朝玩家
        BattleStateFlipController();
    }

    // ==================== 面朝控制 ====================
    // 确保弓箭手始终面向玩家
    private void BattleStateFlipController()
    {
        if (player.position.x > enemy.transform.position.x && enemy.facingDir == -1)
            enemy.Flip();
        else if (player.position.x < enemy.transform.position.x && enemy.facingDir == 1)
            enemy.Flip();
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

    // ==================== 跳跃（后跳）判定 ====================
    // 检查身后是否有地面和墙壁，确保安全后才允许后跳
    // 同时检查跳跃冷却时间
    private bool CanJump()
    {
        // 安全检查：身后无地面或有墙壁时不允许后跳
        if (enemy.GroundBenhundCheck() == false || enemy.WallBehind() == true)
            return false;

        if (Time.time >= enemy.lastTimeJumped + enemy.jumpCooldown)
        {
            enemy.lastTimeJumped = Time.time;
            return true;
        }

        return false;
    }
}
