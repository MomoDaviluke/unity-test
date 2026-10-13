using System.Collections;
using UnityEngine;


public class ArcherStunnedState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Archer enemy;

    // ==================== 构造函数 ====================
    public ArcherStunnedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时启动红色闪烁，设置眩晕时间，施加击退力
    // Update中计时器归零后返回待机
    // Exit时取消颜色变化效果

    public override void Enter()
    {
        base.Enter();

        // 启动红色闪烁视觉反馈（每0.1秒闪烁一次）
        enemy.fX.InvokeRepeating("RedColorBlink", 0, .1f);

        stateTimer = enemy.stunDuration;

        // 施加击退力
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    public override void Exit()
    {
        base.Exit();

        // 取消颜色变化效果
        enemy.fX.Invoke("CancelColorChange", 0);
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.idleState);
    }
}