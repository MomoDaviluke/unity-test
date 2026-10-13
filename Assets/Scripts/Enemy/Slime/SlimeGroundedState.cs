using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGroundedState : EnemyState
{
    // ==================== 核心引用 ====================
    protected Enemy_Slime enemy;
    protected Transform player;

    // ==================== 构造函数 ====================
    public SlimeGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时缓存玩家引用
    // Update中检测玩家（射线检测或距离<2.5），发现则切换到战斗状态

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

        if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.transform.position) < 2.5)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
