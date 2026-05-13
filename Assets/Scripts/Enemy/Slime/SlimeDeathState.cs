using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeDeathState : EnemyState
{
    private Enemy_Slime enemy; // 史莱姆敌人引用

    // 构造函数：初始化死亡状态
    public SlimeDeathState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // 进入死亡状态
    public override void Enter()
    {
        base.Enter();
        rb.velocity = Vector2.zero;

    }

    // 死亡状态更新
    public override void Update()
    {
        base.Update();

        GameObject.Destroy(enemy.gameObject, 2f);
    }
}