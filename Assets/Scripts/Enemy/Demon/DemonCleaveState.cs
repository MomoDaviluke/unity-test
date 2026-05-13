using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonCleaveState : EnemyState
{
    private Enemy_Demon demon;

    public DemonCleaveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        // 保持原地
        demon.SetZeroVelocity();

        // 动画结束，触发技能完成
        if (triggerCalled)
        {
            demon.SkillFinished();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    // 动画帧事件 - 造成伤害
    public void TriggerCleaveDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.cleaveRange);
        
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                demon.stats.DoDamage(playerStats);
                
                // 击退
                hit.GetComponent<Entity>().SetupKnockbackDir(demon.transform);
                demon.PlayHitEffect(hit.transform.position);
            }
        }
    }
}
