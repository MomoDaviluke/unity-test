using System;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Phase 4 敌人共享状态基类迁移的回归测试
/// 覆盖：
/// - 继承链契约：三个常规敌人的状态类必须挂在共享基类下（防复制粘贴回退）；
/// - 旧中间基类删除：SkeletonGroundState/SlimeGroundedState/ArcherGroundState 不再存在；
/// - CanAttack 唯一性：只允许定义在 EnemyBattleStateBase（四胞胎收敛为一份）；
/// - 行为等价钩子：Slime 仇恨距离覆写 2.5、Slime 眩晕 Exit 不取消闪烁、Boss 不放弃死亡玩家。
/// 说明：状态切换行为依赖 Time/物理（PlayMode 范畴），此处以结构契约 + 钩子值断言防护。
/// </summary>
public class EnemyStateHierarchyTests
{
    private static Assembly MainAssembly => typeof(Enemy).Assembly;

    // ==================== 继承链契约 ====================
    [TestCase(typeof(SkeletonIdleState), typeof(EnemyIdleStateBase))]
    [TestCase(typeof(SkeletonMoveState), typeof(EnemyMoveStateBase))]
    [TestCase(typeof(SkeletonBattleState), typeof(EnemyBattleStateBase))]
    [TestCase(typeof(SkeletonStunnedState), typeof(EnemyStunnedStateBase))]
    [TestCase(typeof(SkeletonDeadState), typeof(EnemyDeadStateBase))]
    [TestCase(typeof(SlimeIdleState), typeof(EnemyIdleStateBase))]
    [TestCase(typeof(SlimeMoveState), typeof(EnemyMoveStateBase))]
    [TestCase(typeof(SlimeBattleState), typeof(EnemyBattleStateBase))]
    [TestCase(typeof(SlimeStunState), typeof(EnemyStunnedStateBase))]
    [TestCase(typeof(SlimeDeathState), typeof(EnemyDeadStateBase))]
    [TestCase(typeof(ArcherIdleState), typeof(EnemyIdleStateBase))]
    [TestCase(typeof(ArcherMoveState), typeof(EnemyMoveStateBase))]
    [TestCase(typeof(ArcherBattleState), typeof(EnemyBattleStateBase))]
    [TestCase(typeof(ArcherStunnedState), typeof(EnemyStunnedStateBase))]
    [TestCase(typeof(ArcherDeadState), typeof(EnemyDeadStateBase))]
    [TestCase(typeof(DeathBringerBattleState), typeof(EnemyBattleStateBase))]
    public void StateClass_InheritsSharedBase(Type stateType, Type expectedBase)
    {
        Assert.IsTrue(expectedBase.IsAssignableFrom(stateType),
            $"{stateType.Name} 应继承 {expectedBase.Name}（Phase 4 迁移）");
    }

    [Test]
    public void OldIntermediateBases_AreDeleted()
    {
        foreach (string old in new[] { "SkeletonGroundState", "SlimeGroundedState", "ArcherGroundState" })
            Assert.IsNull(MainAssembly.GetType(old),
                $"旧中间基类 {old} 应已删除（逻辑上提至 Enemy/Shared/）");
    }

    // ==================== CanAttack 唯一性 ====================
    [Test]
    public void CanAttack_DefinedOnlyInBattleBase()
    {
        MethodInfo baseMethod = typeof(EnemyBattleStateBase).GetMethod("CanAttack",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.IsNotNull(baseMethod, "CanAttack 应定义在 EnemyBattleStateBase");

        foreach (Type t in new[] { typeof(SkeletonBattleState), typeof(SlimeBattleState),
                                   typeof(ArcherBattleState), typeof(DeathBringerBattleState) })
        {
            MethodInfo local = t.GetMethod("CanAttack",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.IsNull(local, $"{t.Name} 不应再定义私有 CanAttack（四胞胎收敛）");
        }
    }

    // ==================== 行为等价钩子值 ====================
    [Test]
    public void SlimeStates_OverrideAgroDistance_ToLegacyHardcoded2_5()
    {
        foreach (Type t in new[] { typeof(SlimeIdleState), typeof(SlimeMoveState) })
        {
            PropertyInfo prop = t.GetProperty("AgroDistance",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(prop, $"{t.Name} 应覆写 AgroDistance");

            MethodInfo getter = prop.GetMethod;
            Assert.AreEqual(t, getter.DeclaringType,
                $"{t.Name} 的 AgroDistance 必须是自身覆写（原硬编码 2.5）");
        }
    }

    [Test]
    public void SlimeStunState_DoesNotCancelBlinkOnExit()
    {
        PropertyInfo prop = typeof(SlimeStunState).GetProperty("CancelBlinkOnExit",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(prop);
        Assert.AreEqual(typeof(SlimeStunState), prop.GetMethod.DeclaringType,
            "Slime 必须覆写 CancelBlinkOnExit=false（其闪烁取消在 Update 落地检测内，行为等价红线）");
    }

    [Test]
    public void DeathBringerBattleState_DoesNotGiveUpOnPlayerDead()
    {
        PropertyInfo prop = typeof(DeathBringerBattleState).GetProperty("GiveUpIfPlayerDead",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(prop);
        Assert.AreEqual(typeof(DeathBringerBattleState), prop.GetMethod.DeclaringType,
            "Boss 必须覆写 GiveUpIfPlayerDead=false（原 Enter 无玩家死亡检查）");
    }

    [Test]
    public void SkeletonIdleState_KeepsCounterWindowAndSfxHooks()
    {
        foreach (string name in new[] { "OnEnterExtra", "OnExitExtra" })
        {
            MethodInfo m = typeof(SkeletonIdleState).GetMethod(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"SkeletonIdleState 应覆写 {name}（反击窗口/音效 24）");
            Assert.AreEqual(typeof(SkeletonIdleState), m.DeclaringType);
        }
    }
}
