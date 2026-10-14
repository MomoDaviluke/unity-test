using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SmokeValidator（冒烟资产完整性校验）- batchmode 可执行的自动化冒烟检查
/// 覆盖 Phase 7 DoD 中可自动化的部分：
/// 1. 四个场景全部可加载，且所有 GameObject 无 Missing script 组件（GUID 断链）；
/// 2. 全部 Prefab 可加载内容并扫描，无 Missing script；
/// 3. 改名/移动过的关键组件在场景中仍可解析（GUID 绑定存活抽查）。
/// 用法：Unity -batchmode -executeMethod SmokeValidator.Run
/// 任何一项失败抛异常 → 非零退出码。
/// </summary>
public static class SmokeValidator
{
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/MainScene.unity",
        "Assets/Scenes/Level1Scene.unity",
        "Assets/Scenes/Level2Scene.unity",
    };

    // 关键类型抽查：场景路径关键字 -> 该场景必须存在的组件类型（改名/移动后 GUID 绑定验证）
    private static readonly (string sceneKeyword, string[] typeNames)[] TypePresenceChecks =
    {
        ("MainScene", new[]
        {
            "AudioManager", "PlayerManager", "SkillManager", "LevelManager",
            "SaveManager", "InputReader", "UI_InventoryPresenter", "Inventory",
        }),
    };

    public static void Run()
    {
        var problems = new List<string>();

        // ---- 1+3. 场景加载与 Missing script 扫描 ----
        foreach (string scenePath in Scenes)
        {
            if (!File.Exists(scenePath))
            {
                problems.Add($"场景文件不存在: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                problems.Add($"场景加载失败: {scenePath}");
                continue;
            }

            int missing = ScanSceneForMissingScripts(scene, scenePath, problems);
            Debug.Log($"[SmokeValidator] {scenePath}: root={scene.rootCount}, missing-script组件={missing}");

            CheckTypePresence(scene, scenePath, problems);
        }

        // ---- 2. Prefab 扫描 ----
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int prefabCount = 0;
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                problems.Add($"Prefab 加载失败: {path}");
                continue;
            }

            prefabCount++;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                Component[] comps = t.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null)
                        problems.Add($"Prefab Missing script: {path} @ {GetPath(t)} (index {i})");
                }
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log($"[SmokeValidator] Prefab 扫描完成: {prefabCount} 个");

        // ---- 汇总 ----
        if (problems.Count > 0)
        {
            foreach (string p in problems)
                Debug.LogError($"[SmokeValidator][FAIL] {p}");
            throw new Exception($"SmokeValidator 发现 {problems.Count} 个问题");
        }

        Debug.Log($"[SmokeValidator] PASS: {Scenes.Length} 场景 + {prefabCount} Prefab 全部无 Missing script，关键组件绑定存活");
    }

    private static int ScanSceneForMissingScripts(Scene scene, string scenePath, List<string> problems)
    {
        int missing = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                Component[] comps = t.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null)
                    {
                        missing++;
                        problems.Add($"场景 Missing script: {scenePath} @ {GetPath(t)} (index {i})");
                    }
                }
            }
        }
        return missing;
    }

    // 抽查：指定场景必须能解析出列出的脚本类型（GUID 绑定在改名/移动后仍有效）
    private static void CheckTypePresence(Scene scene, string scenePath, List<string> problems)
    {
        foreach (var (keyword, typeNames) in TypePresenceChecks)
        {
            if (!scenePath.Contains(keyword))
                continue;

            var found = new HashSet<string>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb != null)
                        found.Add(mb.GetType().Name);
                }
            }

            foreach (string typeName in typeNames)
            {
                if (!found.Contains(typeName))
                    problems.Add($"场景 {scenePath} 缺少关键组件类型: {typeName}（GUID 绑定可能断裂）");
            }
        }
    }

    private static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
