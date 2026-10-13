using System.Collections;
using UnityEngine;

public class ShadyBattleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Transform player;
    private Enemy_Shady enemy;
    private int moveDir;
    private float defaultSpeed; // 记录进入战斗前的默认速度，Exit时恢复

    // ==================== 构造函数 ====================
    public ShadyBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Shady _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时保存默认速度并切换到战斗速度，若玩家已死亡则返回移动状态
    // Update中追击玩家，进入攻击距离时直接自杀（触发爆炸）
    // Exit时恢复默认移动速度

    public override void Enter()
    {
        base.Enter();

        defaultSpeed = enemy.moveSpeed;
        enemy.moveSpeed = enemy.battleStateMoveSpeed;

        player = PlayerManager.instance.player.transform;

        if (player.GetComponent<PlayerStats>().isDead)
            stateMachine.ChangeState(enemy.moveState);
    }

    public override void Update()
    {
        base.Update();

        // ---- 检测到玩家时：高速追击，进入攻击范围则自爆 ----
        if (enemy.IsPlayerDetected())
        {
            stateTimer = enemy.battleTime;

            // Shady的特殊机制：进入攻击距离直接自杀，触发死亡状态的爆炸
            if (enemy.IsPlayerDetected().distance < enemy.attackDistance)
                enemy.stats.KillEntity(); // 进入死亡状态，触发自爆+掉落
        }
        // ---- 未检测到玩家：超时或距离过远时放弃追击 ----
        else
        {
            if (stateTimer < 0 || Vector2.Distance(player.transform.position, enemy.transform.position) > 10)
            {
                stateMachine.ChangeState(enemy.idleState);
            }
        }

        // 根据玩家位置更新面朝方向
        if (player.position.x > enemy.transform.position.x)
            moveDir = 1;
        else if (player.position.x < enemy.transform.position.x)
            moveDir = -1;

        enemy.SetVelocity(enemy.moveSpeed * moveDir, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        enemy.moveSpeed = defaultSpeed;
    }

    // ==================== 攻击冷却判定（预留） ====================
    // 当前Shady没有使用此函数，但保留以备后续扩展
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
