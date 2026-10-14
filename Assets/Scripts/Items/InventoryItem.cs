using System;

/// <summary>
/// InventoryItem（背包物品）- 序列化物品数据，支持堆叠
/// 功能：存储物品数据、堆叠数量管理
/// </summary>
[Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData _newItemData)
    {
        data = _newItemData;
        AddStack();
    }

    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}