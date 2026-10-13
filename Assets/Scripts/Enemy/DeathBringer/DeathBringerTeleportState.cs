using System.Collections;
using UnityEngine;

public class DeathBringerTeleportState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;

    // ==================== 构造函数 ====================
    public DeathBringerTeleportState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时设置无敌状态（传送期间不可被攻击）
    // Update中动画完成后：检查是否可以施法，可以则进入施法状态，否则进入战斗状态
    // Exit时取消无敌状态

    public override void Enter()
    {
        base.Enter();

        enemy.stats.MakeInvincible(true);
    }

    public override void Update()
    {
        base.Update();

        // 传送动画完成后决定下一个状态
        if (triggerCalled)
        {
            if (enemy.CanDoSpellCast())
                stateMachine.ChangeState(enemy.spellCastState);
            else
                stateMachine.ChangeState(enemy.battleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        enemy.stats.MakeInvincible(false);
    }
}
