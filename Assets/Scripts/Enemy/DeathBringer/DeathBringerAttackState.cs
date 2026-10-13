using System.Collections;
using UnityEngine;

public class DeathBringerAttackState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;

    // ==================== 构造函数 ====================
    public DeathBringerAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时增加5%传送概率（逐步累加，增加战斗变化性）
    // Update中保持静止，动画完成后判定是否传送
    // Exit时记录攻击时间

    public override void Enter()
    {
        base.Enter();

        enemy.chanceToTeleport += 5;
    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        enemy.SetZeroVelocity();

        // 动画完成后：判定是否传送，否则返回战斗状态
        if (triggerCalled)
        {
            if(enemy.CanTeleport())
                stateMachine.ChangeState(enemy.teleportState);
            else
                stateMachine.ChangeState(enemy.battleState);
        }
    }
}
