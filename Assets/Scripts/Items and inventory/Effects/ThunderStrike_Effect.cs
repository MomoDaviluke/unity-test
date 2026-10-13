using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ThunderStrike_Effect（雷电打击效果）- 在目标位置生成雷电特效
/// 功能：实例化雷电预制体，1秒后销毁
/// </summary>
[CreateAssetMenu(fileName = "ThunderStrike", menuName = "Data/Item effect/ThunderStrike")]
public class ThunderStrike_Effect : ItemEffect
{
    [SerializeField] private GameObject thunderStrikePrefab;

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _enemyPosition)
    {
        GameObject newThunderStrike = Instantiate(thunderStrikePrefab, _enemyPosition.position, Quaternion.identity);
        Destroy(newThunderStrike, 1f);
    }
}
