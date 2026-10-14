using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameData（游戏数据）- 存储游戏中所有可保存数据
/// 功能：货币、技能树、库存、装备、检查点、音量设置、关卡索引
/// </summary>
[System.Serializable]
public class GameData
{
    // ==================== 存档版本 ====================
    // 当前存档结构版本；0 表示引入版本号之前的旧档（v0）。
    // 修改 GameData 结构（增删/改名/改类型字段）时必须递增此值，
    // 并在 MigrateSaveData 中追加对应迁移分支。
    public const int CurrentVersion = 1;

    // 存档版本号：由 SaveManager.SaveGame 在写入前盖上当前版本。
    public int saveVersion;

    // ==================== 玩家数据 ====================
    public int currency;

    // ==================== 技能与物品 ====================
    public SerializableDictionary<string, bool> skillTree;
    public SerializableDictionary<string, int> inventory;
    public List<string> equipmentId;

    // ==================== 检查点 ====================
    public SerializableDictionary<string, bool> checkpoints;
    public string closestCheckpointId;

    // ==================== 死亡掉落 ====================
    public float lostCurrencyX;
    public float lostCurrencyY;
    public int lostCurrencyAmount;

    // ==================== 关卡 ====================
    public int currentLevelIndex;

    // ==================== 音量设置 ====================
    public SerializableDictionary<string, float> volumeSettings;

    // ==================== 构造函数 ====================
    public GameData()
    {
        this.lostCurrencyX = 0;
        this.lostCurrencyY = 0;
        this.lostCurrencyAmount = 0;
        this.currency = 0;

        skillTree = new SerializableDictionary<string, bool>();
        inventory = new SerializableDictionary<string, int>();
        equipmentId = new List<string>();

        closestCheckpointId = string.Empty;
        checkpoints = new SerializableDictionary<string, bool>();
        volumeSettings = new SerializableDictionary<string, float>();
        currentLevelIndex = 0;
    }

    // ==================== 版本迁移 ====================
    /// <summary>
    /// 存档版本迁移：把旧档升级到 CurrentVersion。
    /// JsonUtility 反序列化会调用默认构造函数，旧档缺失的新字段会取构造函数默认值，
    /// 因此本方法主要负责结构性迁移（字段改名/类型变更/数据转换）并盖上版本号。
    /// </summary>
    public static void MigrateSaveData(GameData _data)
    {
        if (_data == null || _data.saveVersion >= CurrentVersion)
            return;

        int fromVersion = _data.saveVersion;

        // v0 → v1：引入 saveVersion 字段本身，无结构性变更，仅盖上版本号。
        // 未来版本在此追加分支： if (_data.saveVersion < 2) { ...转换... }

        _data.saveVersion = CurrentVersion;
        Debug.Log($"存档已从 v{fromVersion} 迁移到 v{CurrentVersion}");
    }
}
