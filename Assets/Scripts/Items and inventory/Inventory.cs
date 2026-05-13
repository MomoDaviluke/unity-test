using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
//Inventory类，用于存储物品(背包系统)
public class Inventory : MonoBehaviour, ISaveManager
{
    public static Inventory instance;
    public List<ItemData> startingItems;
    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictianory;
    public List<InventoryItem> inventory;//inventoryItems类型的列表
    public Dictionary<ItemData, InventoryItem> inventoryDictianory;//以ItemData为Key寻找InventoryItem的字典
    public List<InventoryItem> stash;//stashItems类型的列表
    public Dictionary<ItemData, InventoryItem> stashDictianory;//以ItemData为Key寻找stashItem的字典

    [Header("Inventory UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform stashSloatParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform statSlotParent;

    private UI_itemSlot[] inventoryItemSlot;
    private UI_itemSlot[] stashItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;
    private UI_StatSlot[] statSlot;

    [Header("Items cooldown")]
    private float lastTimeUsedFlask;
    private float lastTimeUsedAmulet;

    public float flaskCooldown { get; private set; }
    private float armorCooldown;

    [Header("数据库")]
    public List<InventoryItem> loadedItems;//加载的物品
    public List<ItemData_Equipment> loadedEquipment;//加载的装备

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        //防止多次创建Inventory
    }

    public void Start()
    {
        inventory = new List<InventoryItem>();
        inventoryDictianory = new Dictionary<ItemData, InventoryItem>();

        stash = new List<InventoryItem>();
        stashDictianory = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictianory = new Dictionary<ItemData_Equipment, InventoryItem>();

        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_itemSlot>();
        stashItemSlot = stashSloatParent.GetComponentsInChildren<UI_itemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        statSlot = statSlotParent.GetComponentsInChildren<UI_StatSlot>();

        UpdateSloatUI();
        AddStartingItems();

        // 初始化冷却时间
        lastTimeUsedFlask = -999f;
        lastTimeUsedAmulet = -999f;
    }

    private void AddStartingItems()
    {
        foreach (ItemData_Equipment item in loadedEquipment)
        {
            EquipItem(item);
        }

        if (loadedItems.Count > 0)
        {
            foreach (InventoryItem item in loadedItems)
            {
                for (int i = 0; i < item.stackSize; i++)
                {
                    AddItem(item.data);
                }
            }
            return;
        }

        for (int i = 0; i < startingItems.Count; i++)
        {
            if (startingItems[i] != null)
                AddItem(startingItems[i]);
        }

    }

    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
            {
                oldEquipment = item.Key;
                break;  // 找到后立即跳出循环
            }
        }

        // 先从背包中移除新装备
        RemoveItem(_item);
        // 然后卸下旧装备并添加到背包
        if (oldEquipment != null)
        {
            UnequipItem(oldEquipment);
            AddItem(oldEquipment);
        }
        // 最后装备新装备
        equipment.Add(newItem);
        equipmentDictianory.Add(newEquipment, newItem);
        newEquipment.AddModifiers();
        UpdateSloatUI();
    }

    /// <summary>
    /// 卸下装备并移除其属性加成
    /// </summary>
    /// <param name="itemToRemove"></param>
    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if (equipmentDictianory.TryGetValue(itemToRemove, out InventoryItem value))
        {
            equipment.Remove(value);
            equipmentDictianory.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
        }
    }

    private void UpdateSloatUI()
    {
        // 首先清理所有装备槽位
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            equipmentSlot[i].CleanUpSloat();
        }

        // 然后更新有装备的槽位
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                {
                    equipmentSlot[i].UpdateSlots(item.Value);
                }
            }
        }

        // 清理背包和仓库槽位
        for (int i = 0; i < inventoryItemSlot.Length; i++)
        {
            inventoryItemSlot[i].CleanUpSloat();
        }
        for (int i = 0; i < stashItemSlot.Length; i++)
        {
            stashItemSlot[i].CleanUpSloat();
        }

        // 更新背包槽位
        for (int i = 0; i < inventory.Count && i < inventoryItemSlot.Length; i++)
        {
            inventoryItemSlot[i].UpdateSlots(inventory[i]);
        }

        // 更新仓库槽位
        for (int i = 0; i < stash.Count && i < stashItemSlot.Length; i++)
        {
            stashItemSlot[i].UpdateSlots(stash[i]);
        }

        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        for (int i = 0; i < statSlot.Length; i++)
        {
            statSlot[i].UpdateStatValueUI();
        }
    }

    /// <summary>
    /// 这个函数根据物品类型将物品添加到背包或仓库，
    /// 并更新UI显示。对于装备类型的物品，首先检查背包是否有足够空间，
    /// 如果有则添加到背包；对于材料类型的物品，检查仓库是否有足够空间，
    /// 如果有则添加到仓库。最后调用UpdateSloatUI()函数刷新UI显示。
    /// </summary>
    /// <param name="_item"></param>
    public void AddItem(ItemData _item)
    {
        if (_item.itemType == ItemType.Equipment)
        {
            if (CanAddItem())
                AddToInventory(_item);
        }
        else if (_item.itemType == ItemType.Material)
        {
            if (CanAddToStash())
                AddToStash(_item);
        }

        UpdateSloatUI();
    }
    public bool CanAddToStash()
    {
        if (stash.Count >= stashItemSlot.Length)
        {
            Debug.Log("Stash is full");
            return false;
        }
        return true;
    }

    private void AddToStash(ItemData _item)
    {
        if (stashDictianory.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();
        }
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            stash.Add(newItem);
            stashDictianory.Add(_item, newItem);
        }
    }

    private void AddToInventory(ItemData _item)
    {
        if (inventoryDictianory.TryGetValue(_item, out InventoryItem value))
        {
            value.AddStack();
        }//字典的使用，通过ItemData类型的数据找到InventoryItem里的与之对应的同样类型的数据
        else//初始时由于没有相同类型的物体，故调用else是为了初始化库存，使其中含有一个基本的值
        {
            InventoryItem newItem = new InventoryItem(_item);
            inventory.Add(newItem);//填进列表里只有一次
            inventoryDictianory.Add(_item, newItem);//同上
        }
    }

    public void RemoveItem(ItemData _item)//将物体剔除Inventory的函数
    {
        if (inventoryDictianory.TryGetValue(_item, out InventoryItem value))
        {
            if (value.stackSize <= 1)
            {
                inventory.Remove(value);
                inventoryDictianory.Remove(_item);

            }
            else
                value.RemoveStack();
        }
        if (stashDictianory.TryGetValue(_item, out InventoryItem stashValue))
        {
            if (stashValue.stackSize <= 1)
            {
                stash.Remove(stashValue);
                stashDictianory.Remove(_item);

            }
            else
                stashValue.RemoveStack();
        }
        UpdateSloatUI();
    }

    public bool CanAddItem()
    {
        if (inventory.Count >= inventoryItemSlot.Length)
        {
            Debug.Log("Inventory is full");
            return false;
        }
        return true;
    }
    public bool CanCraft(ItemData_Equipment _itemToCraft, List<InventoryItem> _requireMaterials)

    {
        List<InventoryItem> materialsToRemove = new List<InventoryItem>();
        for (int i = 0; i < _requireMaterials.Count; i++)
        {
            if (stashDictianory.TryGetValue(_requireMaterials[i].data, out InventoryItem stashValue))
            {
                if (stashValue.stackSize < _requireMaterials[i].stackSize)
                {
                    return false;
                }
                else
                {
                    materialsToRemove.Add(stashValue);
                }
            }
            else
            {
                return false;
            }
        }
        for (int i = 0; i < materialsToRemove.Count; i++)
        {
            RemoveItem(materialsToRemove[i].data);
        }
        AddItem(_itemToCraft);
        return true;
    }

    public List<InventoryItem> GetEquipmentList() => equipment;

    public List<InventoryItem> GetStashList() => stash;

    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        ItemData_Equipment equipedItem = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
        {
            if (item.Key.equipmentType == _type)
            {
                equipedItem = item.Key;
                break;  // 找到后立即跳出循环
            }
        }
        return equipedItem;
    }
    public void UseFlask()
    {
        ItemData_Equipment currentFlask = GetEquipment(EquipmentType.Flask);
        if (currentFlask == null)
        {
            return;
        }

        bool canUseFlask = Time.time > lastTimeUsedFlask + flaskCooldown;

        if (canUseFlask)
        {
            flaskCooldown = currentFlask.itemCooldown;
            currentFlask.Effect(null);
            lastTimeUsedFlask = Time.time;
        }
    }
    public bool canUseArmor()
    {
        ItemData_Equipment currentArmor = GetEquipment(EquipmentType.Armor);

        if (currentArmor == null)
        {
            return false;
        }
        if (Time.time > lastTimeUsedAmulet + armorCooldown)
        {
            armorCooldown = currentArmor.itemCooldown;
            lastTimeUsedAmulet = Time.time;
            return true;
        }
        return false;
    }

    public void LoadData(GameData _data)
    {
        foreach (KeyValuePair<string, int> pair in _data.inventory)
        {
            foreach (var item in GetItemDataBase())
            {
                if (item != null && item.itemId == pair.Key)
                {
                    InventoryItem itemToLoad = new InventoryItem(item);
                    itemToLoad.stackSize = pair.Value;

                    loadedItems.Add(itemToLoad);
                }
            }
        }
        foreach (string loadedItemId in _data.equipmentId)
        {
            foreach (var item in GetItemDataBase())
            {
                if (item != null && loadedItemId == item.itemId)
                {
                    loadedEquipment.Add(item as ItemData_Equipment);
                }
            }
        }
    }

    public void SaveData(ref GameData _data)
    {
        _data.inventory.Clear();
        _data.equipmentId.Clear();

        // 合并背包和仓库物品，避免重复添加相同itemId
        Dictionary<string, int> combinedInventory = new Dictionary<string, int>();

        foreach (var pair in inventoryDictianory)
        {
            if (combinedInventory.ContainsKey(pair.Key.itemId))
                combinedInventory[pair.Key.itemId] += pair.Value.stackSize;
            else
                combinedInventory.Add(pair.Key.itemId, pair.Value.stackSize);
        }

        foreach (var pair in stashDictianory)
        {
            if (combinedInventory.ContainsKey(pair.Key.itemId))
                combinedInventory[pair.Key.itemId] += pair.Value.stackSize;
            else
                combinedInventory.Add(pair.Key.itemId, pair.Value.stackSize);
        }

        // 将合并后的数据写入_data.inventory
        foreach (var pair in combinedInventory)
        {
            _data.inventory.Add(pair.Key, pair.Value);
        }

        // 保存装备ID列表
        foreach (var pair in equipmentDictianory)
        {
            _data.equipmentId.Add(pair.Key.itemId);
        }
    }
    private List<ItemData> GetItemDataBase()//获取物品数据库
    {
        List<ItemData> itemDataBase = new List<ItemData>();

#if UNITY_EDITOR
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/Data/Items" });

        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(SOpath);
            itemDataBase.Add(itemData);
        }
#else
        // 在构建版本中使用一个预定义的物品清单，可以在编辑器中创建并序列化
        // 这里我们简单返回 startingItems 作为后备方案
        foreach (ItemData item in startingItems)
        {
            if (item != null && !itemDataBase.Contains(item))
                itemDataBase.Add(item);
        }
#endif

        return itemDataBase;
    }

}
