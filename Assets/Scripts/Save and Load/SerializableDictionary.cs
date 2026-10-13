using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SerializableDictionary（可序列化字典）- 解决Unity无法直接序列化Dictionary的问题
/// 功能：继承Dictionary，实现ISerializationCallbackReceiver接口进行序列化/反序列化
/// </summary>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    // ==================== 序列化数据 ====================
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    // ==================== 序列化前：字典转列表 ====================
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> kvp in this)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    // ==================== 反序列化后：列表转字典 ====================
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            Debug.Log("键数和值数不相等");

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}
