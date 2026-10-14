using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FreezeEnemies_Effect（冻结敌人效果）- 低血量时冻结周围敌人
/// 功能：血量低于10%且护甲可用时，冻结周围敌人
/// </summary>
[CreateAssetMenu(fileName = "FreezeEnemies_Effect", menuName = "Data/Item effect/Freeze _Enemies")]
public class FreezeEnemies_Effect : ItemEffect
{
    [SerializeField] private float duration;

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _transform)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        if (playerStats.currentHealth > playerStats.GetMaxHealthValue() * 0.1f)
            return;

        if (!Inventory.instance.canUseArmor())
            return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(_transform.position, 2f);

        foreach (var hit in colliders)
        {
            hit.GetComponent<Enemy>()?.FreezeTimeFor(duration);
        }
    }
}
