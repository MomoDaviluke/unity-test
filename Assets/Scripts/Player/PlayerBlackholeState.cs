using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerBlackholeState（玩家黑洞技能状态）- 继承自PlayerState
/// 功能：玩家释放黑洞技能时的特殊状态，先向上飞升再释放黑洞
/// </summary>
public class PlayerBlackholeState : PlayerState
{
    // ==================== 黑洞参数 ====================
    private float flyTime = .4f;   // 上升飞行时间
    private bool skillUsed;        // 技能是否已释放
    private float defaultGravity;  // 默认重力值（用于恢复）

    // ==================== 构造函数 ====================
    public PlayerBlackholeState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 记录默认重力，关闭重力，设置上升时间
    // Update: 先向上飞升，飞升结束后释放黑洞技能，技能完成后切换回空中状态
    // Exit: 恢复重力、取消透明、强制结束黑洞技能

    public override void AnimationFinishTrigger()
    {
        base.AnimationFinishTrigger();
    }

    public override void Enter()
    {
        base.Enter();
        defaultGravity = player.rb.gravityScale;

        skillUsed = false;
        stateTimer = flyTime;
        rb.gravityScale = 0; // 关闭重力实现上升
    }

    public override void Exit()
    {
        base.Exit();

        // 强制恢复玩家属性
        player.rb.gravityScale = defaultGravity;
        player.fX.MakeTransprent(false);

        // 确保结束黑洞技能
        if (player.skill.blackhole != null)
            player.skill.blackhole.ForceCompleteSkill();
    }

    public override void Update()
    {
        base.Update();

        // ---- 上升阶段 ----
        if (stateTimer > 0)
        {
            rb.velocity = new Vector2(0, 15);
        }

        // ---- 释放黑洞阶段 ----
        if (stateTimer < 0)
        {
            rb.velocity = new Vector2(0, -.1f);

            if (!skillUsed)
            {
                if (player.skill.blackhole.CanUseSkill())
                    skillUsed = true;
            }
        }

        // ---- 技能完成检测 ----
        // 增加空值保护
        if (player.skill.blackhole != null && player.skill.blackhole.SkillCompleted())
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
