using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerAimSwordState（玩家瞄准剑状态）- 继承自PlayerState
/// 功能：进入瞄准模式，显示瞄准轨迹点，根据鼠标位置调整朝向
/// </summary>
public class PlayerAimSwordState : PlayerState
{
    // ==================== 构造函数 ====================
    public PlayerAimSwordState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    // ==================== 状态生命周期 ====================
    // Enter: 激活瞄准轨迹点
    // Update: 保持静止，根据鼠标位置翻转朝向，松开右键退出瞄准
    // Exit: 进入短暂忙碌状态防止立即操作

    public override void Enter()
    {
        base.Enter();
        player.skill.sword.DotsActive(true);
    }

    public override void Exit()
    {
        base.Exit();
        player.StartCoroutine("BusyFor", .2f);
    }

    public override void Update()
    {
        base.Update();
        player.SetZeroVelocity();

        // 松开右键退出瞄准状态
        if(Input.GetKeyUp(KeyCode.Mouse1))
            stateMachine.ChangeState(player.idleState);

        // 根据鼠标位置调整角色朝向
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (player.transform.position.x > mousePosition.x && player.facingDir == 1)
            player.Flip();
        else if(player.transform.position.x < mousePosition.x && player.facingDir == -1)
            player.Flip();
    }
}
