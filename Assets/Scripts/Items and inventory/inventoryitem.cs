using System;
//背包系统中的物品类
[Serializable]
public class InventoryItem
{
    public ItemData data;//保存实打实的Item数据
    public int stackSize;//记录相同Item的数量
    public InventoryItem(ItemData _newItemData)//创建时就传入要保存的Item
    {
        data = _newItemData;
        AddStack();//由初始时由于没有相同类型的物体，为了使刚开始初始化便拥有值，此处必须调用一次此函数
    }

    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}