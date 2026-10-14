using System.Collections;
using UnityEngine;

public class ShadyDeadState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Shady enemy;

    // ==================== 构造函数 ====================
    public ShadyDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Shady _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Update中检测动画触发标记，动画完成后调用SelfDestroy销毁自身
    // 注意：Shady的死亡动画会触发自爆效果（在Enemy_Shady.AnimationSpecialAttackTrigger中）

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if(triggerCalled)
            enemy.SelfDestroy();
    }
}
