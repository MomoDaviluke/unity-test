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
}
