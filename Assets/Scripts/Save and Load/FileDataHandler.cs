using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 文件数据处理类，负责游戏数据的保存、加载和删除
/// </summary>
public class FileDataHandler
{
    // 保存数据的目录路径
    private string dataDirPath = "";
    // 保存数据的文件名
    private string dataFileName = "";

    // 是否加密数据
    private bool encyptData = false;
    // 加密密钥
    private string codeWord = "Xu-20";

    /// <summary>
    /// 构造函数，初始化文件数据处理器
    /// </summary>
    /// <param name="_dataDirPath">数据保存目录路径</param>
    /// <param name="_dataFileName">数据文件名</param>
    /// <param name="_encyptData">是否加密数据</param>
    public FileDataHandler(string _dataDirPath, string _dataFileName, bool _encyptData)
    {
        dataDirPath = _dataDirPath;
        dataFileName = _dataFileName;
        encyptData = _encyptData;
    }

    /// <summary>
    /// 保存游戏数据到文件
    /// </summary>
    /// <param name="_data">要保存的游戏数据</param>
    public void Save(GameData _data)
    {
        // 组合完整的文件路径
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            // 确保目录存在，如果不存在则创建
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // 将游戏数据转换为JSON字符串
            string dataToStore = JsonUtility.ToJson(_data, true);

            // 如果需要加密，则加密数据
            if (encyptData)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // 使用文件流写入数据
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("保存数据错误: " + fullPath + "\n" + e);
        }
    }

    /// <summary>
    /// 从文件加载游戏数据
    /// </summary>
    /// <returns>加载的游戏数据，如果加载失败则返回null</returns>
    public GameData Load()
    {
        // 组合完整的文件路径
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadData = null;

        // 检查文件是否存在
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                // 使用文件流读取数据
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // 如果数据已加密，则解密
                if (encyptData)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // 将JSON字符串转换为GameData对象
                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("读取数据错误: " + fullPath + "\n" + e);
                // 文件损坏或格式不正确，删除该文件
                Debug.LogWarning("检测到存档文件可能已被篡改或损坏，正在删除...");
                try
                {
                    File.Delete(fullPath);
                    Debug.Log("已删除损坏的存档文件: " + fullPath);
                }
                catch (Exception deleteEx)
                {
                    Debug.LogError("无法删除损坏的存档文件: " + fullPath + "\n" + deleteEx);
                }

                // 返回null，让游戏创建新存档
                loadData = null;
            }
        }

        return loadData;
    }

    /// <summary>
    /// 删除保存的游戏数据文件
    /// </summary>
    public void Delete()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    /// <summary>
    /// 使用异或(XOR)操作加密或解密数据
    /// 由于XOR操作的可逆性，同一个方法可以用于加密和解密
    /// </summary>
    /// <param name="_data">要加密或解密的数据</param>
    /// <returns>加密或解密后的数据</returns>
    private string EncryptDecrypt(string _data)
    {
        string modifiedData = "";
        for (int i = 0; i < _data.Length; i++)
        {
            // 使用密钥中的字符与数据进行异或操作
            modifiedData += (char)(_data[i] ^ codeWord[i % codeWord.Length]);
        }
        return modifiedData;
    }
}
