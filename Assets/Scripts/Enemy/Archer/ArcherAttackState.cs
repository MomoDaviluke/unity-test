using System.Collections;
using UnityEngine;

public class ArcherAttackState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Archer enemy;

    // ==================== 构造函数 ====================
    public ArcherAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Update中保持静止，动画完成后返回战斗状态
    // Exit时记录攻击时间

    public override void Enter()
    {
        base.Enter();
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

        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
}