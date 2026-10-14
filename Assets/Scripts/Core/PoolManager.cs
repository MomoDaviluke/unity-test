using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PoolManager（通用 GameObject 对象池）- Phase 2 基建
/// 设计思路：
/// - 运行时自举：无需在场景预置，首次访问 Instance 时创建 [PoolManager] 节点，避免改动场景资产；
/// - 按 prefab 引用分池：Spawn 优先复用池内未激活实例，池空时 Instantiate 新实例并挂到池根节点；
/// - Despawn 置未激活并回池；未登记的实例直接销毁（安全兜底，行为与原生 Destroy 一致）；
/// - 播放模式下常驻跨场景（DontDestroyOnLoad），池化对象挂池根下不随场景卸载销毁；
///   编辑器模式（EditMode 测试）跳过常驻调用。
/// 最小接入示例：EntityFX.CreatePopUpText（伤害数字，技术方案点名的第一高频生成源）。
/// </summary>
public class PoolManager : MonoBehaviour
{
    // ==================== 池数据结构 ====================
    private class Pool
    {
        public GameObject prefab;
        public readonly Queue<GameObject> instances = new Queue<GameObject>();
    }

    // ==================== 自举单例 ====================
    private static PoolManager instance;

    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("[PoolManager]");
                instance = go.AddComponent<PoolManager>(); // Awake 内完成判重与赋值
                if (Application.isPlaying)
                    DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // ==================== 内部状态 ====================
    private readonly Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();
    private readonly Dictionary<GameObject, Pool> instanceOwners = new Dictionary<GameObject, Pool>();
    private Transform poolRoot;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // ==================== 池根节点（懒创建） ====================
    private void EnsureRoot()
    {
        if (poolRoot == null)
        {
            GameObject root = new GameObject("PoolRoot");
            root.transform.SetParent(transform, false);
            poolRoot = root.transform;
        }
    }

    // ==================== 取出 ====================
    public GameObject Spawn(GameObject _prefab, Vector3 _position, Quaternion _rotation)
        => Spawn(_prefab, _position, _rotation, null);

    /// <summary>
    /// 取出实例。可选 parent：命中特效等需跟随目标移动的原 Instantiate(prefab, pos, rot, target)
    /// 场景保持同样挂接（世界坐标不变）；回池时由 Despawn 统一脱回池根。
    /// </summary>
    public GameObject Spawn(GameObject _prefab, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        if (_prefab == null)
        {
            Debug.LogError("[PoolManager] Spawn 目标 prefab 为空");
            return null;
        }

        EnsureRoot();

        if (!pools.TryGetValue(_prefab, out Pool pool))
        {
            pool = new Pool { prefab = _prefab };
            pools[_prefab] = pool;
        }

        // 容错出队：跳过已被销毁的实例（如挂接的父对象销毁连带回收池中的子特效）
        GameObject inst = null;
        while (pool.instances.Count > 0)
        {
            GameObject candidate = pool.instances.Dequeue();
            if (candidate != null)   // Unity == 重载：已销毁视为 null
            {
                inst = candidate;
                break;
            }
        }

        if (inst == null)
        {
            inst = Instantiate(_prefab);
            inst.transform.SetParent(poolRoot, false);
        }

        // 先挂接（保持世界坐标语义），再显式定位到请求位姿
        if (_parent != null)
            inst.transform.SetParent(_parent, true);
        else
            inst.transform.SetParent(poolRoot, false);

        inst.transform.SetPositionAndRotation(_position, _rotation);
        instanceOwners[inst] = pool;
        inst.SetActive(true);
        return inst;
    }

    // ==================== 归还 ====================
    public void Despawn(GameObject _instance)
    {
        if (_instance == null)
            return;

        if (instanceOwners.TryGetValue(_instance, out Pool pool))
        {
            // 脱回池根：解除动态挂接（命中特效曾挂敌人），避免池持有将随父销毁的引用
            _instance.transform.SetParent(poolRoot, false);
            _instance.SetActive(false);
            pool.instances.Enqueue(_instance);
        }
        else
        {
            // 非池化实例：兜底销毁（播放模式 Destroy；编辑器模式降级 DestroyImmediate）
            if (Application.isPlaying)
                Destroy(_instance);
            else
                DestroyImmediate(_instance);
        }
    }

    // ==================== 延迟归还 ====================
    // 替代 Instantiate + Destroy(go, delay) 模式（命中特效 0.5 秒生命周期）
    public void DespawnDelayed(GameObject _instance, float _delay)
    {
        if (_instance == null)
            return;

        if (!instanceOwners.ContainsKey(_instance))
        {
            // 非池化实例：保持原生延迟销毁行为
            Destroy(_instance, _delay);
            return;
        }

        StartCoroutine(DelayedDespawnRoutine(_instance, _delay));
    }

    private System.Collections.IEnumerator DelayedDespawnRoutine(GameObject _instance, float _delay)
    {
        yield return new WaitForSeconds(_delay);

        // 延迟期间被父对象销毁连带回收：清理归属记录，不入池
        if (_instance == null)
        {
            instanceOwners.Remove(_instance);
            yield break;
        }

        Despawn(_instance);
    }
}
