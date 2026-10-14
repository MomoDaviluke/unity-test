using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Phase 6 性能专项回归测试
/// 覆盖：
/// - PoolManager 增强：Spawn 挂接 parent、Despawn 脱回池根、销毁实例的出队容错；
/// - IsPlayerDetected 单帧缓存：同帧重复调用返回同一结果且射线只发一次（帧号字段推进一次）；
/// - NonAlloc/缓存化结构契约：反击状态/Skill/Crystal 不再使用 OverlapCircleAll（源码扫描）；
/// - TutorialTrigger 触发器化：无 Update 轮询、有运行时挂 Collider 与触发回调；
/// - 残影/命中特效池化结构契约。
/// </summary>
public class PerformanceTests
{
    private static string Read(string relativeFromAssets) =>
        File.ReadAllText(Path.Combine(Application.dataPath, relativeFromAssets));

    // ==================== PoolManager 增强 ====================
    [Test]
    public void PoolManager_SpawnWithParent_AttachesThenDespawnDetaches()
    {
        GameObject template = new GameObject("fx_template");
        GameObject target = new GameObject("fx_target");

        GameObject inst = PoolManager.Instance.Spawn(template, Vector3.zero, Quaternion.identity, target.transform);
        Assert.AreEqual(target.transform, inst.transform.parent, "带 parent 的 Spawn 必须挂接到目标（保持特效跟随行为）");

        PoolManager.Instance.Despawn(inst);
        Assert.AreNotEqual(target.transform, inst.transform.parent,
            "Despawn 必须脱回池根，避免池持有随父销毁的引用");
        Assert.IsFalse(inst.activeSelf, "Despawn 后实例应未激活");

        UnityEngine.Object.DestroyImmediate(template);
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(PoolManager.Instance.gameObject);
    }

    [Test]
    public void PoolManager_SkipsDestroyedInstances_OnDequeue()
    {
        GameObject template = new GameObject("dequeue_template");

        GameObject first = PoolManager.Instance.Spawn(template, Vector3.zero, Quaternion.identity);
        PoolManager.Instance.Despawn(first);

        // 模拟父对象销毁连带回收池中的实例：直接销毁池中对象
        UnityEngine.Object.DestroyImmediate(first);

        GameObject second = PoolManager.Instance.Spawn(template, Vector3.zero, Quaternion.identity);
        Assert.AreNotSame(first, second, "已销毁实例必须被跳过（容错出队），返回有效新实例");
        Assert.IsNotNull(second);

        UnityEngine.Object.DestroyImmediate(template);
        UnityEngine.Object.DestroyImmediate(PoolManager.Instance.gameObject);
    }

    // ==================== IsPlayerDetected 单帧缓存 ====================
    [Test]
    public void IsPlayerDetected_SameFrame_ReturnsCachedResult()
    {
        GameObject go = new GameObject("ray_enemy");
        Enemy enemy = go.AddComponent<Enemy>();

        // 反射装配最小依赖（EditMode 不调 Awake）
        typeof(Entity).GetField("groundCheck", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            ?.SetValue(enemy, go.transform);
        typeof(Enemy).GetField("isPlayer", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(enemy, new LayerMask());   // 空掩码：射线无命中，行为确定

        RaycastHit2D r1 = enemy.IsPlayerDetected();
        int frameAfterFirst = (int)typeof(Enemy).GetField("playerDetectFrame",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(enemy);

        RaycastHit2D r2 = enemy.IsPlayerDetected();
        int frameAfterSecond = (int)typeof(Enemy).GetField("playerDetectFrame",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(enemy);

        Assert.AreEqual(r1.distance, r2.distance, "同帧两次调用结果必须一致（缓存复用）");
        Assert.AreEqual(frameAfterFirst, frameAfterSecond,
            "同帧第二次调用不得重发射线（帧号只推进一次 = 缓存生效）");

        UnityEngine.Object.DestroyImmediate(go);
    }

    // ==================== NonAlloc / 缓存化 结构契约 ====================
    [TestCase("Scripts/Player/PlayerCounterAttackState.cs")]
    [TestCase("Scripts/Skills/Skill.cs")]
    [TestCase("Scripts/Skills/Skill_Controllers/Crystal_Skill_Controller.cs")]
    public void HotPaths_UseNonAlloc_InsteadOfOverlapCircleAll(string file)
    {
        string source = Read(file);
        Assert.IsFalse(source.Contains("Physics2D.OverlapCircleAll"),
            $"{file} 仍使用 OverlapCircleAll（每帧/高频分配数组，Phase 6 应改 NonAlloc）");
    }

    [Test]
    public void Crystal_Controller_CachesComponents_InAwake()
    {
        Type t = typeof(Crystal_Skill_Controller);
        Assert.IsNotNull(t.GetField("anim", BindingFlags.NonPublic | BindingFlags.Instance),
            "anim 应为缓存字段而非表达式属性");
        Assert.IsNotNull(t.GetField("cd", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(t.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
            "Crystal 应在 Awake 缓存组件");
        Assert.IsNotNull(t.GetField("overlapBuffer", BindingFlags.NonPublic | BindingFlags.Instance),
            "应有 NonAlloc 复用缓冲");
    }

    // ==================== TutorialTrigger 触发器化 ====================
    [Test]
    public void TutorialTrigger_NoUpdatePolling_UsesTriggerCallbacks()
    {
        Type t = typeof(TutorialTrigger);
        Assert.IsNull(t.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance),
            "TutorialTrigger 不得再有 Update 轮询（Phase 6 改触发回调）");
        Assert.IsNotNull(t.GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(t.GetMethod("OnTriggerExit2D", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(t.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
            "Awake 应运行时挂载触发 Collider（宿主原无 Collider，零场景改动）");
    }

    // ==================== 池化接入结构契约 ====================
    [Test]
    public void EntityFX_And_PlayerFX_UsePoolManager_NoRawInstantiateForFx()
    {
        string entityFx = Read("Scripts/Effects/EntityFX.cs");
        Assert.IsTrue(entityFx.Contains("PoolManager.Instance.Spawn(hitPrefab"),
            "命中特效应走池化 Spawn");
        Assert.IsTrue(entityFx.Contains("DespawnDelayed(newHitFx, 0.5f)"),
            "命中特效应延迟回池（保持 0.5 秒表现）");

        string playerFx = Read("Scripts/Effects/PlayerFX.cs");
        Assert.IsTrue(playerFx.Contains("PoolManager.Instance.Spawn(afterImagePrefab"),
            "残影应走池化 Spawn");

        string afterImage = Read("Scripts/Effects/AfterImageFX.cs");
        Assert.IsTrue(afterImage.Contains("PoolManager.Instance.Despawn(gameObject)"),
            "残影淡出结束应回池而非 Destroy");
        Assert.IsNotNull(typeof(AfterImageFX).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance),
            "残影必须有 OnEnable 复用重置（恢复初始 alpha）");
    }

    [Test]
    public void Enemy_IsPlayerDetected_HasFrameCacheFields()
    {
        Assert.IsNotNull(typeof(Enemy).GetField("playerDetectFrame", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.IsNotNull(typeof(Enemy).GetField("playerDetectResult", BindingFlags.NonPublic | BindingFlags.Instance));
    }
}
