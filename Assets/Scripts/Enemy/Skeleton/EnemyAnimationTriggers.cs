using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

    private void AnimationTrigger()
    {
        enemy.AnimationFinishTrigger();
    }
    private void AttackTrigger()
    {
        // 获取目标（玩家）
        Player player = PlayerManager.instance.player;

        // 设置伤害来源
        if (player != null)
        {
            player.lastDamageSource = transform;
        }

        // 原有的攻击逻辑
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackCheck.position, enemy.attackCheckRadius);
        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                PlayerStats target = hit.GetComponent<PlayerStats>();
                enemy.stats.DoDamage(target);
            }
        }
    }
    private void SpecialAttackTrigger()
    {
        enemy.AnimationSepcoalAttackTrigger();
    }
    private void OpenCountAttackWindow() => enemy.OpenCountAttackWindow();
    private void CloseCountAttackWindow() => enemy.CloseCountAttackWindow();
}
