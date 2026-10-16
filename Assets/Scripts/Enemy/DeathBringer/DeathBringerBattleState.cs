using UnityEngine;

/// <summary>
/// DeathBringerBattleState（死亡使者战斗状态）- Phase 4 收敛 CanAttack 至共享基类
/// 复用 EnemyBattleStateBase 的 CanAttack 与玩家缓存；
/// Boss 特有：无"玩家死亡放弃"逻辑（GiveUpIfPlayerDead=false，原 Enter 不含 isDead 检查）、
/// 冷却中转入待机、无巡逻状态。移动/攻击节奏逻辑保留在本类。
/// </summary>
public class DeathBringerBattleState : EnemyBattleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer deathBringer;

    // ==================== 基类钩子实现 ====================
    // Boss 不巡逻：玩家死亡不返回移动状态（保持原行为）
    protected override bool GiveUpIfPlayerDead => false;
    // 本钩子在 GiveUpIfPlayerDead=false 时不会被调用，返回待机状态仅为满足抽象契约
    protected override EnemyState MoveState => deathBringer.idleState;

    // ==================== 构造函数 ====================
    public DeathBringerBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        deathBringer = _enemy;
    }

    // ==================== 状态生命周期 ====================
    public override void Update()
    {
        base.Update();

        // ---- 检测到玩家时：追击并尝试攻击（冷却中则转入待机） ----
        if (deathBringer.IsPlayerDetected())
        {
            stateTimer = deathBringer.battleTime;

            if (deathBringer.IsPlayerDetected().distance < deathBringer.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(deathBringer.attackState);
                else
                    stateMachine.ChangeState(deathBringer.idleState);
            }
        }

        // 根据玩家位置更新面朝方向
        if (player.position.x > deathBringer.transform.position.x)
            moveDir = 1;
        else if (player.position.x < deathBringer.transform.position.x)
            moveDir = -1;

        // 已进入攻击范围时停止移动（避免穿模）
        if (deathBringer.IsPlayerDetected() && deathBringer.IsPlayerDetected().distance < deathBringer.attackDistance - .1f)
            return;

        deathBringer.SetVelocity(deathBringer.moveSpeed * moveDir, rb.linearVelocity.y);
    }
}
