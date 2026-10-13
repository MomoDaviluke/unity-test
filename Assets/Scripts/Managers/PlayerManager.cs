using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// PlayerManager（玩家管理器）- 单例模式管理玩家实例和货币
/// 设计思路：集中管理玩家引用和全局货币，避免在多个脚本中重复查找玩家
/// 使用场景：敌人获取玩家位置、商店扣费、存档加载货币
/// Phase 2：继承 Singleton&lt;PlayerManager&gt;；执行顺序 -200
/// Phase 3：currency 收敛为属性，所有写入（击杀掉落/拾取/扣费/清零/读档）统一经 setter 触发 OnCurrencyChanged
/// </summary>
[DefaultExecutionOrder(-200)]
public class PlayerManager : Singleton<PlayerManager>, ISaveManager
{
    public Player player;                 // 玩家引用：缓存避免频繁查找（Inspector 序列化赋值）

    // 当前货币（灵魂值）：用于购买装备和解锁技能。
    // FormerlySerializedAs 保证 MainScene 中旧序列化键 currency 的值（1000）迁移到本字段。
    [FormerlySerializedAs("currency")] [SerializeField] private int currencyValue;

    /// <summary>
    /// 货币访问属性：任何赋值（含外部 += / = 0）都会广播变化，UI 无需轮询
    /// </summary>
    public int currency
    {
        get => currencyValue;
        set
        {
            currencyValue = value;
            GameEvents.CurrencyChanged(value);
        }
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
        currency -= _price; // 扣除货币（属性 setter 自动触发 OnCurrencyChanged）
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
