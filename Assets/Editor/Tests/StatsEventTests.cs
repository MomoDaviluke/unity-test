using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// onStatsChanged 属性变化事件测试（Phase 3 补充，方案文档 §3.3）
/// 覆盖：
/// - Stat 三个变更入口（AddModifier/RemoveModifier/SetDefaultValue）触发 onChanged；
/// - CharacterStats.Awake 绑定后，任意 Stat 变化汇聚为 onStatsChanged 广播；
/// - 订阅时机无关性（绑定用 lambda 读最新字段，Awake 后订阅仍能收到）；
/// - UI_StatSlot 订阅/退订结构；Dodge_Skill 不再显式广播（单一传递路径）。
/// 说明：EditMode 不调用 Awake（PUD-06），行为测试反射显式驱动。
/// </summary>
public class StatsEventTests
{
    [TearDown]
    public void TearDown()
    {
        PlayerManager.instance = null;
    }

    // ==================== Stat 变更入口 ====================
    [Test]
    public void Stat_MutationMethods_TriggerOnChanged()
    {
        Stat stat = new Stat();
        int fired = 0;
        stat.onChanged = () => fired++;

        stat.AddModifier(5);
        Assert.AreEqual(1, fired, "AddModifier 应触发 onChanged");

        stat.RemoveModifier(5);
        Assert.AreEqual(2, fired, "RemoveModifier 应触发 onChanged");

        stat.SetDefaultValue(10);
        Assert.AreEqual(3, fired, "SetDefaultValue 应触发 onChanged");

        Assert.AreEqual(10, stat.GetValue(), "值计算应正确（修饰符已移除，基础值生效）");
    }

    // ==================== CharacterStats 汇聚 ====================
    [Test]
    public void CharacterStats_StatChange_BroadcastsOnStatsChanged()
    {
        GameObject go = new GameObject("Stats_Test");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.strength = new Stat();

        // EditMode 不调 Awake，反射驱动绑定
        MethodInfo awake = typeof(CharacterStats).GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(awake);
        awake.Invoke(stats, null);

        int fired = 0;
        Action handler = () => fired++;
        stats.onStatsChanged += handler;

        stats.strength.AddModifier(3);
        stats.strength.RemoveModifier(3);
        stats.strength.SetDefaultValue(7);

        stats.onStatsChanged -= handler;
        Assert.AreEqual(3, fired, "三类变更均应汇聚触发 onStatsChanged");

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void CharacterStats_SubscribeAfterBind_StillReceives()
    {
        GameObject go = new GameObject("Stats_Test");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.agility = new Stat();

        MethodInfo awake = typeof(CharacterStats).GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance);
        awake.Invoke(stats, null);

        // 绑定发生在订阅之前（模拟 UI 面板延迟打开）：lambda 读最新字段，仍能收到
        int fired = 0;
        Action handler = () => fired++;
        stats.onStatsChanged += handler;
        stats.agility.AddModifier(2);
        stats.onStatsChanged -= handler;

        Assert.AreEqual(1, fired, "Awake 后订阅的 UI 应能收到后续变化");

        UnityEngine.Object.DestroyImmediate(go);
    }

    // ==================== 订阅方结构契约 ====================
    [Test]
    public void UI_StatSlot_DeclaresSubscribeAndUnsubscribe()
    {
        Type t = typeof(UI_StatSlot);
        Assert.IsNotNull(t.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance),
            "UI_StatSlot 应 OnEnable 订阅 onStatsChanged");
        Assert.IsNotNull(t.GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance),
            "UI_StatSlot 应 OnDisable 退订防悬空");
        Assert.IsNotNull(t.GetField("subscribedStats", BindingFlags.NonPublic | BindingFlags.Instance),
            "应缓存订阅目标以对称退订");
    }

    [Test]
    public void Dodge_Skill_DoesNotBroadcastExplicitly()
    {
        string source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(Application.dataPath, "Scripts/Skills/Dodge_Skill.cs"));

        Assert.IsFalse(source.Contains("GameEvents.InventoryUpdated"),
            "Stat 变化已由 onStatsChanged 自动传递，Dodge_Skill 不应再显式广播（单一传递路径）");
    }
}
