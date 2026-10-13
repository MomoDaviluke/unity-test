using NUnit.Framework;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档系统单元测试（基线健康 + 关键语义固化）
/// 覆盖：GameData 结构、FileDataHandler 的 JSON 读写、XOR 加密往返、SerializableDictionary 往返
/// 设计思路：FileDataHandler 为纯逻辑类（非 MonoBehaviour），可直接 new；
///          测试写入临时目录并在 TearDown 清理，避免污染真实存档
/// </summary>
public class SaveSystemTests
{
    private string tempDir;

    [SetUp]
    public void SetUp()
    {
        // 使用临时缓存目录，与真实存档隔离
        tempDir = Path.Combine(Application.temporaryCachePath, "SaveTests_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [Test]
    public void GameData_DefaultConstructor_InitializesAllCollections()
    {
        var data = new GameData();
        Assert.IsNotNull(data.skillTree);
        Assert.IsNotNull(data.inventory);
        Assert.IsNotNull(data.equipmentId);
        Assert.IsNotNull(data.checkpoints);
        Assert.IsNotNull(data.volumeSettings);
        Assert.AreEqual(0, data.currency);
        Assert.AreEqual(0, data.currentLevelIndex);
    }

    [Test]
    public void SaveLoad_PlainText_RoundTripsScalarAndCollections()
    {
        var handler = new FileDataHandler(tempDir, "data.sav", false);

        var original = new GameData();
        original.currency = 1234;
        original.currentLevelIndex = 2;
        original.closestCheckpointId = "cp-7";
        original.inventory.Add("sword_01", 3);
        original.skillTree.Add("dash", true);
        original.volumeSettings.Add("Music", 0.75f);
        original.equipmentId.Add("helm_02");

        handler.Save(original);
        var loaded = handler.Load();

        Assert.IsNotNull(loaded, "存档应能被读回");
        Assert.AreEqual(1234, loaded.currency);
        Assert.AreEqual(2, loaded.currentLevelIndex);
        Assert.AreEqual("cp-7", loaded.closestCheckpointId);
        Assert.AreEqual(3, loaded.inventory["sword_01"], "SerializableDictionary 往返应保留键值");
        Assert.AreEqual(true, loaded.skillTree["dash"]);
        Assert.AreEqual(0.75f, loaded.volumeSettings["Music"], 0.0001f);
        Assert.Contains("helm_02", loaded.equipmentId);
    }

    [Test]
    public void SaveLoad_Encrypted_RoundTripsCorrectly()
    {
        var handler = new FileDataHandler(tempDir, "data.sav", true); // 开启 XOR 加密

        var original = new GameData();
        original.currency = 999;
        original.inventory.Add("potion", 5);

        handler.Save(original);

        // 密文文件不应包含明文 "currency" 字段值结构（验证确实加密落盘）
        string raw = File.ReadAllText(Path.Combine(tempDir, "data.sav"));
        Assert.IsFalse(raw.Contains("\"currency\": 999"), "加密后明文不应原样出现在文件中");

        var loaded = handler.Load();
        Assert.IsNotNull(loaded);
        Assert.AreEqual(999, loaded.currency);
        Assert.AreEqual(5, loaded.inventory["potion"]);
    }

    [Test]
    public void Load_MissingFile_ReturnsNull()
    {
        var handler = new FileDataHandler(tempDir, "nonexistent.sav", false);
        Assert.IsNull(handler.Load(), "存档不存在时应返回 null 而非抛异常");
    }

    [Test]
    public void Delete_RemovesFile()
    {
        var handler = new FileDataHandler(tempDir, "data.sav", false);
        handler.Save(new GameData());
        Assert.IsTrue(File.Exists(Path.Combine(tempDir, "data.sav")));
        handler.Delete();
        Assert.IsFalse(File.Exists(Path.Combine(tempDir, "data.sav")));
    }
}
