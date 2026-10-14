using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ISaveManager（存档接口）- 所有需要保存/加载数据的对象都需实现
/// 功能：提供统一的数据存取规范
/// </summary>
public interface ISaveManager
{
    void LoadData(GameData _data);
    void SaveData(ref GameData _data);
}
