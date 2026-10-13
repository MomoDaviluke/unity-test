using System.Collections;
using UnityEngine;

public class DeathBringerDeadState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;

    // ==================== 构造函数 ====================
    public DeathBringerDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时冻结动画、禁用碰撞体
    // Update中在极短时间内施加向上的速度（死亡动画的击飞效果）

    public override void Enter()
    {
        base.Enter();

        enemy.anim.SetBool(enemy.lastAnimBoolName, true);
        enemy.anim.speed = 0;       // 冻结动画
        enemy.cd.enabled = false;   // 禁用碰撞体

        stateTimer = .1f;
    }

    public override void Update()
    {
        base.Update();

        // 死亡时的击飞效果（极短时间内向上移动）
        if (stateTimer > 0)
            rb.velocity = new Vector2(0, 10);
    }
}