using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonWalkState : EnemyState
{
    private Enemy_Demon demon;
    private Transform player;
    private int moveDir;

    public DemonWalkState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

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

        // 重置战斗计时器
        stateTimer = demon.battleTime;

        // 根据玩家位置确定方向
        if (player.position.x > demon.transform.position.x)
            moveDir = 1;
        else
            moveDir = -1;

        // 设置朝向
        demon.SetupDefaultFacingDir(moveDir);

        float distanceToPlayer = demon.GetPlayerDistance();
        
        // 进入攻击范围
        if (distanceToPlayer < demon.attackDistance)
        {
            // 冷却完成则攻击
            if (Time.time >= demon.lastTimeSkillUsed + demon.skillCooldown)
            {
                demon.SetZeroVelocity();
                demon.SelectNextSkill();
                return;
            }
            // 冷却中继续追击
        }

        // 追击玩家
        if (distanceToPlayer > demon.stopDistance)
        {
            demon.SetVelocity(demon.chaseSpeed * moveDir, rb.velocity.y);
        }
        else
        {
            demon.SetZeroVelocity();
        }
    }
}
