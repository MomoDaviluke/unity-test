using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStunState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Slime enemy;

    // ==================== 构造函数 ====================
    public SlimeStunState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时启动红色闪烁，设置眩晕时间，施加击退力
    // Update中落地后触发眩晕动画并设置无敌
    // Exit时取消无敌状态

    public override void Enter()
    {
        base.Enter();

        // 启动红色闪烁视觉反馈
        enemy.fX.InvokeRepeating("RedColorBlink", 0, .1f);

        stateTimer = enemy.stunDuration;

        // 施加击退力
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    public override void Exit()
    {
        base.Exit();

        enemy.stats.MakeInvincible(false);
    }

    public override void Update()
    {
        base.Update();

        // 落地后触发眩晕动画并设置无敌
        if (rb.velocity.y < .1f && enemy.IsGroundDetected())
        {
            enemy.fX.Invoke("CancelColorChange", 0);
            enemy.anim.SetTrigger("Stun");
            enemy.stats.MakeInvincible(true);
        }

        // 眩晕时间结束后返回空闲状态
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.idleState);
    }
}