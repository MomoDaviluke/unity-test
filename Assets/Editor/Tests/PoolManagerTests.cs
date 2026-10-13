using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PoolManager 对象池与 PopUpText 池化接入的回归测试（Phase 2 基建）
/// 覆盖：
/// - 池行为：Spawn 返回激活实例、Despawn 停用回池、再次 Spawn 复用同一实例并更新位姿；
/// - 自举契约：首次访问 Instance 自动创建节点，销毁后静态引用清空；
/// - PopUpTextFx 复用重置契约（结构断言）：必须声明 Awake 捕获基准值 + OnEnable 重置，
///   否则池化复用时暴击字号/速度会跨次累积（行为级验证依赖 TMP 运行时，属 PlayMode 范畴）。
/// </summary>
public class PoolManagerTests
{
    // ==================== 池行为 ====================
    [Test]
    public void Pool_SpawnDespawn_ReusesSameInstance()
    {
        GameObject template = new GameObject("pool_template");

        GameObject p1 = PoolManager.Instance.Spawn(template, Vector3.zero, Quaternion.identity);
        Assert.IsNotNull(p1, "Spawn 应返回实例");
        Assert.IsTrue(p1.activeSelf, "Spawn 出的实例应处于激活状态");

        PoolManager.Instance.Despawn(p1);
        Assert.IsFalse(p1.activeSelf, "Despawn 应停用实例而非销毁");

        GameObject p2 = PoolManager.Instance.Spawn(template, new Vector3(1f, 2f, 0f), Quaternion.identity);
        Assert.AreSame(p1, p2, "回池实例应被复用，而非重新 Instantiate");
        Assert.AreEqual(new Vector3(1f, 2f, 0f), p2.transform.position, "Spawn 必须更新位姿");

        // 清理
        PoolManager.Instance.Despawn(p2);
        UnityEngine.Object.DestroyImmediate(template);
        UnityEngine.Object.DestroyImmediate(PoolManager.Instance.gameObject);
    }

    [Test]
    public void Pool_Bootstrap_ClearsStaticOnDestroy()
    {
        GameObject template = new GameObject("pool_template2");
        PoolManager pm = PoolManager.Instance;
        UnityEngine.Object.DestroyImmediate(template);

        Assert.IsNotNull(pm, "首次访问应自举创建 PoolManager");

        UnityEngine.Object.DestroyImmediate(pm.gameObject);
        // 销毁后再次访问应重新自举（静态引用已清空）
        PoolManager pm2 = PoolManager.Instance;
        Assert.AreNotSame(pm, pm2, "销毁后静态引用必须清空并重新自举");
        UnityEngine.Object.DestroyImmediate(pm2.gameObject);
    }

    // ==================== PopUpText 复用重置契约（结构） ====================
    [Test]
    public void PopUpTextFx_DeclaresPoolingResetContract()
    {
        System.Type t = typeof(PopUpTextFx);
        Assert.IsNotNull(t.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
            "PopUpTextFx 必须在 Awake 捕获基准值（池化实例 Start 只执行一次，不可依赖）");
        Assert.IsNotNull(t.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance),
            "PopUpTextFx 必须声明 OnEnable 重置复用状态（计时器/速度/字号/透明度）");
        Assert.IsNotNull(t.GetField("initialSpeed", BindingFlags.NonPublic | BindingFlags.Instance),
            "缺少 speed 基准值字段：B4 修复会把 speed 改为 disappearanceSpeed，复用时必须还原");
        Assert.IsNotNull(t.GetField("baseFontSize", BindingFlags.NonPublic | BindingFlags.Instance),
            "缺少 fontSize 基准值字段：暴击 fontSize += 2 复用时必须还原，否则跨次累积");
    }
}
