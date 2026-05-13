using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 关卡管理器：管理关卡切换和进度（支持叠加场景加载）
public class LevelManager : MonoBehaviour, ISaveManager
{
    public static LevelManager instance;
    
    [Header("关卡设置")]
    [SerializeField] private List<string> levelNames = new List<string>
    {
        "Level1Scene",
        "Level2Scene"
    };
    
    [Header("当前关卡")]
    public int currentLevelIndex = 0;
    public string currentLevelName { get; private set; }
    public bool isLoading { get; private set; }  // 是否正在加载中
    
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }
    
    private void Start()
    {
        // 检查是否是第一次启动（新游戏）
        // 如果SaveManager中没有存档数据，说明是新游戏
        if (SaveManager.instance != null && !SaveManager.instance.HasSavedData())
        {
            // 新游戏，加载第一关
            currentLevelIndex = 0;
            MovePlayerToInitialPosition();
        }
        else
        {
            // 有存档，检测当前加载的子场景
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

    // 新游戏初始化
    public void InitializeNewGame()
    {
        currentLevelIndex = 0;
        currentLevelName = "Level1Scene";
        MovePlayerToInitialPosition();
    }
    
    // 获取当前子场景名称
    private string GetCurrentSubSceneName()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            // 跳过主场景和已加载的场景
            if (scene.name != "MainScene" && scene.isLoaded)
                return scene.name;
        }
        return string.Empty;
    }
    
    // 加载下一关
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
    
    // 加载指定关卡（异步叠加加载）
    public void LoadLevel(string levelName)
    {
        if (isLoading || string.IsNullOrEmpty(levelName))
        {
            Debug.Log($"[LevelManager] 正在加载中或关卡名为空，跳过加载请求: {levelName}");
            return;
        }
        
        Debug.Log($"[LevelManager] 开始加载关卡: {levelName}");
        isLoading = true;
        
        // 先移动玩家到目标关卡的初始位置（避免切换时掉下去）
        if (levelName == "Level2Scene")
        {
            MovePlayerToPosition(new Vector2(-7.47f, -3f));
        }
        
        StartCoroutine(LoadLevelCoroutine(levelName));
    }
    
    // 异步加载关卡协程
    private IEnumerator LoadLevelCoroutine(string levelName)
    {
        // 卸载当前子场景
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            Debug.Log($"[LevelManager] 卸载当前场景: {currentLevelName}");
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentLevelName);
            yield return unloadOperation;
            Debug.Log($"[LevelManager] 场景卸载完成: {currentLevelName}");
        }
        
        // 加载新子场景
        Debug.Log($"[LevelManager] 开始加载新场景: {levelName}");
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        yield return loadOperation;
        Debug.Log($"[LevelManager] 场景加载完成: {levelName}");
        
        currentLevelName = levelName;
        isLoading = false;
        
        // 如果是第一个场景（Level1Scene），将玩家传送到初始位置
        if (levelName == "Level1Scene" || levelName == "Level2Scene")
        {
            Debug.Log($"[LevelManager] 定位玩家到 {levelName} 的初始位置");
            MovePlayerToInitialPosition();
        }
        
        Debug.Log($"[LevelManager] 关卡加载完成：{levelName}");
    }
    
    // 将玩家移动到初始位置
    private void MovePlayerToInitialPosition()
    {
        // 等待一帧确保场景已加载
        StartCoroutine(MovePlayerToInitialPositionCoroutine());
    }
    
    // 直接移动玩家到指定位置
    public void MovePlayerToPosition(Vector2 position)
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.transform.position = position;
            Debug.Log($"[LevelManager] 玩家移动到: {position}");
        }
    }
    
    private IEnumerator MovePlayerToInitialPositionCoroutine()
    {
        // 等待足够的时间让所有场景加载完成
        yield return new WaitForSeconds(0.5f);
        
        // 初始位置坐标
        Vector2 initialPosition = new Vector2(-7.47f, -3f);
        
        // 尝试获取玩家
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.player.transform.position = initialPosition;
            Debug.Log($"[LevelManager] 玩家已移动到初始位置: {initialPosition}");
        }
        else
        {
            // 再次尝试
            yield return new WaitForSeconds(0.5f);
            
            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                PlayerManager.instance.player.transform.position = initialPosition;
                Debug.Log($"[LevelManager] 玩家已移动到初始位置（第二次尝试）: {initialPosition}");
            }
            else
            {
                // 最后尝试直接查找
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
    
    // 加载主菜单
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
    
    // 重新开始当前关卡
    public void RestartCurrentLevel()
    {
        SaveManager.instance?.SaveGame();
        StartCoroutine(ReloadCurrentLevelCoroutine());
    }
    
    // 重新加载当前关卡协程
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
    
    // 是否是最后一关
    public bool IsLastLevel()
    {
        return currentLevelIndex >= levelNames.Count - 1;
    }
    
    // 获取关卡总数
    public int GetTotalLevels()
    {
        return levelNames.Count;
    }
    
    // 存档接口
    public void LoadData(GameData _data)
    {
        currentLevelIndex = _data.currentLevelIndex;
        // 加载保存的关卡
        if (currentLevelIndex >= 0 && currentLevelIndex < levelNames.Count)
        {
            LoadLevel(levelNames[currentLevelIndex]);
        }
    }
    
    public void SaveData(ref GameData _data)
    {
        _data.currentLevelIndex = currentLevelIndex;
    }
}
