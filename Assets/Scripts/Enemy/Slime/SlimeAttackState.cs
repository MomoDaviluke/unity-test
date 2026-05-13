using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttackState : EnemyState
{
    protected Enemy_Slime enemy; // 史莱姆敌人引用

    // 构造函数：初始化攻击状态
    public SlimeAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // 进入攻击状态
    public override void Enter()
    {
        base.Enter();
    }

    // 退出攻击状态
    public override void Exit()
    {
        base.Exit();

        // 记录最后攻击时间，用于计算冷却
        enemy.lastTimeAttacked = Time.time;
    }

    // 攻击状态更新
    public override void Update()
    {
        base.Update();

        // 攻击时速度设为0，防止被击退
        enemy.SetZeroVelocity();

        // 如果动画触发器被调用（攻击动画结束），则返回战斗状态
        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }
}