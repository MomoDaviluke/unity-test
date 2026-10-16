using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherJumpState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Archer enemy;

    // ==================== 构造函数 ====================
    public ArcherJumpState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时施加后跳速度（与面朝方向相反的水平力+向上的垂直力）
    // Update中更新动画的yVelocity参数，落地后返回战斗状态

    public override void Enter()
    {
        base.Enter();

        // 后跳：与面朝方向相反的水平速度 + 向上的垂直速度
        enemy.rb.linearVelocity = new Vector2(enemy.jumpVelocity.x * -enemy.facingDir, enemy.jumpVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 更新动画中的垂直速度参数
        enemy.anim.SetFloat("yVelocity", rb.linearVelocity.y);

        // 下落过程中检测到地面，返回战斗状态
        if (rb.linearVelocity.y < 0 && enemy.IsGroundDetected())
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}