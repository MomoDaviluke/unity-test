using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGroundedState : PlayerState
{
    public PlayGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

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
        if (Input.GetKeyDown(KeyCode.R) && player.skill.blackhole.blackHoleUnlocked)
        {
            if (player.skill.blackhole.cooldownTimer > 0)
            {
                player.fX.CreatePopUpText("Cooldown");
                return;
            }
            stateMachine.ChangeState(player.blackhole);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1) && HasNoSword() && player.skill.sword.swordUnlocked)
        {
            stateMachine.ChangeState(player.aimSowrd);
        }
        if (Input.GetKeyDown(KeyCode.Q) && player.skill.parry.parryUnlocked)
        {
            stateMachine.ChangeState(player.counterAttackState);
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            stateMachine.ChangeState(player.primaryAttackState);
        }
        // 闪避：按Ctrl键
        if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time - player.lastDodgeTime > player.dodgeCooldown)
        {
            stateMachine.ChangeState(player.dodgeState);
        }
        if (!player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.airState);
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.jumpState);
        }
        // 跳跃缓冲：提前按跳键，落地后自动跳
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.jumpBufferTimer = player.jumpBufferTime;
        }
        if (player.CanJump())
        {
            player.UseJump();
            stateMachine.ChangeState(player.jumpState);
        }
    }
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
