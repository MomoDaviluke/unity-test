using UnityEngine;

/// <summary>
/// SlimeBattleState（史莱姆战斗状态）- Phase 4 迁移至共享基类
/// 公共逻辑（Enter 缓存玩家、玩家死亡放弃、CanAttack）在 EnemyBattleStateBase；
/// 史莱姆特有：moveSpeed 逼近 + 实时面朝 Flip + 距离小于 1 时停止。
/// </summary>
public class SlimeBattleState : EnemyBattleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Slime slime;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState MoveState => slime.moveState;

    // ==================== 构造函数 ====================
    public SlimeBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        slime = enemy;
    }

    // ==================== 状态生命周期 ====================
    public override void Update()
    {
        base.Update();

        // ---- 检测到玩家：处理攻击逻辑 ----
        if (slime.IsPlayerDetected())
        {
            stateTimer = slime.battleTime;

            if (slime.IsPlayerDetected().distance < slime.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(slime.attackState);
            }
        }
        // ---- 丢失玩家或距离过远 ----
        else
        {
            if (stateTimer < 0 || Vector2.Distance(player.position, slime.transform.position) > 10)
                stateMachine.ChangeState(slime.idleState);
        }

        // ---- 面朝控制 ----
        if (player.position.x > slime.transform.position.x)
            moveDir = 1;
        else if (player.position.x < slime.transform.position.x)
            moveDir = -1;

        if (moveDir != slime.facingDir)
            slime.Flip();

        // ---- 移动控制：距离>1 时逼近，<=1 时停止 ----
        if (Vector2.Distance(player.position, slime.transform.position) > 1)
            slime.SetVelocity(slime.moveSpeed * slime.facingDir, rb.velocity.y);
        else
            slime.SetZeroVelocity();
    }
}
