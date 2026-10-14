using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemDatabase（物品数据库）- ScriptableObject，显式登记全部物品资产
/// 目的（修复 B1）：Inventory 原先在编辑器下用 AssetDatabase 扫描 Assets/Data/Items，
///                 打包后（#else 分支）却只回退到 startingItems，导致打包后加载存档时
///                 所有非初始物品全部丢失。改用显式登记的资产后，编辑器与打包行为完全一致。
/// 使用：在 Inspector 中登记所有 ItemData 资产，由 Inventory 注入使用。
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // 全部已登记物品资产
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    /// <summary>返回内部物品列表（作为 Inventory 的物品数据库使用）</summary>
    public List<ItemData> GetItems() => items;

    /// <summary>
    /// 按 itemId 查找物品；itemId 为空或未找到时返回 null。
    /// </summary>
    public ItemData GetItemById(string _itemId)
    {
        if (string.IsNullOrEmpty(_itemId))
            return null;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemId == _itemId)
                return items[i];
        }
        return null;
    }
}
