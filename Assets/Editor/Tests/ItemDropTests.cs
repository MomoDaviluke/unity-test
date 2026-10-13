using NUnit.Framework;
using System.Reflection;
using UnityEngine;

/// <summary>
/// ItemDrop 掉落系统测试
/// 覆盖：B2 复现（hasDropped 永不重置导致重复调用失效 + 单次调用被截断为 1 件）
/// 设计思路：用纯代码组装 dropPrefab（Rigidbody2D + SpriteRenderer + ItemObject），
///          反射注入私有序列化字段，不依赖任何场景/预制体资产
/// 状态：B2 修复前，DropTwice 与 MaxCount 两个测试预期为红灯（复现 bug）
/// </summary>
public class ItemDropTests
{
    private GameObject dropGO;
    private ItemDrop itemDrop;
    private GameObject dropPrefabGO;
    private ItemData item100;

    [SetUp]
    public void SetUp()
    {
        // 掉落物模板：ItemObject.SetupItem 需要 Rigidbody2D 与 SpriteRenderer
        dropPrefabGO = new GameObject("DropPrefab");
        var rb = dropPrefabGO.AddComponent<Rigidbody2D>();
        dropPrefabGO.AddComponent<SpriteRenderer>();
        var itemObject = dropPrefabGO.AddComponent<ItemObject>();
        // ItemObject.rb 是 [SerializeField] 私有字段，代码组装时反射注入
        typeof(ItemObject).GetField("rb", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(itemObject, rb);
        // 注意：Instantiate 的克隆继承模板激活状态，模板必须保持激活，
        // 否则克隆非激活导致 CountDrops 漏计；模板本身在计数中按引用排除

        item100 = ScriptableObject.CreateInstance<ItemData>();
        item100.itemName = "必掉材料";
        item100.itemType = ItemType.Material;
        item100.dropChance = 100;

        dropGO = new GameObject("TestItemDrop");
        itemDrop = dropGO.AddComponent<ItemDrop>();

        SetPrivate("dropPrefab", dropPrefabGO);
        SetPrivate("possibleDrop", new[] { item100 });
        SetPrivate("PossilbleItemDrop", 3);
    }

    [TearDown]
    public void TearDown()
    {
        // 清理所有由 GenerateDrop 实例化的掉落物（场景中激活的 DropPrefab(Clone)）
        foreach (var obj in Object.FindObjectsOfType<ItemObject>())
            if (obj.gameObject != dropPrefabGO)
                Object.DestroyImmediate(obj.gameObject);

        Object.DestroyImmediate(dropGO);
        Object.DestroyImmediate(dropPrefabGO);
        Object.DestroyImmediate(item100);
    }

    private void SetPrivate(string name, object value) =>
        typeof(ItemDrop).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(itemDrop, value);

    private int CountDrops()
    {
        int count = 0;
        foreach (var obj in Object.FindObjectsOfType<ItemObject>())
            if (obj.gameObject != dropPrefabGO && obj.gameObject.activeInHierarchy)
                count++;
        return count;
    }

    // ==================== 绿灯：当前正确语义 ====================

    [Test]
    public void GenerateDrop_DropsAtLeastOne_WhenChanceIs100()
    {
        itemDrop.GenerateDrop();
        Assert.GreaterOrEqual(CountDrops(), 1, "100% 概率物品应至少掉落 1 件");
    }

    [Test]
    public void GenerateDrop_DropsNothing_WhenNoItemsConfigured()
    {
        SetPrivate("possibleDrop", new ItemData[0]);
        itemDrop.GenerateDrop();
        Assert.AreEqual(0, CountDrops(), "无可掉物品时不应生成实例");
    }

    // ==================== 红灯：复现 B2 ====================

    [Test]
    public void GenerateDrop_CalledTwice_ShouldDropOnSecondCallToo()
    {
        itemDrop.GenerateDrop();
        Assert.AreEqual(1, CountDrops());

        // B2：hasDropped 置 true 后永不重置 → 第二次调用被 DropItem 拦截，掉 0 件
        // 修复后此断言应通过（第二次调用同样掉落）
        itemDrop.GenerateDrop();
        Assert.GreaterOrEqual(CountDrops(), 2,
            "B2 复现：第二次 GenerateDrop 应再次掉落（当前实现因 hasDropped 永不重置而失效）");
    }

    [Test]
    public void GenerateDrop_MaxDropCount3_WithThreeGuaranteedItems_ShouldDropThree()
    {
        // 配置 3 个 100% 物品，PossilbleItemDrop=3
        var item2 = ScriptableObject.CreateInstance<ItemData>();
        item2.itemName = "必掉2"; item2.itemType = ItemType.Material; item2.dropChance = 100;
        var item3 = ScriptableObject.CreateInstance<ItemData>();
        item3.itemName = "必掉3"; item3.itemType = ItemType.Material; item3.dropChance = 100;
        SetPrivate("possibleDrop", new[] { item100, item2, item3 });

        // B2：DropItem 内 hasDropped 检查使循环第一次迭代后全部拦截 → 只掉 1 件
        // 修复后应掉 3 件
        itemDrop.GenerateDrop();
        Assert.AreEqual(3, CountDrops(),
            "B2 复现：配置 3 件必掉物品时应掉满 3 件（当前实现被 hasDropped 截断为 1 件）");

        Object.DestroyImmediate(item2);
        Object.DestroyImmediate(item3);
    }
}
