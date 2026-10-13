using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// SaveManager（存档管理器）- 协调游戏中所有需要保存/加载数据的对象
/// 功能：管理存档文件、协调各系统的存档/加载、新游戏初始化
/// </summary>
public class SaveManager : MonoBehaviour
{
    // ==================== 单例 ====================
    public static SaveManager instance;

    // ==================== 存档设置 ====================
    [SerializeField] private string fileName;
    [SerializeField] private bool encryptData;

    // ==================== 内部状态 ====================
    private GameData gameData;
    private List<ISaveManager> saveManagers = new List<ISaveManager>();
    private FileDataHandler dataHandler;

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

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

        if (this.gameData == null)
        {
            Debug.Log("没有找到存档");
            NewGame();
        }

        foreach (ISaveManager saveManager in saveManagers)
            saveManager.LoadData(gameData);
    }

    // ==================== 保存游戏 ====================
    public void SaveGame()
    {
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
