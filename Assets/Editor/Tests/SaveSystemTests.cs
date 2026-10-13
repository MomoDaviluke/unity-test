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

    // ==================== B7：存档版本号与迁移 ====================

    [Test]
    public void SaveLoad_PersistsVersionNumber()
    {
        // 注：写入前盖版本号由 SaveManager.SaveGame 负责（MonoBehaviour，此处不覆盖），
        // 本用例只验证 saveVersion 字段本身的序列化往返。
        var handler = new FileDataHandler(tempDir, "v.sav", false);
        var data = new GameData();
        data.saveVersion = GameData.CurrentVersion;
        handler.Save(data);

        var loaded = handler.Load();
        Assert.IsNotNull(loaded);
        Assert.AreEqual(GameData.CurrentVersion, loaded.saveVersion, "存档往返应保留版本号");
    }

    [Test]
    public void Load_LegacySaveWithoutVersion_IsDetectedAsZero()
    {
        // 模拟引入版本号之前的旧档：正常序列化后删除 saveVersion 字段
        var data = new GameData();
        data.currency = 7;
        string json = JsonUtility.ToJson(data, true);
        json = System.Text.RegularExpressions.Regex.Replace(json, @"\s*""saveVersion"":\s*\d+,?", "");
        File.WriteAllText(Path.Combine(tempDir, "legacy.sav"), json);

        var handler = new FileDataHandler(tempDir, "legacy.sav", false);
        var loaded = handler.Load();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(7, loaded.currency, "旧档其余字段应正常读取");
        Assert.AreEqual(0, loaded.saveVersion, "旧档缺失 saveVersion 时应视为 v0");
    }

    [Test]
    public void MigrateSaveData_UpgradesLegacySaveToCurrentVersion()
    {
        var legacy = new GameData();
        legacy.saveVersion = 0; // v0
        legacy.currency = 42;

        GameData.MigrateSaveData(legacy);

        Assert.AreEqual(GameData.CurrentVersion, legacy.saveVersion, "应迁移到当前版本");
        Assert.AreEqual(42, legacy.currency, "迁移不应影响既有数据");
        Assert.IsNotNull(legacy.inventory, "迁移后集合字段应可用");
    }

    [Test]
    public void MigrateSaveData_CurrentSave_IsNoOp()
    {
        var data = new GameData();
        data.saveVersion = GameData.CurrentVersion;
        data.currency = 99;

        GameData.MigrateSaveData(data);

        Assert.AreEqual(GameData.CurrentVersion, data.saveVersion);
        Assert.AreEqual(99, data.currency);
    }
}
