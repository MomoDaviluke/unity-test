using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemEffect（物品效果基类）- ScriptableObject，所有物品效果的基类
/// 功能：定义效果执行接口，子类重写实现具体效果
/// </summary>
public class ItemEffect : ScriptableObject
{
    [TextArea]
    public string effectDescription;

    public virtual void ExecuteEffect(Transform _enemyPosition)
    {
    }
}
