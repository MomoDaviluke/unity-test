using UnityEngine;

/// <summary>
/// PlayCounterAttackState（玩家反击状态）- 继承自PlayerState
/// 功能：进入反击姿态，检测范围内敌人的可眩晕攻击并进行反击
/// </summary>
public class PlayCounterAttackState : PlayerState
{
    // ==================== 反击参数 ====================
    private bool canCreateClone; // 是否可创建反击分身

    // ==================== 构造函数 ====================
    public PlayCounterAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 初始化反击状态，重置动画标记
    // Update: 检测范围内的箭矢和敌人，成功反击时触发效果
    // Exit: 无特殊处理

    public override void Enter()
    {
        base.Enter();

        canCreateClone = true;
        stateTimer = player.counterAttackDuration;
        player.anim.SetBool("SuccesfulCounterAttack", false);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        player.SetZeroVelocity();

        // ---- 反击检测 ----
        // 检测攻击范围内的所有碰撞体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);
        foreach (var hit in colliders)
        {
            // 反击箭矢：翻转箭矢方向
            if (hit.GetComponent<Arrow_Controller>() != null)
            {
                hit.GetComponent<Arrow_Controller>().FlipArrow();
                SuccesfulCounterAttack();
            }

            // 反击敌人：检测敌人是否可被眩晕
            if (hit.GetComponent<Enemy>() != null)
            {
                if (hit.GetComponent<Enemy>().CanbeStunned())
                {
                    SuccesfulCounterAttack();
                    player.skill.parry.UseSkill();

                    // 创建反击分身（仅一次）
                    if (canCreateClone)
                    {
                        canCreateClone = false;
                        player.skill.parry.MakeMirageOnParry(hit.transform);
                    }
                }
            }
        }

        // 反击时间结束或动画完成，切换回待机
        if (stateTimer < 0 || triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    // ==================== 成功反击 ====================
    // 延长状态时间并播放成功反击动画
    private void SuccesfulCounterAttack()
    {
        stateTimer = 10;
        player.anim.SetBool("SuccesfulCounterAttack", true);
    }
}
