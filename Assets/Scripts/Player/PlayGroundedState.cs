using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayGroundedState（玩家地面状态基类）- 继承自PlayerState
/// 功能：处理所有地面状态的共同逻辑（攻击、技能、跳跃、闪避输入检测）
/// 子类：PlayerIdleState, PlayerIMoveState
/// </summary>
public class PlayGroundedState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Update中检测所有地面输入：
    //   1. 技能输入（黑洞、投掷剑、反击）
    //   2. 攻击输入（普通攻击）
    //   3. 闪避输入
    //   4. 跳跃输入（含跳跃缓冲和土狼时间机制）
    //   5. 离开地面检测

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // ---- 黑洞技能 ----
        if (InputReader.instance.BlackholePressed && player.skill.blackhole.blackHoleUnlocked)
        {
            if (player.skill.blackhole.cooldownTimer > 0)
            {
                player.fX.CreatePopUpText("Cooldown");
                return;
            }
            stateMachine.ChangeState(player.blackhole);
        }

        // ---- 投掷剑技能 ----
        if (InputReader.instance.AimSwordPressed && HasNoSword() && player.skill.sword.swordUnlocked)
        {
            stateMachine.ChangeState(player.aimSowrd);
        }

        // ---- 反击技能 ----
        if (InputReader.instance.ParryPressed && player.skill.parry.parryUnlocked)
        {
            stateMachine.ChangeState(player.counterAttackState);
        }

        // ---- 普通攻击（按住连段）----
        if (InputReader.instance.AttackHeld)
        {
            stateMachine.ChangeState(player.primaryAttackState);
        }

        // ---- 闪避 ----
        if (InputReader.instance.DodgePressed && Time.time - player.lastDodgeTime > player.dodgeCooldown)
        {
            stateMachine.ChangeState(player.dodgeState);
        }

        // ---- 离开地面检测 ----
        if (!player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }

        // ---- 跳跃（Phase 5 收敛）----
        // 原此处有"直判 Space+落地 切跳跃"与"直写 jumpBufferTimer"两段逻辑，
        // 与 Player.UpdateJumpBuffer/CanJump 构成双路径判定（同帧可两次 ChangeState）。
        // 现统一为 Player 侧 CanJump() 单一查询：缓冲/土狼计时器在 Player.Update
        // 先于状态机更新（按键当帧即生效，无延迟），地面状态只调用查询。
        if (player.CanJump())
        {
            player.UseJump();
            stateMachine.ChangeState(player.jumpState);
        }
    }

    // ==================== 剑状态检查 ====================
    // 检查是否没有剑（用于投掷剑技能），有剑则召回
    private bool HasNoSword()
    {
        if (!player.sword)
        {
            return true;
        }
        player.sword.GetComponent<Sword_Skill_Controller>().ReturnSword();
        return false;
    }
}
