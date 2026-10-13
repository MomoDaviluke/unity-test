using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonAnimationTriggers : MonoBehaviour
{
    // ==================== 核心引用 ====================
    // 通过懒加载获取父对象中的Enemy_Demon组件
    private Enemy_Demon demon => GetComponentInParent<Enemy_Demon>();

    // ==================== 动画事件触发器 ====================
    // 以下方法由动画事件调用，触发对应的技能效果

    // 动画完成触发
    private void AnimationTrigger()
    {
        demon.AnimationFinishTrigger();
    }

    // 劈砍伤害触发：检测范围内玩家并造成伤害和击退
    private void CleaveDamageTrigger()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.cleaveRange);
        
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                demon.stats.DoDamage(playerStats);
                hit.GetComponent<Entity>().SetupKnockbackDir(demon.transform);
            }
        }
    }

    // 踩踏伤害触发：检测范围内玩家并造成伤害和击退
    private void SmashDamageTrigger()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.smashRadius);
        
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                demon.stats.DoDamage(playerStats);
                hit.GetComponent<Entity>().SetupKnockbackDir(demon.transform);
            }
        }
    }

    // 魔法施放触发
    private void CastSpellTrigger()
    {
        demon.CastSpell();
    }

    // 瞬移触发
    private void TeleportTrigger()
    {
        demon.Teleport();
    }

    // 安全位置瞬移触发
    private void TeleportToSafePosTrigger()
    {
        demon.FindSafeTeleportPosition();
    }
}
