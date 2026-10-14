using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ItemType（物品类型枚举）- 定义物品的分类
/// </summary>
public enum ItemType
{
    Material,   // 材料：用于合成装备
    Equipment   // 装备：可穿戴，提供属性加成
}

/// <summary>
/// ItemData（物品数据基类）- ScriptableObject，定义物品基础属性
/// 设计思路：使用ScriptableObject存储物品数据，便于在编辑器中配置
/// 使用场景：所有物品（材料、装备、药水）的数据定义
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "Data/item")]
public class ItemData : ScriptableObject
{
    public ItemType itemType;       // 物品类型：材料或装备
    public string itemName;         // 物品名称
    public Sprite itemIcon;         // 物品图标
    public string itemId;           // 唯一标识符：用于存档识别

    [Range(0, 100)]
    public int dropChance;          // 掉落概率：0-100

    protected StringBuilder sb = new StringBuilder(); // 字符串构建器：用于生成描述文本

    /// <summary>
    /// 编辑器验证：自动生成唯一ID
    /// </summary>
    private void OnValidate()
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        itemId = AssetDatabase.AssetPathToGUID(path);
#endif
    }

    /// <summary>
    /// 获取物品描述：子类重写实现具体描述
    /// </summary>
    public virtual string GetDescription()
    {
        return "";
    }
}
