using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGroundedState : EnemyState
{
    protected Enemy_Slime enemy; // 史莱姆敌人引用
    protected Transform player; // 玩家位置引用

    // 构造函数：初始化地面状态
    public SlimeGroundedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    // 进入地面状态
    public override void Enter()
    {
        base.Enter();

        player = PlayerManager.instance.player.transform; // 获取玩家位置引用
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 地面状态更新
    public override void Update()
    {
        base.Update();

        // 检测玩家，如发现玩家则切换到战斗状态
        if (enemy.IsPlayerDetected() || Vector2.Distance(enemy.transform.position, player.transform.position) < 2.5)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
