using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStunState : EnemyState
{
    private Enemy_Slime enemy; // 史莱姆敌人引用

    // 构造函数：初始化状态
    public SlimeStunState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // 进入眩晕状态
    public override void Enter()
    {
        base.Enter();

        enemy.fX.InvokeRepeating("RedColorBlink", 0, .1f); // 启动红色闪烁效果

        stateTimer = enemy.stunDuration; // 设置眩晕持续时间

        // 设置眩晕击退的速度和方向
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    // 退出眩晕状态
    public override void Exit()
    {
        base.Exit();

        enemy.stats.MakeInvincible(false); // 取消无敌状态
    }

    // 眩晕状态更新
    public override void Update()
    {
        base.Update();
        if (rb.velocity.y < .1f && enemy.IsGroundDetected())
        {
            enemy.fX.Invoke("CancelColorChange", 0); // 取消颜色变化效果
            enemy.anim.SetTrigger("Stun");
            enemy.stats.MakeInvincible(true);
        }
        // 眩晕时间结束后返回空闲状态
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.idleState);
    }
}