using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// SaveManager（存档管理器）- 协调游戏中所有需要保存/加载数据的对象
/// 功能：管理存档文件、协调各系统的存档/加载、新游戏初始化
/// Phase 2：继承 Singleton&lt;SaveManager&gt;；执行顺序 -300（最先初始化，其余 Manager 依赖其存档数据分发）
/// </summary>
[DefaultExecutionOrder(-300)]
public class SaveManager : Singleton<SaveManager>
{
    // ==================== 存档设置 ====================
    [SerializeField] private string fileName;
    [SerializeField] private bool encryptData;

    // ==================== 内部状态 ====================
    private GameData gameData;
    private List<ISaveManager> saveManagers = new List<ISaveManager>();
    private FileDataHandler dataHandler;

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        saveManagers = FindAllSaveManagers();
        LoadGame();
    }

    // ==================== 存档管理 ====================
    [ContextMenu("Delete Save File")]
    public void DeleteSaveData()
    {
        if (dataHandler == null)
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);

        dataHandler.Delete();
        gameData = null;
    }

    public bool HasSavedData()
    {
        if (dataHandler.Load() != null)
            return true;

        return false;
    }

    // ==================== 新游戏 ====================
    public void NewGame()
    {
        gameData = new GameData();

        foreach (ISaveManager saveManager in saveManagers)
            saveManager.LoadData(gameData);
    }

    // ==================== 加载游戏 ====================
    public void LoadGame()
    {
        gameData = dataHandler.Load();

        if (gameData == null)
        {
            Debug.Log("没有找到存档");
            // 修复 B7：NewGame 内部已调用各 ISaveManager.LoadData，此处直接返回，
            // 避免下方再次遍历导致 LoadData 被重复调用。
            NewGame();
            return;
        }

        // 修复 B7：加载后按版本号迁移旧档
        GameData.MigrateSaveData(gameData);

        foreach (ISaveManager saveManager in saveManagers)
            saveManager.LoadData(gameData);
    }

    // ==================== 保存游戏 ====================
    public void SaveGame()
    {
        // 修复 B7：写入前盖上当前存档版本，供下次加载判断是否需要迁移
        gameData.saveVersion = GameData.CurrentVersion;

        foreach (ISaveManager saveManager in saveManagers)
            saveManager.SaveData(ref gameData);

        dataHandler.Save(gameData);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // ==================== 查找存档对象 ====================
    private List<ISaveManager> FindAllSaveManagers()
    {
        IEnumerable<ISaveManager> saveManagers = FindObjectsOfType<MonoBehaviour>().OfType<ISaveManager>();
        return new List<ISaveManager>(saveManagers);
    }
}
