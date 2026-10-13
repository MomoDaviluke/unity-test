using System.Reflection;
using NUnit.Framework;

/// <summary>
/// B5/B9 回归契约测试（Phase 1 就地修复，B9 于 Phase 3 演进为事件总线）
/// - B5：Enemy_Demon 不得重新声明 chaseSpeed 遮蔽基类 Enemy.chaseSpeed，
///   否则 Enemy.SlowEntityBy 的减速改的是基类字段、DemonWalkState 读的是派生字段，减速对 Boss 无效。
/// - B9：PlayerDeadState 不得 GameObject.Find("Canvas")，也不得直接引用 UI；
///   死亡经 GameEvents.OnPlayerDead 广播、UI 订阅（Phase 3 最终形态，Phase 1 的 UI.instance 过渡方案已移除）。
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
    public void PlayerDeadState_MustNotReferenceUiOrFind()
    {
        string source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts/Player/PlayerDeadState.cs"));

        Assert.IsFalse(source.Contains("GameObject.Find"),
            "B9 回归：PlayerDeadState 不得按名字查找对象");
        Assert.IsFalse(source.Contains("UI.instance"),
            "B9 回归：PlayerDeadState 不得直接引用 UI（应经 GameEvents 广播，UI 订阅）");
        Assert.IsTrue(source.Contains("GameEvents.PlayerDead()"),
            "B9 最终形态：Enter 应经事件总线广播玩家死亡");
    }
}
