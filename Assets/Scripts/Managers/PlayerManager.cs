using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerManager（玩家管理器）- 单例模式管理玩家实例和货币
/// 设计思路：集中管理玩家引用和全局货币，避免在多个脚本中重复查找玩家
/// 使用场景：敌人获取玩家位置、商店扣费、存档加载货币
/// </summary>
public class PlayerManager : MonoBehaviour, ISaveManager
{
    public static PlayerManager instance; // 单例实例：全局唯一访问点
    public Player player;                 // 玩家引用：缓存避免频繁查找
    public int currency;                  // 当前货币（灵魂值）：用于购买装备和解锁技能

    /// <summary>
    /// 初始化单例：确保场景中只有一个PlayerManager
    /// </summary>
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    /// <summary>
    /// 检查是否有足够货币并扣除
    /// </summary>
    /// <param name="_price">需要支付的价格</param>
    /// <returns>是否有足够货币</returns>
    public bool HaveEnoughMoney(int _price)
    {
        if (_price > currency)
        {
            Debug.Log("没有足够的钱");
            return false;
        }
        currency -= _price; // 扣除货币
        return true;
    }

    /// <summary>
    /// 获取当前货币数量
    /// </summary>
    public int GetCurrency() => currency;

    /// <summary>
    /// 从存档加载货币数据
    /// </summary>
    public void LoadData(GameData _data)
    {
        currency = _data.currency;
    }

    /// <summary>
    /// 保存货币数据到存档
    /// </summary>
    public void SaveData(ref GameData _data)
    {
        _data.currency = this.currency;
    }
}
