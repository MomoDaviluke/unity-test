using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonAnimationTriggers : MonoBehaviour
{
    private Enemy_Demon demon => GetComponentInParent<Enemy_Demon>();

    // 动画完成触发
    private void AnimationTrigger()
    {
        demon.AnimationFinishTrigger();
    }

    // 打击伤害触发 (Cleave)
    private void CleaveDamageTrigger()
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
            }
        }
    }

    // 踩踏伤害触发 (Smash)
    private void SmashDamageTrigger()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(demon.transform.position, demon.smashRadius);
        
        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                demon.stats.DoDamage(playerStats);
                
                // 击退
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
