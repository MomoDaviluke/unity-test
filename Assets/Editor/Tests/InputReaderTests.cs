using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Phase 5 Player 重构（InputReader 输入集中）回归测试
/// 覆盖：
/// - InputReader 结构契约：继承 Singleton、键字段全部序列化（Inspector 可改键）、语义读取属性齐全；
/// - 零硬编码扫描：Player 模块与玩家技能输入链源码不再出现 Input.Get*（KeyCode 集中于 InputReader 一处）；
/// - 跳跃收敛：PlayGroundedState 不再直判 Space/写缓冲（单一 CanJump 路径），
///   Player.Update 中计时器更新先于状态机驱动；
/// - partial 拆分：攻击/剑职责在 PlayerCombat，成员名不变；
/// - Die 事件化：Player 不再引用 SaveManager，SaveManager 订阅 OnPlayerDead；
/// - 场景挂载：MainScene Player 对象含 InputReader 组件块。
/// </summary>
public class InputReaderTests
{
    private static string PlayerDir => Path.Combine(Application.dataPath, "Scripts/Player");

    private static string Read(string relativeFromAssets) =>
        File.ReadAllText(Path.Combine(Application.dataPath, relativeFromAssets));

    // ==================== InputReader 结构 ====================
    [Test]
    public void InputReader_InheritsSingleton_AndKeysAreSerialized()
    {
        Assert.IsTrue(typeof(Singleton<InputReader>).IsAssignableFrom(typeof(InputReader)),
            "InputReader 应继承 Singleton<InputReader>（Phase 2 基建复用）");

        string[] keyFields = { "horizontalAxis", "verticalAxis", "jumpKey", "jumpAltKey",
                               "attackKey", "aimSwordKey", "dashKey", "parryKey",
                               "blackholeKey", "crystalKey", "flaskKey", "dodgeKey" };
        foreach (string name in keyFields)
        {
            FieldInfo f = typeof(InputReader).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, $"InputReader 缺少序列化键字段 {name}（改键须 Inspector 可配）");
            Assert.IsNotNull(f.GetCustomAttribute<SerializeField>(),
                $"InputReader.{name} 必须 [SerializeField]（DoD：改键在 Inspector 生效）");
        }
    }

    [Test]
    public void InputReader_ExposesSemanticReaders()
    {
        string[] props = { "MoveX", "MoveY", "MousePosition", "JumpPressed", "WallJumpPressed",
                           "JumpReleased", "AttackHeld", "AimSwordPressed", "AimSwordReleased",
                           "AimSwordHeld", "DashPressed", "ParryPressed", "BlackholePressed",
                           "CrystalPressed", "FlaskPressed", "DodgePressed" };
        foreach (string name in props)
            Assert.IsNotNull(typeof(InputReader).GetProperty(name, BindingFlags.Public | BindingFlags.Instance),
                $"InputReader 缺少语义读取 {name}");
    }

    // ==================== 零硬编码扫描 ====================
    [Test]
    public void PlayerModule_HardcodesNoInput()
    {
        string[] files =
        {
            "Scripts/Player/Player.cs",
            "Scripts/Player/PlayerState.cs",
            "Scripts/Player/PlayGroundedState.cs",
            "Scripts/Player/PlayerAirState.cs",
            "Scripts/Player/PlayerJumpState.cs",
            "Scripts/Player/PlayerWallSlideState.cs",
            "Scripts/Player/PlayerAimSwordState.cs",
            "Scripts/Player/PlayerDodgeState.cs",
            "Scripts/Skills/Sword_Skill.cs",
            "Scripts/Skills/Skill_Controllers/Blackhole_skill_controller.cs"
        };

        foreach (string file in files)
        {
            string source = Read(file);
            Assert.IsFalse(source.Contains("Input.GetKey"),
                $"{file} 仍硬编码 Input.GetKey*（应经 InputReader）");
            Assert.IsFalse(source.Contains("Input.GetMouseButton"),
                $"{file} 仍硬编码鼠标按钮（应经 InputReader）");
            Assert.IsFalse(source.Contains("Input.GetAxisRaw"),
                $"{file} 仍硬编码输入轴（应经 InputReader）");
            Assert.IsFalse(source.Contains("Input.mousePosition"),
                $"{file} 仍直读鼠标位置（应经 InputReader.MousePosition）");
        }
    }

    // ==================== 跳跃收敛 ====================
    [Test]
    public void GroundedState_NoDirectSpaceCheck_OnlyCanJump()
    {
        string source = Read("Scripts/Player/PlayGroundedState.cs");
        Assert.IsFalse(source.Contains("KeyCode.Space"),
            "地面状态不得再直判 Space（跳跃统一经 Player.CanJump 单一查询）");
        Assert.IsFalse(source.Contains("jumpBufferTimer ="),
            "地面状态不得再写 jumpBufferTimer（缓冲唯一写入点 = Player.UpdateJumpBuffer）");
        Assert.IsTrue(source.Contains("player.CanJump()"),
            "地面状态应调用 CanJump 查询");
    }

    [Test]
    public void Player_UpdatesTimers_BeforeStateMachine()
    {
        string source = Read("Scripts/Player/Player.cs");
        int coyoteIdx = source.IndexOf("UpdateCoyoteTime();", StringComparison.Ordinal);
        int bufferIdx = source.IndexOf("UpdateJumpBuffer();", StringComparison.Ordinal);
        int stateIdx = source.IndexOf("stateMachine.currentState.Update();", StringComparison.Ordinal);

        Assert.IsTrue(coyoteIdx >= 0 && bufferIdx >= 0 && stateIdx >= 0);
        Assert.IsTrue(coyoteIdx < stateIdx && bufferIdx < stateIdx,
            "跳跃计时器必须先于状态机更新（按键当帧 CanJump 即生效，消除帧延迟与双触发）");
    }

    // ==================== partial 拆分 ====================
    [Test]
    public void CombatResponsibilities_MovedToPlayerCombat()
    {
        Assert.IsTrue(File.Exists(Path.Combine(PlayerDir, "PlayerCombat.cs")),
            "PlayerCombat.cs 应存在（partial 拆分）");

        string combat = Read("Scripts/Player/PlayerCombat.cs");
        foreach (string member in new[] { "attackMovement", "counterAttackDuration", "isBusy",
                                          "lastDamageSource", "sword", "AssignNewSword",
                                          "CatchTheSword", "BusyFor" })
            Assert.IsTrue(combat.Contains(member), $"PlayerCombat 应包含 {member}");

        string player = Read("Scripts/Player/Player.cs");
        Assert.IsFalse(player.Contains("Vector2[] attackMovement"),
            "Player.cs 不应再声明攻击参数（已搬家）");
        Assert.IsTrue(player.Contains("public partial class Player"),
            "Player 必须声明为 partial");
    }

    // ==================== Die 事件化 ====================
    [Test]
    public void Player_Die_NoLongerReferencesSaveManager_DrivenByEvent()
    {
        string player = Read("Scripts/Player/Player.cs");
        Assert.IsFalse(player.Contains("SaveManager.instance"),
            "Player 不应直接调用 SaveManager（死亡存档由 OnPlayerDead 事件驱动）");

        string saveManager = Read("Scripts/Save and Load/SaveManager.cs");
        Assert.IsTrue(saveManager.Contains("GameEvents.OnPlayerDead += SaveGame"),
            "SaveManager 应订阅 OnPlayerDead 自动存档");
    }

    // ==================== 场景挂载 ====================
    [Test]
    public void MainScene_PlayerHasInputReaderComponent()
    {
        string scene = Read("Scenes/MainScene.unity");
        string guid = Read("Scripts/Core/InputReader.cs.meta").Split('\n')[1].Trim();
        Assert.IsTrue(scene.Contains("m_Script: {fileID: 11500000, " + guid + ", type: 3}")
            || scene.Contains("m_Script: {fileID: 11500000, " + guid.Replace("guid: ", "") + ", type: 3}"),
            "MainScene 应含 InputReader 组件块（Player 对象挂载）");
    }
}
