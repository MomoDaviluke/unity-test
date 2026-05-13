using UnityEngine;


[CreateAssetMenu(fileName = "Buff_Effect", menuName = "Data/Item effect/Buff_Effect")]
public class Buff_Effect : ItemEffect
{

    private PlayerStats stats;
    [SerializeField] private StatType buffType;
    [SerializeField] private int buffAmount;
    [SerializeField] private float buffDuration;

    public override void ExecuteEffect(Transform _enemyPosition)
    {
        stats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        stats.IncreaseStaBy(buffAmount, buffDuration, stats.GetStat(buffType));
    }

}
