using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();
    private void AnimationTrigger()
    {
        player.AnimationTrigger();
    }

    private void AttackTrigger()
    {
        AudioManager.instance.PlaySFX(2, null);
        if (player == null || player.attackCheck == null)
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                EnemyStats _target = hit.GetComponent<EnemyStats>();

                if (_target != null && player.stats != null)
                {
                    player.stats.DoDamage(_target);

                    // 检查背包系统和武器是否存在
                    if (Inventory.instance != null)
                    {
                        var weapon = Inventory.instance.GetEquipment(EquipmentType.Weapon);
                        if (weapon != null)
                        {
                            weapon.Effect(_target.transform);
                        }
                    }
                }
            }
        }
    }
    private void ThrowSword()
    {
        SkillManager.instance.sword.CreateSword();
    }
}
