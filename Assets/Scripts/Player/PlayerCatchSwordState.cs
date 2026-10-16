using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerCatchSwordState（玩家接剑状态）- 继承自PlayerState
/// 功能：播放接剑动画，施加反冲力，触发屏幕震动和尘土特效
/// </summary>
public class PlayerCatchSwordState : PlayerState
{
    // ==================== 接剑参数 ====================
    private Transform sword; // 剑的Transform引用

    // ==================== 构造函数 ====================
    public PlayerCatchSwordState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 获取剑的位置，播放特效和屏幕震动，根据剑的位置翻转朝向，施加反冲力
    // Update: 动画完成后切换回待机
    // Exit: 进入短暂忙碌状态

    public override void Enter()
    {
        base.Enter();

        sword = player.sword.transform;

        // 播放接剑特效
        player.fX.PlayDustFx();
        player.fX.ScreenShake(player.fX.shakeSwordImpact);

        // 根据剑的位置翻转朝向（面向剑的方向）
        if (player.transform.position.x > sword.position.x && player.facingDir == 1)
            player.Flip();
        else if (player.transform.position.x < sword.position.x && player.facingDir == -1)
            player.Flip();

        // 施加接剑反冲力
        rb.linearVelocity = new Vector2(player.swordReturnImpact * -player.facingDir, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine("BusyFor", .1f);
    }

    public override void Update()
    {
        base.Update();

        // 动画完成后切换回待机状态
        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);
    }
}
