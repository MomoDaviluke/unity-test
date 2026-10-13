using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherGroundState : EnemyState
{
    // ==================== 核心引用 ====================
    protected Transform player;
    protected Enemy_Archer enemy;

    // ==================== 构造函数 ====================
    public ArcherGroundState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时缓存玩家引用
    // Update中检测玩家是否在仇恨范围内，检测到则切换到战斗状态

    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.transform.position) < enemy.agroDistance)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}