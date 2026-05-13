using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//2024.11.25
[System.Serializable]
public class GameData
{
    public int currency;

    public SerializableDictionary<string, bool> skillTree;
    public SerializableDictionary<string, int> inventory;//物品的名字和数量
    public List<string> equipmentId;//装备的ID

    public SerializableDictionary<string, bool> checkpoints;//检查点的ID和激活状态
    public string closestCheckpointId;//最近的检查点ID

    public float lostCurrencyX;
    public float lostCurrencyY;
    public int lostCurrencyAmount;

    public int currentLevelIndex;  // 当前关卡索引

    public SerializableDictionary<string, float> volumeSettings;


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