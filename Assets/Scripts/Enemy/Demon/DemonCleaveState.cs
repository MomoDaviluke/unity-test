using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonCleaveState（恶魔Boss劈砍状态）- 对范围内玩家造成伤害和击退
/// </summary>
public class DemonCleaveState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;

    // ==================== 构造函数 ====================
    public DemonCleaveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
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

    // ==================== 劈砍伤害触发 ====================
    // 检测范围内玩家并造成伤害和击退
    public void TriggerCleaveDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.cleaveRange);

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
    }
}
