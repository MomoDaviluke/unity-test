using NUnit.Framework;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Stat 属性系统单元测试（基线健康测试）
/// 目的：固化 Stat 的计算语义，作为重构期间的行为保护网
/// 设计思路：Stat 为纯逻辑类（非 MonoBehaviour），可直接 new 实例测试
/// </summary>
public class StatTests
{
    // 通过反射设置私有 baseValue（Stat 无对外设置基础值的构造入口时保持封装）
    private static void SetBase(Stat stat, int value) => stat.SetDefaultValue(value);

    // 工厂：Stat.modifiers 在脱离 Unity 序列化环境时默认为 null，
    // 直接 new 后调用 AddModifier/GetValue 会 NRE。测试需显式初始化列表。
    // （这也是一个隐患记录：Phase 1 可考虑给 modifiers 字段加默认初始化。）
    private static Stat NewStat()
    {
        var stat = new Stat();
        stat.modifiers = new System.Collections.Generic.List<int>();
        return stat;
    }

    [Test]
    public void GetValue_OnlyBase_ReturnsBase()
    {
        var stat = NewStat();
        SetBase(stat, 10);
        Assert.AreEqual(10, stat.GetValue());
    }

    [Test]
    public void GetValue_WithModifiers_ReturnsBasePlusSum()
    {
        var stat = NewStat();
        SetBase(stat, 10);
        stat.AddModifier(5);
        stat.AddModifier(3);
        Assert.AreEqual(18, stat.GetValue(), "最终值应为基础值 + 所有修饰符之和");
    }

    [Test]
    public void GetValue_WithNegativeModifiers_AllowsDebuff()
    {
        var stat = NewStat();
        SetBase(stat, 10);
        stat.AddModifier(-4);
        Assert.AreEqual(6, stat.GetValue(), "负修饰符（诅咒/减益）应生效");
    }

    [Test]
    public void RemoveModifier_RemovesOnlyOneInstance()
    {
        var stat = NewStat();
        SetBase(stat, 10);
        stat.AddModifier(5);
        stat.AddModifier(5);
        stat.RemoveModifier(5);
        Assert.AreEqual(15, stat.GetValue(), "List.Remove 应只移除一个同值修饰符（装备叠加语义依赖此行为）");
    }

    [Test]
    public void SetDefaultValue_ChangesBaseAndAffectsTotal()
    {
        var stat = NewStat();
        stat.AddModifier(5);
        SetBase(stat, 20);
        Assert.AreEqual(25, stat.GetValue());
    }
}
