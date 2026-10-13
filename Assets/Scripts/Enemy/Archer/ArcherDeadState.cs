using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherDeadState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Archer enemy;

    // ==================== 构造函数 ====================
    public ArcherDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时停止所有移动
    // Update中延迟2秒销毁敌人对象

    public override void Enter()
    {
        base.Enter();

        rb.velocity = Vector2.zero;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 2秒后销毁敌人对象（给死亡动画播放时间）
        GameObject.Destroy(enemy.gameObject, 2f);
    }
}