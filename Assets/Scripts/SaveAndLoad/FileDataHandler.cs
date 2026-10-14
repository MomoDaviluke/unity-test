using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// FileDataHandler（文件数据处理）- 负责游戏数据的保存、加载和删除
/// 功能：JSON序列化、文件流读写、XOR加密/解密
/// </summary>
public class FileDataHandler
{
    // ==================== 文件路径 ====================
    private string dataDirPath = "";
    private string dataFileName = "";

    // ==================== 加密设置 ====================
    private bool encyptData = false;
    private string codeWord = "Xu-20";

    // ==================== 构造函数 ====================
    public FileDataHandler(string _dataDirPath, string _dataFileName, bool _encyptData)
    {
        dataDirPath = _dataDirPath;
        dataFileName = _dataFileName;
        encyptData = _encyptData;
    }

    // ==================== 保存数据 ====================
    public void Save(GameData _data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonUtility.ToJson(_data, true);

            if (encyptData)
                dataToStore = EncryptDecrypt(dataToStore);

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

    // ==================== 加载数据 ====================
    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadData = null;

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (encyptData)
                    dataToLoad = EncryptDecrypt(dataToLoad);

                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("读取数据错误: " + fullPath + "\n" + e);
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

                loadData = null;
            }
        }

        return loadData;
    }

    // ==================== 删除存档 ====================
    public void Delete()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    // ==================== XOR加密/解密 ====================
    private string EncryptDecrypt(string _data)
    {
        string modifiedData = "";
        for (int i = 0; i < _data.Length; i++)
        {
            modifiedData += (char)(_data[i] ^ codeWord[i % codeWord.Length]);
        }
        return modifiedData;
    }
}
