using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonSmashState（恶魔Boss重击状态）- 对范围内玩家造成伤害和击退
/// </summary>
public class DemonSmashState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;

    // ==================== 构造函数 ====================
    public DemonSmashState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Update中保持静止，动画完成后返回行走状态

    public override void Update()
    {
        base.Update();
        demon.SetZeroVelocity();

        if (triggerCalled)
            demon.SkillFinished();
    }

    // ==================== 踩踏伤害触发 ====================
    // 检测范围内玩家并造成伤害和击退，同时播放地面特效
    public void TriggerSmashDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.smashRadius);

        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                demon.stats.DoDamage(playerStats);
                hit.GetComponent<Entity>().SetupKnockbackDir(demon.transform);
                demon.PlayHitEffect(hit.transform.position);
            }
        }

        demon.PlaySmashGroundEffect(demon.transform.position);
    }
}
