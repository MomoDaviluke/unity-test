using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/// <summary>
/// 保存管理器，负责协调游戏中所有需要保存/加载数据的对象
/// </summary>
public class SaveManager : MonoBehaviour
{
    // 单例实例
    public static SaveManager instance;

    // 保存文件名
    [SerializeField] private string fileName;
    // 是否加密数据
    [SerializeField] private bool encryptData;

    // 当前游戏数据
    private GameData gameData;
    // 所有实现ISaveManager接口的对象列表
    private List<ISaveManager> saveManagers = new List<ISaveManager>();
    // 文件数据处理器
    private FileDataHandler dataHandler;

    /// <summary>
    /// 在Unity编辑器中右键菜单选项，用于删除保存文件
    /// </summary>
    [ContextMenu("Delete Save File")]
    public void DeleteSaveData()
    {
        if (dataHandler == null)
            dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);

        dataHandler.Delete();
        // 重置内存中的游戏数据
        gameData = null;
    }

    /// <summary>
    /// 初始化单例实例
    /// </summary>
    private void Awake()
    {
        // 确保场景中只有一个SaveManager实例
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    /// <summary>
    /// 初始化并加载游戏数据
    /// </summary>
    private void Start()
    {
        // 创建文件数据处理器
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, encryptData);
        // 查找所有实现ISaveManager接口的对象
        saveManagers = FindAllSaveManagers();
        // 加载游戏数据
        LoadGame();
    }

    /// <summary>
    /// 创建新游戏数据
    /// </summary>
    public void NewGame()
    {
        gameData = new GameData();
        
        // 通知所有管理器初始化
        foreach (ISaveManager saveManager in saveManagers)
        {
            saveManager.LoadData(gameData);
        }
    }

    /// <summary>
    /// 加载游戏数据并分发给所有需要的对象
    /// </summary>
    public void LoadGame()
    {
        // 从文件加载游戏数据
        gameData = dataHandler.Load();

        // 如果没有找到保存的数据，创建新游戏数据
        if (this.gameData == null)
        {
            Debug.Log("没有找到存档");
            NewGame();
        }

        // 将加载的数据分发给所有实现ISaveManager接口的对象
        foreach (ISaveManager saveManager in saveManagers)
        {
            saveManager.LoadData(gameData);
        }
    }

    /// <summary>
    /// 收集所有对象的数据并保存到文件
    /// </summary>
    public void SaveGame()
    {
        // 从所有实现ISaveManager接口的对象收集数据
        foreach (ISaveManager saveManager in saveManagers)
        {
            saveManager.SaveData(ref gameData);
        }

        // 保存数据到文件
        dataHandler.Save(gameData);
    }

    /// <summary>
    /// 在应用退出时自动保存游戏
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// 查找场景中所有实现ISaveManager接口的对象
    /// </summary>
    /// <returns>实现ISaveManager接口的对象列表</returns>
    private List<ISaveManager> FindAllSaveManagers()
    {
        // 使用LINQ查找所有MonoBehaviour中实现了ISaveManager接口的对象
        IEnumerable<ISaveManager> saveManagers = FindObjectsOfType<MonoBehaviour>().OfType<ISaveManager>();
        return new List<ISaveManager>(saveManagers);
    }
    public bool HasSavedData()
    {
        if (dataHandler.Load() != null)
        {
            return true;
        }

        return false;
    }

}

