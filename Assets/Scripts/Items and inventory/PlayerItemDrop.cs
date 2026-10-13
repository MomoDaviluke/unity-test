using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerItemDrop（玩家物品掉落）- 玩家死亡时掉落装备和材料
/// 功能：根据概率掉落已装备物品和仓库材料
/// </summary>
public class PlayerItemDrop : ItemDrop
{
    // ==================== 掉落概率 ====================
    [Header("玩家掉落物品")]
    [SerializeField] private float chanceToLooseItems;
    [SerializeField] private float chanceToLooseMaterials;

    // ==================== 掉落生成 ====================
    // 遍历装备和仓库，根据概率决定掉落物品
    public override void GenerateDrop()
    {
        if (Inventory.instance == null)
            return;

        Inventory inventory = Inventory.instance;
        List<InventoryItem> itemsToUnequip = new List<InventoryItem>();
        List<InventoryItem> materialsToLoose = new List<InventoryItem>();

        // 遍历装备列表，根据概率掉落
        foreach (InventoryItem item in inventory.GetEquipmentList())
        {
            if (Random.Range(0, 100) <= chanceToLooseItems)
            {
                DropItem(item.data);
                itemsToUnequip.Add(item);
            }
        }

        for (int i = 0; i < itemsToUnequip.Count; i++)
        {
            inventory.UnequipItem(itemsToUnequip[i].data as ItemData_Equipment);
        }

        // 遍历仓库材料，根据概率掉落
        foreach (InventoryItem item in inventory.GetStashList())
        {
            if (Random.Range(0, 100) <= chanceToLooseMaterials)
            {
                DropItem(item.data);
                materialsToLoose.Add(item);
            }
        }

        for (int i = 0; i < materialsToLoose.Count; i++)
        {
            inventory.RemoveItem(materialsToLoose[i].data);
        }
    }
}
