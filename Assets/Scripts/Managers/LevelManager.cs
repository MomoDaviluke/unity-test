using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelManager（关卡管理器）- 管理关卡切换和叠加场景加载
/// 设计思路：使用叠加场景（Additive）加载关卡，主场景保持常驻
/// 使用场景：通关后加载下一关、死亡后重启当前关卡、返回主菜单
/// </summary>
public class LevelManager : MonoBehaviour, ISaveManager
{
    // ==================== 单例 ====================
    public static LevelManager instance;

    // ==================== 关卡设置 ====================
    [Header("关卡设置")]
    [SerializeField] private List<string> levelNames = new List<string>
    {
        "Level1Scene",
        "Level2Scene"
    };

    // ==================== 当前关卡 ====================
    [Header("当前关卡")]
    public int currentLevelIndex = 0;
    public string currentLevelName { get; private set; }
    public bool isLoading { get; private set; }

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    // ==================== 启动初始化 ====================
    private void Start()
    {
        if (SaveManager.instance != null && !SaveManager.instance.HasSavedData())
        {
            currentLevelIndex = 0;
            MovePlayerToInitialPosition();
        }
        else
        {
            string currentSubScene = GetCurrentSubSceneName();
            if (!string.IsNullOrEmpty(currentSubScene))
            {
                currentLevelName = currentSubScene;
                currentLevelIndex = levelNames.IndexOf(currentLevelName);
                if (currentLevelIndex < 0)
                    currentLevelIndex = 0;
            }
        }
    }

    // ==================== 新游戏 ====================
    public void InitializeNewGame()
    {
        currentLevelIndex = 0;
        currentLevelName = "Level1Scene";
        MovePlayerToInitialPosition();
    }

    // ==================== 场景查询 ====================
    private string GetCurrentSubSceneName()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "MainScene" && scene.isLoaded)
                return scene.name;
        }
        return string.Empty;
    }

    // ==================== 关卡加载 ====================
    public void LoadNextLevel()
    {
        if (isLoading) return;

        if (currentLevelIndex < levelNames.Count - 1)
        {
            currentLevelIndex++;
            LoadLevel(levelNames[currentLevelIndex]);
        }
        else
        {
            Debug.Log("已经是最后一关！");
            LoadMainMenu();
        }
    }

    public void LoadLevel(string levelName)
    {
        if (isLoading || string.IsNullOrEmpty(levelName))
        {
            Debug.Log($"[LevelManager] 正在加载中或关卡名为空，跳过加载请求: {levelName}");
            return;
        }

        Debug.Log($"[LevelManager] 开始加载关卡: {levelName}");
        isLoading = true;

        if (levelName == "Level2Scene")
            MovePlayerToPosition(new Vector2(-7.47f, -3f));

        StartCoroutine(LoadLevelCoroutine(levelName));
    }

    // ==================== 加载协程 ====================
    // 先卸载旧场景，再叠加加载新场景
    private IEnumerator LoadLevelCoroutine(string levelName)
    {
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            Debug.Log($"[LevelManager] 卸载当前场景: {currentLevelName}");
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentLevelName);
            yield return unloadOperation;
            Debug.Log($"[LevelManager] 场景卸载完成: {currentLevelName}");
        }

        Debug.Log($"[LevelManager] 开始加载新场景: {levelName}");
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        yield return loadOperation;
        Debug.Log($"[LevelManager] 场景加载完成: {levelName}");

        currentLevelName = levelName;
        isLoading = false;

        if (levelName == "Level1Scene" || levelName == "Level2Scene")
        {
            Debug.Log($"[LevelManager] 定位玩家到 {levelName} 的初始位置");
            MovePlayerToInitialPosition();
        }

        Debug.Log($"[LevelManager] 关卡加载完成：{levelName}");
    }

    /// <summary>
    /// 移动玩家到初始位置
    /// </summary>
    private void MovePlayerToInitialPosition()
    {
        StartCoroutine(MovePlayerToInitialPositionCoroutine());
    }

    /// <summary>
    /// 将玩家移动到指定位置
    /// </summary>
    public void MovePlayerToPosition(Vector2 position)
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.transform.position = position;
            Debug.Log($"[LevelManager] 玩家移动到: {position}");
        }
    }

    // ==================== 玩家位置管理 ====================
    // 移动玩家到初始位置，包含重试机制
    private IEnumerator MovePlayerToInitialPositionCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        Vector2 initialPosition = new Vector2(-7.47f, -3f);

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.transform.position = initialPosition;
            Debug.Log($"[LevelManager] 玩家已移动到初始位置: {initialPosition}");
        }
        else
        {
            yield return new WaitForSeconds(0.5f);

            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                PlayerManager.instance.player.transform.position = initialPosition;
                Debug.Log($"[LevelManager] 玩家已移动到初始位置（第二次尝试）: {initialPosition}");
            }
            else
            {
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.transform.position = initialPosition;
                    Debug.Log($"[LevelManager] 玩家已移动到初始位置（直接查找）: {initialPosition}");
                }
                else
                {
                    Debug.LogWarning($"[LevelManager] 玩家对象未找到");
                }
            }
        }
    }

    // ==================== 主菜单 ====================
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // ==================== 关卡重启 ====================
    public void RestartCurrentLevel()
    {
        SaveManager.instance?.SaveGame();
        StartCoroutine(ReloadCurrentLevelCoroutine());
    }

    private IEnumerator ReloadCurrentLevelCoroutine()
    {
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentLevelName);
            yield return unloadOperation;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(currentLevelName, LoadSceneMode.Additive);
        yield return loadOperation;
    }

    // ==================== 关卡查询 ====================
    public bool IsLastLevel()
    {
        return currentLevelIndex >= levelNames.Count - 1;
    }

    public int GetTotalLevels()
    {
        return levelNames.Count;
    }

    // ==================== 存档/加载 ====================
    public void LoadData(GameData _data)
    {
        currentLevelIndex = _data.currentLevelIndex;
        if (currentLevelIndex >= 0 && currentLevelIndex < levelNames.Count)
            LoadLevel(levelNames[currentLevelIndex]);
    }

    public void SaveData(ref GameData _data)
    {
        _data.currentLevelIndex = currentLevelIndex;
    }
}
