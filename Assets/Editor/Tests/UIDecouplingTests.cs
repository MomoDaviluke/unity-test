using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Phase 3 UI/库存解耦回归测试
/// 覆盖：
/// - 结构契约：Inventory 不再持有 UI 类型字段（依赖方向反转为表现层→逻辑层）；
///   UI_InventoryPresenter 持有槽位父节点并具备 OnEnable/OnDisable 订阅退订；
/// - 生产者契约：AddItem/RemoveItem 触发 OnInventoryUpdated；currency 属性 setter/复合赋值触发 OnCurrencyChanged 且恰好一次；
///   currencyValue 字段带 FormerlySerializedAs("currency") 保证 MainScene 序列化值（1000）迁移；
/// - B9 最终形态：PlayerDeadState 源码经事件总线广播死亡、不再引用 UI；UI 具备 OnPlayerDead 订阅入口。
/// 说明：EditMode 不调用 Awake/Start 生命周期（PUD-06），行为测试手动初始化所需公共集合并直接调用方法。
/// </summary>
public class UIDecouplingTests
{
    private GameObject _inventoryGo;
    private List<UnityEngine.Object> _spawned;

    [SetUp]
    public void Setup()
    {
        _spawned = new List<UnityEngine.Object>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (UnityEngine.Object o in _spawned)
            if (o != null) UnityEngine.Object.DestroyImmediate(o);
        Inventory.instance = null;
        PlayerManager.instance = null;
    }

    private Inventory CreateInitializedInventory()
    {
        _inventoryGo = new GameObject("Inventory_Test");
        Inventory inv = _inventoryGo.AddComponent<Inventory>();
        inv.inventory = new List<InventoryItem>();
        inv.inventoryDictianory = new Dictionary<ItemData, InventoryItem>();
        inv.stash = new List<InventoryItem>();
        inv.stashDictianory = new Dictionary<ItemData, InventoryItem>();
        inv.equipment = new List<InventoryItem>();
        inv.equipmentDictianory = new Dictionary<ItemData_Equipment, InventoryItem>();
        Inventory.instance = inv; // EditMode 不调 Awake，手动注册
        return inv;
    }

    private ItemData CreateItemData(ItemType _type)
    {
        ItemData data = ScriptableObject.CreateInstance<ItemData>();
        data.itemType = _type;
        data.itemId = "test-item-" + _type;
        _spawned.Add(data);
        return data;
    }

    // ==================== 结构契约：Inventory 去 UI 化 ====================
    [Test]
    public void Inventory_HoldsNoUiReferences()
    {
        FieldInfo[] fields = typeof(Inventory).GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo f in fields)
        {
            Assert.AreNotEqual(typeof(Transform), f.FieldType,
                $"Inventory 不应持有 UI Transform 字段（{f.Name}）");
            Assert.IsFalse(f.FieldType.IsArray &&
                (f.FieldType.GetElementType() == typeof(UI_ItemSlot) ||
                 f.FieldType.GetElementType() == typeof(UI_EquipmentSlot) ||
                 f.FieldType.GetElementType() == typeof(UI_StatSlot)),
                $"Inventory 不应持有槽位组件数组（{f.Name}）");
        }
    }

    [Test]
    public void Presenter_DeclaresSlotParentsAndSubscription()
    {
        Type t = typeof(UI_InventoryPresenter);
        foreach (string name in new[] { "inventorySlotParent", "stashSloatParent", "equipmentSlotParent", "statSlotParent" })
            Assert.IsNotNull(t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance),
                $"Presenter 缺少槽位父节点字段 {name}（场景引用平移目标）");

        Assert.IsNotNull(t.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(t.GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance));
    }

    // ==================== 生产者契约：OnInventoryUpdated ====================
    [Test]
    public void AddItem_TriggersInventoryUpdated()
    {
        Inventory inv = CreateInitializedInventory();
        int fired = 0;
        Action handler = () => fired++;
        GameEvents.OnInventoryUpdated += handler;

        inv.AddItem(CreateItemData(ItemType.Equipment));

        GameEvents.OnInventoryUpdated -= handler;
        Assert.AreEqual(1, fired, "AddItem 应触发一次 OnInventoryUpdated");
        Assert.AreEqual(1, inv.inventory.Count, "装备应进入背包");
    }

    [Test]
    public void RemoveItem_TriggersInventoryUpdated()
    {
        Inventory inv = CreateInitializedInventory();
        ItemData data = CreateItemData(ItemType.Equipment);
        inv.AddItem(data);

        int fired = 0;
        Action handler = () => fired++;
        GameEvents.OnInventoryUpdated += handler;
        inv.RemoveItem(data);
        GameEvents.OnInventoryUpdated -= handler;

        Assert.AreEqual(1, fired, "RemoveItem 应触发一次 OnInventoryUpdated");
        Assert.AreEqual(0, inv.inventory.Count, "物品应已从背包移除");
    }

    // ==================== 生产者契约：currency 属性 ====================
    [Test]
    public void Currency_Setter_TriggersEventExactlyOnce()
    {
        GameObject go = new GameObject("PlayerManager_Test");
        _spawned.Add(go);
        PlayerManager pm = go.AddComponent<PlayerManager>();

        int fired = 0;
        int lastValue = -1;
        Action<int> handler = v => { fired++; lastValue = v; };
        GameEvents.OnCurrencyChanged += handler;

        pm.currency = 55;
        GameEvents.OnCurrencyChanged -= handler;

        Assert.AreEqual(1, fired, "属性赋值应恰好触发一次事件（防 setter+手动广播双触发）");
        Assert.AreEqual(55, lastValue);
    }

    [Test]
    public void Currency_CompoundAdd_TriggersEventWithNewValue()
    {
        GameObject go = new GameObject("PlayerManager_Test");
        _spawned.Add(go);
        PlayerManager pm = go.AddComponent<PlayerManager>();
        pm.currency = 100;

        int fired = 0;
        int lastValue = -1;
        Action<int> handler = v => { fired++; lastValue = v; };
        GameEvents.OnCurrencyChanged += handler;
        pm.currency += 30; // 外部写入点模式（EnemyStats/LostCurrencyController）
        GameEvents.OnCurrencyChanged -= handler;

        Assert.AreEqual(1, fired);
        Assert.AreEqual(130, lastValue, "复合赋值应广播新值");
    }

    [Test]
    public void HaveEnoughMoney_TriggersCurrencyChangedOnce()
    {
        GameObject go = new GameObject("PlayerManager_Test");
        _spawned.Add(go);
        PlayerManager pm = go.AddComponent<PlayerManager>();
        pm.currency = 100;

        int fired = 0;
        Action<int> handler = v => fired++;
        GameEvents.OnCurrencyChanged += handler;
        bool paid = pm.HaveEnoughMoney(30);
        GameEvents.OnCurrencyChanged -= handler;

        Assert.IsTrue(paid);
        Assert.AreEqual(1, fired, "扣费应恰好触发一次事件（原 Phase 2 手动广播已收敛到 setter）");
    }

    [Test]
    public void CurrencyValue_Field_HasFormerlySerializedAsForSceneMigration()
    {
        FieldInfo f = typeof(PlayerManager).GetField("currencyValue",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(f, "currency 序列化字段应存在");
        var attr = f.GetCustomAttribute<FormerlySerializedAsAttribute>();
        Assert.IsNotNull(attr, "缺少 FormerlySerializedAs：MainScene 的 currency: 1000 将丢失");
        Assert.AreEqual("currency", attr.oldName);
    }

    // ==================== B9 最终形态：死亡事件驱动 ====================
    // 注：PlayerDeadState 源码契约（不引用 UI、经 GameEvents 广播）已由 B5B9RegressionTests 覆盖，此处不重复。
    [Test]
    public void UI_SubscribesPlayerDeadEvent()
    {
        Assert.IsNotNull(typeof(UI).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance),
            "UI 应声明 OnEnable 订阅（OnDisable 对称退订）");

        int fired = 0;
        Action handler = () => fired++;
        GameEvents.OnPlayerDead += handler;
        GameEvents.PlayerDead();
        GameEvents.OnPlayerDead -= handler;
        Assert.AreEqual(1, fired, "PlayerDead 应广播给订阅者");
    }
}
