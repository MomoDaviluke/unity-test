using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ItemDatabase 与 Inventory 物品解析测试（B1 回归防护）
/// 背景（B1）：Inventory 原先在编辑器下用 AssetDatabase 扫描 Assets/Data/Items，
///            打包后（#else 分支）却只回退到 startingItems，导致打包后加载存档时
///            所有非初始物品全部丢失。修复后统一使用显式登记的 ItemDatabase 资产。
/// 本测试守护两点：
///  1) 项目 ItemDatabase 资产必须登记 Data/Items 下的全部物品资产（登记完整性）；
///  2) Inventory.LoadData 只从注入的 ItemDatabase 解析物品，不再依赖资源扫描。
/// 说明：B1 的原始缺陷是"仅打包环境"问题，EditMode 无法直接复现，
///      故此处以"数据库为唯一解析来源"的契约测试作为回归防护。
/// </summary>
public class ItemDatabaseTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags Public = BindingFlags.Public | BindingFlags.Instance;

    [Test]
    public void ProjectItemDatabase_RegistersEveryItemAsset()
    {
        string[] dbGuids = AssetDatabase.FindAssets("t:ItemDatabase");
        Assert.AreEqual(1, dbGuids.Length, "项目中应存在且仅存在一个 ItemDatabase 资产");

        var database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(
            AssetDatabase.GUIDToAssetPath(dbGuids[0]));
        Assert.IsNotNull(database, "ItemDatabase 资产应能被加载");

        // 已登记物品 -> GUID 集合
        var registered = new HashSet<string>();
        foreach (var item in database.GetItems())
            if (item != null)
                registered.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item)));

        // Data/Items 下的全部物品资产（ItemData 与子类 ItemData_Equipment）
        var found = new HashSet<string>();
        foreach (string g in AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" }))
            found.Add(g);
        foreach (string g in AssetDatabase.FindAssets("t:ItemData_Equipment", new[] { "Assets/Data/Items" }))
            found.Add(g);

        Assert.Greater(found.Count, 0, "应至少扫描到一个物品资产");

        foreach (string g in found)
            Assert.IsTrue(registered.Contains(g),
                $"B1 回归：物品资产 {AssetDatabase.GUIDToAssetPath(g)} 未登记进 ItemDatabase，打包后会丢失");
    }

    [Test]
    public void GetItemById_ReturnsMatchingItem_AndNullForUnknown()
    {
        var a = ScriptableObject.CreateInstance<ItemData>();
        a.itemId = "id-a";
        var b = ScriptableObject.CreateInstance<ItemData>();
        b.itemId = "id-b";

        var database = NewDatabase(new List<ItemData> { a, b });

        Assert.AreSame(b, database.GetItemById("id-b"), "应返回匹配 itemId 的物品");
        Assert.IsNull(database.GetItemById("missing"), "未知 itemId 应返回 null");
        Assert.IsNull(database.GetItemById(null), "null itemId 应返回 null");
        Assert.IsNull(database.GetItemById(""), "空 itemId 应返回 null");

        Object.DestroyImmediate(a);
        Object.DestroyImmediate(b);
        Object.DestroyImmediate(database);
    }

    [Test]
    public void Inventory_LoadData_ResolvesOnlyFromInjectedDatabase()
    {
        // 数据库只登记 itemA；存档中同时含 itemA 与未登记的 itemB
        var itemA = ScriptableObject.CreateInstance<ItemData>();
        itemA.itemId = "test-a"; itemA.itemType = ItemType.Material; itemA.itemName = "A";
        var itemB = ScriptableObject.CreateInstance<ItemData>();
        itemB.itemId = "test-b"; itemB.itemType = ItemType.Material; itemB.itemName = "B";

        var database = NewDatabase(new List<ItemData> { itemA });

        var go = new GameObject("TestInventory_B1");
        var inventory = go.AddComponent<Inventory>();
        Inventory.instance = inventory;
        var t = typeof(Inventory);
        t.GetField("itemDatabase", Private).SetValue(inventory, database);
        t.GetField("loadedItems", Public).SetValue(inventory, new List<InventoryItem>());
        t.GetField("loadedEquipment", Public).SetValue(inventory, new List<ItemData_Equipment>());

        var data = new GameData();
        data.inventory = new SerializableDictionary<string, int>();
        data.inventory.Add("test-a", 2);
        data.inventory.Add("test-b", 5);

        inventory.LoadData(data);

        var loaded = (List<InventoryItem>)t.GetField("loadedItems", Public).GetValue(inventory);
        Assert.AreEqual(1, loaded.Count, "B1：只能解析登记在 ItemDatabase 中的物品");
        Assert.AreSame(itemA, loaded[0].data);
        Assert.AreEqual(2, loaded[0].stackSize);

        Inventory.instance = null;
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(itemA);
        Object.DestroyImmediate(itemB);
        Object.DestroyImmediate(database);
    }

    private static ItemDatabase NewDatabase(List<ItemData> items)
    {
        var database = ScriptableObject.CreateInstance<ItemDatabase>();
        typeof(ItemDatabase).GetField("items", Private).SetValue(database, items);
        return database;
    }
}
