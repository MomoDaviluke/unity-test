using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonStunnedState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Skeleton enemy;

    // ==================== 构造函数 ====================
    public SkeletonStunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时启动红色闪烁效果，设置眩晕计时器，施加击退速度
    // Update中计时器归零后返回待机状态
    // Exit时取消颜色变化效果

    public override void Enter()
    {
        base.Enter();

        // 启动红色闪烁视觉反馈（每0.1秒闪烁一次）
        enemy.fX.InvokeRepeating("RedColorBlink", 0f, 0.1f);

        stateTimer = enemy.stunDuration;

        // 施加击退力：与面朝方向相反的水平力 + 向上的垂直力
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    public override void Exit()
    {
        base.Exit();

        enemy.fX.Invoke("CancelColorChange", 0);
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.idleState);
        }

    }
}
