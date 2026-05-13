using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Coelonychia effect", menuName = "Data/Item effect/Coelonychia effect")]
public class Coelonychia_Effect : ItemEffect
{
    [Header("反甲效果")]
    [SerializeField] private float damageReflectPercentage = 20f; // 反弹伤害的百分比
    [SerializeField] public float damageReductionPercentage = 15f; // 减少受到伤害的百分比

    // 这个变量用于存储最后一次受到的伤害值
    private int lastDamageTaken = 0;

    // 这个方法需要在PlayerStats.TakeDamage中调用
    public void SetLastDamageTaken(int damage)
    {
        lastDamageTaken = damage;
    }

    public override void ExecuteEffect(Transform _enemyPosition)
    {

        if (_enemyPosition == null)
        {
            return;
        }

        CharacterStats targetStats = _enemyPosition.GetComponent<CharacterStats>();
        if (targetStats == null)
        {
            return;
        }

        // 使用最后一次受到的伤害值计算反弹伤害
        int reflectDamage = Mathf.RoundToInt(lastDamageTaken * (damageReflectPercentage / 100f));

        if (reflectDamage > 0)
        {
            targetStats.TakeDamage(reflectDamage);
        }
    }
}
