using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// 测试专用单例子类（顶层类，确保 AddComponent 可用；嵌套 MonoBehaviour 在部分 Unity 版本无法挂载）
internal class TestSingleton : Singleton<TestSingleton> { }

/// <summary>
/// Singleton&lt;T&gt; 基类与 7 个 Manager 迁移的回归测试（Phase 2）
/// 覆盖：
/// - 行为契约：首个实例赋值 instance、判重"保留首个销毁后来者"、销毁后清空静态引用；
/// - 结构契约：Manager 必须继承 Singleton&lt;自身&gt; 且不得再声明派生 instance 字段（防复制粘贴回退）；
/// - B8 执行顺序：[DefaultExecutionOrder] 值符合方案（Save -300 / 中间层 -200 / Level -100）。
/// </summary>
public class SingletonTests
{
    // ==================== 行为契约 ====================
    // 说明：EditMode 下 AddComponent 不会同步触发 Awake/OnDestroy（Unity 仅在播放模式/域重载调用），
    // 故行为测试通过反射显式调用生命周期方法验证契约；生产代码不做任何测试特化。
    // 用例隔离：DestroyImmediate 在 EditMode 同样不触发 OnDestroy，静态 instance 不会自动清空，
    // 必须在 TearDown 显式重置，否则跨用例泄漏（首个用例的残留会使后续用例误入判重分支）。
    private readonly System.Collections.Generic.List<GameObject> _spawned = new System.Collections.Generic.List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in _spawned)
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
        _spawned.Clear();
        TestSingleton.instance = null;
    }

    private static void InvokeLifecycle(Component comp, string method)
    {
        MethodInfo mi = comp.GetType().BaseType.GetMethod(method,
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(mi, $"基类缺少 {method} 方法");
        mi.Invoke(comp, null);
    }

    [Test]
    public void Singleton_FirstInstance_IsAssigned()
    {
        GameObject go = new GameObject("s1");
        _spawned.Add(go);
        TestSingleton comp = go.AddComponent<TestSingleton>();
        InvokeLifecycle(comp, "Awake");

        Assert.AreEqual(comp, TestSingleton.instance, "首个实例应被赋值为 instance");
    }

    [Test]
    public void Singleton_Duplicate_KeepsFirstDestroysNewcomer()
    {
        GameObject go1 = new GameObject("s1");
        _spawned.Add(go1);
        TestSingleton comp1 = go1.AddComponent<TestSingleton>();
        InvokeLifecycle(comp1, "Awake");

        GameObject go2 = new GameObject("s2");
        _spawned.Add(go2);
        TestSingleton comp2 = go2.AddComponent<TestSingleton>();
        InvokeLifecycle(comp2, "Awake");

        Assert.AreEqual(comp1, TestSingleton.instance, "判重必须保留首个实例（修复旧代码销毁旧留新的悬空引用）");
        // 后来者已被销毁：必须用 Unity 的 == 重载判空（NUnit IsNull 走引用语义，对已销毁对象无效；
        // 访问 comp2.gameObject 会抛 MissingReferenceException）
        Assert.IsTrue(comp2 == null, "后来者应被销毁");
    }

    [Test]
    public void Singleton_OnDestroy_ClearsStaticReference()
    {
        GameObject go = new GameObject("s1");
        _spawned.Add(go);
        TestSingleton comp = go.AddComponent<TestSingleton>();
        InvokeLifecycle(comp, "Awake");
        Assert.IsNotNull(TestSingleton.instance);

        InvokeLifecycle(comp, "OnDestroy");
        Assert.IsNull(TestSingleton.instance, "销毁后必须清空静态引用，防止场景重载后悬空");
    }

    // ==================== 结构契约 ====================
    [TestCase(typeof(SaveManager))]
    [TestCase(typeof(PlayerManager))]
    [TestCase(typeof(GameManager))]
    [TestCase(typeof(SkillManager))]
    [TestCase(typeof(AudioManager))]
    [TestCase(typeof(LevelManager))]
    [TestCase(typeof(TutorialManager))]
    public void Manager_InheritsSingleton_AndNoOwnInstanceField(Type managerType)
    {
        Type singletonBase = typeof(Singleton<>).MakeGenericType(managerType);
        Assert.IsTrue(singletonBase.IsAssignableFrom(managerType),
            $"{managerType.Name} 必须继承 Singleton<{managerType.Name}>");

        FieldInfo ownField = managerType.GetField("instance",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.IsNull(ownField,
            $"{managerType.Name} 不得再声明派生 instance 字段（遮蔽基类静态字段属复制粘贴回退）");
    }

    // ==================== B8 执行顺序 ====================
    [TestCase(typeof(SaveManager), -300)]
    [TestCase(typeof(PlayerManager), -200)]
    [TestCase(typeof(GameManager), -200)]
    [TestCase(typeof(SkillManager), -200)]
    [TestCase(typeof(AudioManager), -200)]
    [TestCase(typeof(TutorialManager), -200)]
    [TestCase(typeof(LevelManager), -100)]
    public void Manager_HasExpectedExecutionOrder(Type managerType, int expectedOrder)
    {
        var attr = (DefaultExecutionOrder)Attribute.GetCustomAttribute(managerType, typeof(DefaultExecutionOrder));
        Assert.IsNotNull(attr, $"{managerType.Name} 缺少 [DefaultExecutionOrder]（B8 时序固化）");
        Assert.AreEqual(expectedOrder, attr.order, $"{managerType.Name} 执行顺序应固化");
    }
}
