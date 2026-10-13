using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Inventory（背包系统）- 管理物品、装备、药水和合成
/// 功能：物品存储、装备穿戴、药水使用、合成制作、存档/加载
/// </summary>
public class Inventory : MonoBehaviour, ISaveManager
{
    // ==================== 单例 ====================
    public static Inventory instance;

    // ==================== 物品数据 ====================
    [Header("初始物品")]
    public List<ItemData> startingItems;

    [Header("装备")]
    public List<InventoryItem> equipment;
    public Dictionary<ItemData_Equipment, InventoryItem> equipmentDictianory;

    [Header("背包")]
    public List<InventoryItem> inventory;
    public Dictionary<ItemData, InventoryItem> inventoryDictianory;

    [Header("仓库")]
    public List<InventoryItem> stash;
    public Dictionary<ItemData, InventoryItem> stashDictianory;

    // ==================== UI引用 ====================
    [Header("背包UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform stashSloatParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform statSlotParent;

    private UI_itemSlot[] inventoryItemSlot;
    private UI_itemSlot[] stashItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;
    private UI_StatSlot[] statSlot;

    // ==================== 冷却系统 ====================
    [Header("物品冷却")]
    private float lastTimeUsedFlask;
    private float lastTimeUsedAmulet;

    public float flaskCooldown { get; private set; }
    private float armorCooldown;

    // ==================== 存档数据 ====================
    [Header("数据库")]
    public List<InventoryItem> loadedItems;
    public List<ItemData_Equipment> loadedEquipment;

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // ==================== 启动初始化 ====================
    public void Start()
    {
        // 初始化列表和字典
        inventory = new List<InventoryItem>();
        inventoryDictianory = new Dictionary<ItemData, InventoryItem>();

        stash = new List<InventoryItem>();
        stashDictianory = new Dictionary<ItemData, InventoryItem>();

        equipment = new List<InventoryItem>();
        equipmentDictianory = new Dictionary<ItemData_Equipment, InventoryItem>();

        // 获取UI槽位引用
        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_itemSlot>();
        stashItemSlot = stashSloatParent.GetComponentsInChildren<UI_itemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        statSlot = statSlotParent.GetComponentsInChildren<UI_StatSlot>();

        UpdateSloatUI();
        AddStartingItems();

        // 初始化冷却时间（设为很早以前，确保首次可用）
        lastTimeUsedFlask = -999f;
        lastTimeUsedAmulet = -999f;
    }

    // ==================== 初始物品 ====================
    private void AddStartingItems()
    {
        // 先加载存档中的装备
        foreach (ItemData_Equipment item in loadedEquipment)
            EquipItem(item);

        // 加载存档中的物品
        if (loadedItems.Count > 0)
        {
            foreach (InventoryItem item in loadedItems)
            {
                for (int i = 0; i < item.stackSize; i++)
                    AddItem(item.data);
            }
            return;
        }

        // 添加配置的初始物品
        for (int i = 0; i < startingItems.Count; i++)
        {
            if (startingItems[i] != null)
                AddItem(startingItems[i]);
        }
    }

    // ==================== 装备管理 ====================
    // 装备物品：卸下同类型旧装备，应用属性加成
    public void EquipItem(ItemData _item)
    {
        ItemData_Equipment newEquipment = _item as ItemData_Equipment;
        InventoryItem newItem = new InventoryItem(newEquipment);

        ItemData_Equipment oldEquipment = null;

        // 查找同类型旧装备
        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
        {
            if (item.Key.equipmentType == newEquipment.equipmentType)
            {
                oldEquipment = item.Key;
                break;
            }
        }

        RemoveItem(_item);

        // 如果有旧装备，卸下并放回背包
        if (oldEquipment != null)
        {
            UnequipItem(oldEquipment);
            AddItem(oldEquipment);
        }

        // 装备新物品
        equipment.Add(newItem);
        equipmentDictianory.Add(newEquipment, newItem);
        newEquipment.AddModifiers();
        UpdateSloatUI();
    }

    // 卸下装备：移除属性加成
    public void UnequipItem(ItemData_Equipment itemToRemove)
    {
        if (equipmentDictianory.TryGetValue(itemToRemove, out InventoryItem value))
        {
            equipment.Remove(value);
            equipmentDictianory.Remove(itemToRemove);
            itemToRemove.RemoveModifiers();
        }
    }

    // ==================== UI更新 ====================
    private void UpdateSloatUI()
    {
        // 清空装备槽
        for (int i = 0; i < equipmentSlot.Length; i++)
            equipmentSlot[i].CleanUpSloat();

        // 更新装备槽
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                    equipmentSlot[i].UpdateSlots(item.Value);
            }
        }

        // 清空背包和仓库槽
        for (int i = 0; i < inventoryItemSlot.Length; i++)
            inventoryItemSlot[i].CleanUpSloat();
        for (int i = 0; i < stashItemSlot.Length; i++)
            stashItemSlot[i].CleanUpSloat();

        // 更新背包槽
        for (int i = 0; i < inventory.Count && i < inventoryItemSlot.Length; i++)
            inventoryItemSlot[i].UpdateSlots(inventory[i]);

        // 更新仓库槽
        for (int i = 0; i < stash.Count && i < stashItemSlot.Length; i++)
            stashItemSlot[i].UpdateSlots(stash[i]);

        UpdateStatsUI();
    }

    // ==================== 属性UI更新 ====================
    public void UpdateStatsUI()
    {
        for (int i = 0; i < statSlot.Length; i++)
            statSlot[i].UpdateStatValueUI();
    }

    // ==================== 物品添加 ====================
    // 根据物品类型添加到背包或仓库
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

    // ==================== 仓库管理 ====================
    public bool CanAddToStash()
    {
        if (stash.Count >= stashItemSlot.Length)
        {
            Debug.Log("Stash is full");
            return false;
        }
        return true;
    }

    // 添加物品到仓库（可堆叠）
    private void AddToStash(ItemData _item)
    {
        if (stashDictianory.TryGetValue(_item, out InventoryItem value))
            value.AddStack();
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            stash.Add(newItem);
            stashDictianory.Add(_item, newItem);
        }
    }

    // ==================== 背包管理 ====================
    // 添加物品到背包（可堆叠）
    private void AddToInventory(ItemData _item)
    {
        if (inventoryDictianory.TryGetValue(_item, out InventoryItem value))
            value.AddStack();
        else
        {
            InventoryItem newItem = new InventoryItem(_item);
            inventory.Add(newItem);
            inventoryDictianory.Add(_item, newItem);
        }
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

    // ==================== 物品移除 ====================
    public void RemoveItem(ItemData _item)
    {
        // 从背包移除
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

        // 从仓库移除
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

    // ==================== 合成系统 ====================
    // 检查材料是否足够，消耗材料并添加成品
    public bool CanCraft(ItemData_Equipment _itemToCraft, List<InventoryItem> _requireMaterials)
    {
        List<InventoryItem> materialsToRemove = new List<InventoryItem>();

        // 检查材料是否足够
        for (int i = 0; i < _requireMaterials.Count; i++)
        {
            if (stashDictianory.TryGetValue(_requireMaterials[i].data, out InventoryItem stashValue))
            {
                if (stashValue.stackSize < _requireMaterials[i].stackSize)
                    return false;
                else
                    materialsToRemove.Add(stashValue);
            }
            else
                return false;
        }

        // 消耗材料并添加成品
        for (int i = 0; i < materialsToRemove.Count; i++)
            RemoveItem(materialsToRemove[i].data);

        AddItem(_itemToCraft);
        return true;
    }

    // ==================== 查询方法 ====================
    public List<InventoryItem> GetEquipmentList() => equipment;
    public List<InventoryItem> GetStashList() => stash;

    // 获取指定类型的装备
    public ItemData_Equipment GetEquipment(EquipmentType _type)
    {
        ItemData_Equipment equipedItem = null;

        foreach (KeyValuePair<ItemData_Equipment, InventoryItem> item in equipmentDictianory)
        {
            if (item.Key.equipmentType == _type)
            {
                equipedItem = item.Key;
                break;
            }
        }
        return equipedItem;
    }

    // ==================== 物品使用 ====================
    // 使用药水：检查冷却并执行效果
    public void UseFlask()
    {
        ItemData_Equipment currentFlask = GetEquipment(EquipmentType.Flask);
        if (currentFlask == null)
            return;

        bool canUseFlask = Time.time > lastTimeUsedFlask + flaskCooldown;

        if (canUseFlask)
        {
            flaskCooldown = currentFlask.itemCooldown;
            currentFlask.Effect(null);
            lastTimeUsedFlask = Time.time;
        }
    }

    // 检查护甲是否可用
    public bool canUseArmor()
    {
        ItemData_Equipment currentArmor = GetEquipment(EquipmentType.Armor);

        if (currentArmor == null)
            return false;

        // 检查冷却
        if (Time.time > lastTimeUsedAmulet + armorCooldown)
        {
            armorCooldown = currentArmor.itemCooldown;
            lastTimeUsedAmulet = Time.time;
            return true;
        }
        return false;
    }

    #region 存档/加载
    // ==================== 存档/加载 ====================
    public void LoadData(GameData _data)
    {
        // 加载背包物品
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

        // 加载装备
        foreach (string loadedItemId in _data.equipmentId)
        {
            foreach (var item in GetItemDataBase())
            {
                if (item != null && loadedItemId == item.itemId)
                    loadedEquipment.Add(item as ItemData_Equipment);
            }
        }
    }

    public void SaveData(ref GameData _data)
    {
        _data.inventory.Clear();
        _data.equipmentId.Clear();

        // 合并背包和仓库数据
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

        // 保存到存档
        foreach (var pair in combinedInventory)
            _data.inventory.Add(pair.Key, pair.Value);

        foreach (var pair in equipmentDictianory)
            _data.equipmentId.Add(pair.Key.itemId);
    }

    // ==================== 物品数据库 ====================
    // 编辑器模式下查找所有物品数据，运行时回退到初始物品
    private List<ItemData> GetItemDataBase()
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
        foreach (ItemData item in startingItems)
        {
            if (item != null && !itemDataBase.Contains(item))
                itemDataBase.Add(item);
        }
#endif

        return itemDataBase;
    }
}
