using UnityEngine;

/// <summary>
/// Singleton<T>（MonoBehaviour 单例基类）- Phase 2 基建
/// 设计思路：
/// - 保留小写 `instance` 静态字段命名，派生类经 `XxxManager.instance` 访问（C# 允许以派生类名访问继承的静态成员），
///   现有 55 处调用点零改动；
/// - 判重语义统一为"保留首个、销毁后来者"，修复旧代码"销毁旧实例却不重新赋值"导致的悬空引用隐患；
/// - DontDestroyOnLoadInstance 默认 false：本项目 7 个 Manager 全部位于 MainScene，
///   经 Single 模式重载（RestartScene/LoadMainMenu）；若常驻跨场景，重载后旧单例会存活
///   且新场景序列化引用断裂（技术方案风险表已警示）。
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // ==================== 单例访问点 ====================
    public static T instance;

    // ==================== 跨场景常驻开关（子类按需覆写为 true） ====================
    protected virtual bool DontDestroyOnLoadInstance => false;

    // ==================== 初始化 ====================
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            // 销毁后来者：播放模式用 Destroy；编辑器模式（EditMode 测试）Destroy 不可用，降级 DestroyImmediate
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
            return;
        }

        instance = this as T;

        if (DontDestroyOnLoadInstance && Application.isPlaying)
            DontDestroyOnLoad(gameObject);
    }

    // ==================== 销毁清理 ====================
    // 场景重载/对象销毁时清空静态引用，防止悬空引用导致后续判重逻辑误判
    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
