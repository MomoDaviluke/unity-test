using UnityEngine;

/// <summary>
/// Buff_Effect（增益效果）- 临时提升玩家属性
/// 功能：根据属性类型和数值提供限时增益
/// </summary>
[CreateAssetMenu(fileName = "Buff_Effect", menuName = "Data/Item effect/Buff_Effect")]
public class Buff_Effect : ItemEffect
{
    private PlayerStats stats;
    [SerializeField] private StatType buffType;
    [SerializeField] private int buffAmount;
    [SerializeField] private float buffDuration;

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _enemyPosition)
    {
        stats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        stats.IncreaseStaBy(buffAmount, buffDuration, stats.GetStat(buffType));
    }
}
