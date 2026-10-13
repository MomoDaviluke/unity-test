using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonIdleState（骷髅待机状态）- 静止待机，计时结束后切换到移动状态
/// </summary>
public class SkeletonIdleState : SkeletonGroundState
{
    // ==================== 构造函数 ====================
    // 直接调用父类构造函数，无需额外初始化
    public SkeletonIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Skeleton enemy) : base(_enemyBase, _stateMachine, _animBoolName, enemy)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter时关闭反击窗口并设置待机计时器
    // Update中计时器归零后切换到移动状态进行巡逻
    // Exit时播放音效提示状态切换

    public override void Enter()
    {
        base.Enter();
        enemy.CloseCountAttackWindow();
        stateTimer = enemy.idleTime;
    }

    public override void Exit()
    {
        base.Exit();
        AudioManager.instance.PlaySFX(24, enemy.transform);
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.moveState);
    }
}
