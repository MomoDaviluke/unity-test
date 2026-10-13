using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Inventory 装备冷却特征测试（Characterization Test）
/// 目的：固化 canUseArmor() 的冷却语义（首次可用 → 冷却期内拦截 → 冷却结束可用）。
/// B3 修复说明：canUseArmor 原使用名为 lastTimeUsedAmulet 的字段记录护甲冷却。
///       经全量调用点核查，护符（Amulet）效果路径（Crystal/Sword Controller →
///       equipedAmulet.Effect）并不检查冷却，该字段实际只被护甲使用——属“命名污染”。
///       修复即将字段重命名为 lastTimeUsedArmor；本测试同步更新反射字段名，
///       断言内容不变，重命名前后均通过，证明重命名未改变行为。
/// </summary>
public class InventoryCooldownTests
{
    private GameObject inventoryGO;
    private Inventory inventory;
    private ItemData_Equipment armor;

    [SetUp]
    public void SetUp()
    {
        // 构造 Inventory：绕过 Start()（避免 UI 槽位/PlayerManager 依赖），
        // 直接反射初始化 canUseArmor 所需的最小状态
        inventoryGO = new GameObject("TestInventory");
        inventory = inventoryGO.AddComponent<Inventory>();
        Inventory.instance = inventory;

        armor = ScriptableObject.CreateInstance<ItemData_Equipment>();
        armor.itemName = "测试护甲";
        armor.itemType = ItemType.Equipment;
        armor.equipmentType = EquipmentType.Armor;
        armor.itemCooldown = 10f;

        // canUseArmor 依赖 equipmentDictianory（public，直接赋值）
        inventory.equipmentDictianory = new Dictionary<ItemData_Equipment, InventoryItem>
        {
            { armor, new InventoryItem(armor) }
        };

        // 私有冷却字段反射重置（等价于 Start() 中的初始化值）
        SetPrivate("lastTimeUsedArmor", -999f);
        SetPrivate("armorCooldown", 0f);
    }

    [TearDown]
    public void TearDown()
    {
        Inventory.instance = null;
        Object.DestroyImmediate(inventoryGO);
        Object.DestroyImmediate(armor);
    }

    private void SetPrivate(string name, float value) =>
        typeof(Inventory).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(inventory, value);

    private float GetPrivate(string name) =>
        (float)typeof(Inventory).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(inventory);

    [Test]
    public void CanUseArmor_FirstCall_ReturnsTrueAndRecordsTime()
    {
        Assert.IsTrue(inventory.canUseArmor(), "初始状态（-999）护甲应可用");
        Assert.AreEqual(Time.time, GetPrivate("lastTimeUsedArmor"), 0.001f,
            "可用时应记录使用时间");
        Assert.AreEqual(armor.itemCooldown, GetPrivate("armorCooldown"), 0.001f,
            "可用时应载入装备配置的冷却时长");
    }

    [Test]
    public void CanUseArmor_ImmediatelyAfterUse_ReturnsFalse()
    {
        inventory.canUseArmor(); // 首次使用
        Assert.IsFalse(inventory.canUseArmor(), "同一帧内连续调用应被冷却拦截");
    }

    [Test]
    public void CanUseArmor_NoArmorEquipped_ReturnsFalse()
    {
        inventory.equipmentDictianory.Clear();
        Assert.IsFalse(inventory.canUseArmor(), "未装备护甲时应返回 false");
    }

    [Test]
    public void CanUseArmor_StateAfterCooldownElapsed_ReturnsTrue()
    {
        // 模拟上次使用发生在 100 秒前（冷却 10 秒）
        SetPrivate("lastTimeUsedArmor", Time.time - 100f);
        SetPrivate("armorCooldown", 10f);
        Assert.IsTrue(inventory.canUseArmor(), "冷却时长已过时应恢复可用");
    }
}
