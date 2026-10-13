using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stat（属性类）- 管理基础值和动态修饰符
/// 设计思路：将属性值拆分为基础值和修饰符列表，支持装备、Buff等临时增减
/// 使用场景：角色属性（力量、敏捷等）、装备加成、临时Buff效果
/// </summary>
[System.Serializable]
public class Stat
{
    // ==================== 属性值存储 ====================
    // 将属性拆分为基础值和修饰符列表，支持动态增减（装备、Buff、技能等）
    // baseValue是角色固有属性，modifiers是临时加成，最终值 = baseValue + sum(modifiers)
    [SerializeField] private int baseValue;     // 基础值：角色初始属性，不随装备改变
    public List<int> modifiers;                 // 修饰符列表：装备、Buff等提供的临时加成

    // ==================== 属性值计算 ====================
    // GetValue遍历所有修饰符并累加，得到当前最终属性值
    // 设计为每次实时计算，确保装备更换/Buff变化时立即生效

    /// <summary>
    /// 获取最终属性值：基础值 + 所有修饰符之和
    /// </summary>
    /// <returns>计算后的最终属性值</returns>
    public int GetValue()
    {
        int finalValue = baseValue;
        // 遍历所有修饰符并累加
        foreach (int modifier in modifiers)
            finalValue += modifier;
        return finalValue;
    }

    // ==================== 基础值管理 ====================
    // SetDefaultValue用于角色创建时初始化基础属性（如升级时增加力量）

    /// <summary>
    /// 设置基础值：用于初始化角色属性
    /// </summary>
    /// <param name="_value">要设置的基础值</param>
    public void SetDefaultValue(int _value)
    {
        baseValue = _value;
    }

    // ==================== 修饰符管理 ====================
    // AddModifier/RemoveModifier用于装备穿戴/卸下、Buff生效/结束等场景
    // 修饰符可为负数（如诅咒装备降低属性），使用List支持多个叠加

    /// <summary>
    /// 添加修饰符：装备穿戴或Buff生效时调用
    /// </summary>
    /// <param name="_modifier">要添加的修饰值（可为负数）</param>
    public void AddModifier(int _modifier)
    {
        modifiers.Add(_modifier);
    }

    /// <summary>
    /// 移除修饰符：装备卸下或Buff结束时调用
    /// </summary>
    /// <param name="_modifier">要移除的修饰值</param>
    public void RemoveModifier(int _modifier)
    {
        modifiers.Remove(_modifier);
    }
}
