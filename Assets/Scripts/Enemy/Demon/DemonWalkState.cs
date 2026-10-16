using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonWalkState（恶魔Boss行走状态）- 追击玩家并释放技能
/// </summary>
public class DemonWalkState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;
    private Transform player;
    private int moveDir;

    // ==================== 构造函数 ====================
    public DemonWalkState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时获取玩家引用
    // Update中处理：
    //   1. 面朝玩家并追击
    //   2. 进入攻击距离且技能冷却完毕时释放随机技能
    //   3. 距离过近时后退，距离过远时前进

    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.instance.player.transform;
    }

    public override void Update()
    {
        base.Update();

        if (player == null)
        {
            stateMachine.ChangeState(demon.idleState);
            return;
        }

        stateTimer = demon.battleTime;

        // ---- 面朝控制 ----
        if (player.position.x > demon.transform.position.x)
            moveDir = 1;
        else
            moveDir = -1;

        demon.SetupDefaultFacingDir(moveDir);

        float distanceToPlayer = demon.GetPlayerDistance();

        // ---- 进入攻击距离：释放技能 ----
        if (distanceToPlayer < demon.attackDistance)
        {
            if (Time.time >= demon.lastTimeSkillUsed + demon.skillCooldown)
            {
                demon.SetZeroVelocity();
                demon.SelectNextSkill();
                return;
            }
        }

        // ---- 移动控制 ----
        // 距离过近时后退，距离过远时前进
        if (distanceToPlayer > demon.stopDistance)
            demon.SetVelocity(demon.chaseSpeed * moveDir, rb.linearVelocity.y);
        else
            demon.SetVelocity(demon.chaseSpeed * (-moveDir), rb.linearVelocity.y);
    }
}
