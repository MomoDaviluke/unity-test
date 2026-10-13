using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemDrop（物品掉落系统）- 处理敌人死亡时的物品掉落
/// 功能：根据概率生成掉落物品，创建掉落实例
/// 设计思路：每个敌人配置可掉落物品列表，死亡时按概率生成
/// </summary>
public class ItemDrop : MonoBehaviour
{
    [SerializeField] private int PossilbleItemDrop;     // 最大掉落数量：一次最多掉落的物品数
    [SerializeField] private ItemData[] possibleDrop;   // 可掉落物品列表
    private List<ItemData> dropList;                    // 实际掉落列表
    [SerializeField] private GameObject dropPrefab;     // 掉落物预制体

    // 修复 B2：移除 hasDropped 标志。
    // 原设计意图是"防止重复掉落"，但调用方 EnemyStats.Die()/PlayerStats.Die()
    // 已通过 isDead 保证每次死亡只调用一次 GenerateDrop，该标志完全冗余；
    // 更严重的是 DropItem 内的检查会把同一次 GenerateDrop 的多件掉落截断为 1 件
    // （敌人多物品配置、玩家死亡掉装备+材料均被错误拦截）。

    /// <summary>
    /// 生成掉落：根据概率选择物品并创建掉落实例
    /// </summary>
    public virtual void GenerateDrop()
    {
        // 初始化掉落列表
        dropList = new List<ItemData>();

        // 根据每个物品的掉落概率决定是否加入掉落列表
        for (int i = 0; i < possibleDrop.Length; i++)
        {
            if (Random.Range(0, 100) <= possibleDrop[i].dropChance)
                dropList.Add(possibleDrop[i]);
        }

        // 如果没有物品可掉落，直接返回
        if (dropList.Count == 0)
            return;

        // 随机选择指定数量的物品掉落
        for (int i = 0; i < PossilbleItemDrop && dropList.Count > 0; i++)
        {
            ItemData randomItem = dropList[Random.Range(0, dropList.Count)];
            dropList.Remove(randomItem);
            DropItem(randomItem);
        }
    }

    /// <summary>
    /// 生成指定物品的掉落实例
    /// </summary>
    /// <param name="_itemData">要掉落的物品数据</param>
    protected void DropItem(ItemData _itemData)
    {
        if (dropPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] 的 dropPrefab 未设置！无法生成掉落物品。");
            return;
        }

        // 创建掉落物实例
        GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        // 给掉落物品一个随机的初始速度，使其看起来像是被弹出
        Vector2 randomVelocity = new Vector2(Random.Range(-5f, 5f), Random.Range(15f, 20f));
        newDrop.GetComponent<ItemObject>().SetupItem(_itemData, randomVelocity);
    }
}
