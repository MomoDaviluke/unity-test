using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coelonychia_Effect（反甲效果）- 反弹受到的伤害给攻击者
/// 功能：记录受到的伤害，按百分比反弹给敌人
/// </summary>
[CreateAssetMenu(fileName = "Coelonychia effect", menuName = "Data/Item effect/Coelonychia effect")]
public class Coelonychia_Effect : ItemEffect
{
    // ==================== 反甲参数 ====================
    [Header("反甲效果")]
    [SerializeField] private float damageReflectPercentage = 20f;
    [SerializeField] public float damageReductionPercentage = 15f;

    private int lastDamageTaken = 0;

    // ==================== 伤害记录 ====================
    // 在PlayerStats.TakeDamage中调用，记录最后一次受到的伤害
    public void SetLastDamageTaken(int damage)
    {
        lastDamageTaken = damage;
    }

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _enemyPosition)
    {
        if (_enemyPosition == null)
            return;

        CharacterStats targetStats = _enemyPosition.GetComponent<CharacterStats>();
        if (targetStats == null)
            return;

        int reflectDamage = Mathf.RoundToInt(lastDamageTaken * (damageReflectPercentage / 100f));

        if (reflectDamage > 0)
        {
            targetStats.TakeDamage(reflectDamage);
        }
    }
}
