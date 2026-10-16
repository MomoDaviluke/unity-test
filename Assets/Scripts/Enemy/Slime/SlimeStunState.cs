using UnityEngine;

/// <summary>
/// SlimeStunState（史莱姆眩晕状态）- Phase 4 迁移至共享基类
/// 闪烁/击退/计时归位在 EnemyStunnedStateBase；
/// 史莱姆特有：落地后播眩晕动画并开启无敌帧（OnUpdateExtra），
/// Exit 不做闪烁取消（其 CancelColorChange 在落地检测内调用，CancelBlinkOnExit=false），
/// 仅关闭无敌帧（OnExitExtra）。
/// </summary>
public class SlimeStunState : EnemyStunnedStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Slime slime;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState IdleState => slime.idleState;
    protected override bool CancelBlinkOnExit => false;   // 闪烁取消在 Update 落地检测中执行

    // ==================== 构造函数 ====================
    public SlimeStunState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Slime enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        slime = enemy;
    }

    // ==================== 史莱姆特有逻辑 ====================
    protected override void OnUpdateExtra()
    {
        // 落地后触发眩晕动画并设置无敌
        if (rb.linearVelocity.y < .1f && slime.IsGroundDetected())
        {
            slime.fX.Invoke("CancelColorChange", 0);
            slime.anim.SetTrigger("Stun");
            slime.stats.MakeInvincible(true);
        }
    }

    protected override void OnExitExtra()
    {
        slime.stats.MakeInvincible(false);
    }
}
