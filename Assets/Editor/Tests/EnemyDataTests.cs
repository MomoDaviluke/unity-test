using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EnemyData 数据化迁移的回归测试（Phase 4 第二轮）
/// 覆盖：
/// - 9 个 EnemyData 资产存在且字段值与迁移核对表逐项一致（"Inspector 数值搬运无误"的验收条件）；
/// - Enemy 组件块不再序列化公共战斗参数（数据源唯一化为 SO）；
/// - Enemy 持有 enemyData 序列化引用；运行时字段为 public 非序列化（状态类代码零改动的前提）。
/// </summary>
public class EnemyDataTests
{
    private const string DataDir = "Assets/Data/EnemyData/";

    // 核对表：资产名, stunDuration, stunX, stunY, move, chase, idle, battle, atkDist, atkCd, minCd, maxCd, agro
    [TestCase("EnemyData_Skeleton", 1f, 7f, 12f, 1.5f, 3f, 2f, 7f, 2f, 0.4f, 0.35f, 0.55f, 3f)]
    [TestCase("EnemyData_Skeleton_Alt", 1f, 7f, 12f, 2f, 3.5f, 2f, 7f, 1.5f, 0.4f, 1f, 2f, 3f)]
    [TestCase("EnemyData_Slime_Small", 2f, 10f, 12f, 3f, 4f, 2f, 5f, 1.4f, 0.4f, 0.25f, 0.5f, 3f)]
    [TestCase("EnemyData_Slime_Medium", 2f, 10f, 12f, 2f, 3f, 2f, 5f, 1.41f, 0.4f, 0.5f, 0.8f, 3f)]
    [TestCase("EnemyData_Slime_Big", 2.5f, 10f, 12f, 1.5f, 2f, 2f, 5f, 1.41f, 0.4f, 0.35f, 0.55f, 3f)]
    [TestCase("EnemyData_Archer", 1f, 10f, 12f, 3f, 3f, 2f, 7f, 15f, 0f, 1f, 2f, 3f)]
    [TestCase("EnemyData_DeathBringer", 1f, 10f, 12f, 1.5f, 3f, 0.7f, 7f, 4.23f, 2f, 1f, 2f, 3f)]
    [TestCase("EnemyData_Demon", 1f, 10f, 12f, 2f, 4f, 2f, 7f, 10f, 1f, 1f, 2f, 3f)]
    [TestCase("EnemyData_Shady", 1f, 10f, 12f, 1.5f, 3f, 2f, 7f, 1.5f, 0.5f, 1f, 2f, 3f)]
    public void EnemyDataAsset_MatchesMigrationChecklist(
        string assetName, float stun, float sx, float sy, float move, float chase,
        float idle, float battle, float atkDist, float atkCd, float minCd, float maxCd, float agro)
    {
        EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(DataDir + assetName + ".asset");
        Assert.IsNotNull(data, $"{assetName} 资产缺失或不可加载");

        Assert.AreEqual(stun, data.stunDuration, $"{assetName}.stunDuration");
        Assert.AreEqual(new Vector2(sx, sy), data.stunDirection, $"{assetName}.stunDirection");
        Assert.AreEqual(move, data.moveSpeed, $"{assetName}.moveSpeed");
        Assert.AreEqual(chase, data.chaseSpeed, $"{assetName}.chaseSpeed");
        Assert.AreEqual(idle, data.idleTime, $"{assetName}.idleTime");
        Assert.AreEqual(battle, data.battleTime, $"{assetName}.battleTime");
        Assert.AreEqual(atkDist, data.attackDistance, $"{assetName}.attackDistance");
        Assert.AreEqual(atkCd, data.attackCooldown, $"{assetName}.attackCooldown");
        Assert.AreEqual(minCd, data.minAttackCooldown, $"{assetName}.minAttackCooldown");
        Assert.AreEqual(maxCd, data.maxAttackCooldown, $"{assetName}.maxAttackCooldown");
        Assert.AreEqual(agro, data.agroDistance, $"{assetName}.agroDistance");
    }

    // ==================== 数据源唯一化 ====================
    // 提取敌人组件块（按子类脚本 GUID 定位），断言块内无旧数值键、有 enemyData 引用。
    // 块级检查避免同文件其他组件（如 Player）的同名字段误报。
    [TestCase("Assets/Prefabs/Enemies/Enemy.prefab", "4a1fe4826e5bb764583a728d1aae19de")]
    [TestCase("Assets/Prefabs/Enemy_Skeleton.prefab", "4a1fe4826e5bb764583a728d1aae19de")]
    [TestCase("Assets/Prefabs/Enemies/Slime_Smail.prefab", "dbbcbc8d08bb61f4a9fbbf74e88d6968")]
    [TestCase("Assets/Prefabs/Enemies/Slime_Medium.prefab", "dbbcbc8d08bb61f4a9fbbf74e88d6968")]
    [TestCase("Assets/Prefabs/Enemies/Slime_Big.prefab", "dbbcbc8d08bb61f4a9fbbf74e88d6968")]
    [TestCase("Assets/Prefabs/Enemies/Enemy_Archer.prefab", "d65a97819b0a7bf459959572ec608da9")]
    [TestCase("Assets/Prefabs/Enemies/Enemy_Boss.prefab", "b25e8397b0b04e341af3ba9fda3d454e")]
    [TestCase("Assets/Prefabs/Enemies/Shady.prefab", "0abec15b08f26e9468317ac22c7f4684")]
    [TestCase("Assets/Scenes/Level2Scene.unity", "befa5b89ef52f8c4990789582cb8b348")]
    public void EnemyInstance_NoLongerSerializesCombatParams(string assetPath, string scriptGuid)
    {
        string text = System.IO.File.ReadAllText(assetPath);
        int idx = text.IndexOf("guid: " + scriptGuid);
        Assert.IsTrue(idx >= 0, $"{assetPath} 未找到敌人组件块（GUID {scriptGuid}）");

        int blockStart = text.LastIndexOf("--- !u!", idx);
        int blockEnd = text.IndexOf("\n---", idx);
        if (blockEnd < 0) blockEnd = text.Length;
        string block = text.Substring(blockStart, blockEnd - blockStart);

        foreach (string key in new[] { "moveSpeed:", "chaseSpeed:", "idleTime:", "battleTime:",
                                        "attackDistance:", "attackCooldown:", "minAttackCooldown:",
                                        "maxAttackCooldown:", "agroDistance:", "stunDuration:", "stunDirection:" })
        {
            Assert.IsFalse(block.Contains("  " + key),
                $"{assetPath} 敌人块仍序列化 {key.TrimEnd(':')}（数据源应唯一化为 EnemyData）");
        }

        Assert.IsTrue(block.Contains("enemyData: {fileID: 11400000, guid:"),
            $"{assetPath} 敌人块缺少 enemyData 引用");
    }

    [Test]
    public void Enemy_HasEnemyDataReference_AndRuntimeFields()
    {
        FieldInfo dataField = typeof(Enemy).GetField("enemyData",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(dataField, "Enemy 应持有 enemyData 序列化引用");
        Assert.IsNotNull(dataField.GetCustomAttribute<UnityEngine.SerializeField>());

        // 运行时字段：public 但不再序列化（保持状态类 enemy.xxx 读法零改动）
        foreach (string name in new[] { "moveSpeed", "chaseSpeed", "idleTime", "battleTime",
                                        "attackDistance", "attackCooldown", "minAttackCooldown",
                                        "maxAttackCooldown", "agroDistance", "stunDuration" })
        {
            FieldInfo f = typeof(Enemy).GetField(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, $"Enemy.{name} 应保留为 public 运行时字段");
            Assert.IsNull(f.GetCustomAttribute<UnityEngine.SerializeField>(),
                $"Enemy.{name} 不应再序列化（数据源 = EnemyData）");
        }
    }
}
