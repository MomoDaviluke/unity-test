using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Heal_Effect（治疗效果）- 按比例恢复玩家生命值
/// 功能：根据最大生命值百分比恢复生命
/// </summary>
[CreateAssetMenu(fileName = "Heal effect", menuName = "Data/Item effect/Heal effect")]
public class Heal_Effect : ItemEffect
{
    [Range(0f, 1f)]
    [SerializeField] private float healPercent;

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _enemyPosition)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        int healAmount = Mathf.RoundToInt(playerStats.GetMaxHealthValue() * healPercent);
        playerStats.IncreaseHealthBy(healAmount);
    }
}
