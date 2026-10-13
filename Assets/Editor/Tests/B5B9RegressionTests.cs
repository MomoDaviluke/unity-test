using System.Reflection;
using NUnit.Framework;

/// <summary>
/// B5/B9 回归契约测试（Phase 1 就地修复）
/// - B5：Enemy_Demon 不得重新声明 chaseSpeed 遮蔽基类 Enemy.chaseSpeed，
///   否则 Enemy.SlowEntityBy 的减速改的是基类字段、DemonWalkState 读的是派生字段，减速对 Boss 无效。
/// - B9：UI 须暴露 public static instance，使 PlayerDeadState 等调用方不再依赖
///   GameObject.Find("Canvas")（改名即崩、运行时查找昂贵）。
/// 说明：两者均为结构契约测试——B5 的减速失效需 PlayMode 物理环境才能直接复现，
/// B9 的 Enter() 需完整 Player/状态机实例；故以结构断言做回归防护。
/// </summary>
public class B5B9RegressionTests
{
    // ==================== B5 ====================
    [Test]
    public void EnemyDemon_MustNotRedeclareChaseSpeed()
    {
        FieldInfo derivedField = typeof(Enemy_Demon).GetField(
            "chaseSpeed",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.IsNull(derivedField,
            "Enemy_Demon 重新声明了 chaseSpeed，遮蔽基类 Enemy.chaseSpeed，导致 SlowEntityBy 减速对 Boss 无效");
    }

    // ==================== B9 ====================
    [Test]
    public void UI_MustExposeStaticInstance()
    {
        FieldInfo instanceField = typeof(UI).GetField(
            "instance",
            BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(instanceField,
            "UI 缺少 public static instance，PlayerDeadState 只能退回 GameObject.Find(\"Canvas\")");
        Assert.AreEqual(typeof(UI), instanceField.FieldType,
            "UI.instance 必须为 UI 类型");
    }
}
