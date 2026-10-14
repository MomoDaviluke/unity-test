using UnityEngine;

/// <summary>
/// InputReader（输入读取集中层）- Phase 5 输入集中
/// 设计思路：
/// - 全项目按键 KeyCode 唯一配置处（序列化字段，Inspector 改键即全局生效）；
/// - 对外只暴露语义化读取（MoveX/JumpPressed/DashPressed...），调用方不再接触 KeyCode；
/// - 作为后续迁移新 Input System / 手柄重映射的隔离层：届时只换本类实现，调用方零改动；
/// - 继承 Singleton&lt;InputReader&gt;（Phase 2 基建），挂载于 MainScene Player 对象。
/// 行为等价约束：每个读取属性对应原代码的一处判定（含"手柄跳跃键仅参与跳跃缓冲、
/// 不参与蹬墙跳/跳高切断"这类原有细节，迁移时未擅自扩展）。
/// </summary>
public class InputReader : Singleton<InputReader>
{
    // ==================== 移动轴（Input Manager 轴名） ====================
    [Header("移动轴")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";

    // ==================== 跳跃 ====================
    [Header("跳跃")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode jumpAltKey = KeyCode.JoystickButton4;   // 手柄跳：仅参与跳跃缓冲（保持原行为）

    // ==================== 战斗 ====================
    [Header("战斗")]
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;             // 普通攻击（按住连段）
    [SerializeField] private KeyCode aimSwordKey = KeyCode.Mouse1;           // 飞剑瞄准/释放/召回

    // ==================== 技能 ====================
    [Header("技能")]
    [SerializeField] private KeyCode dashKey = KeyCode.Q;                    // 冲刺
    [SerializeField] private KeyCode parryKey = KeyCode.Q;                   // 反击（原与冲刺同键，独立字段便于日后分键）
    [SerializeField] private KeyCode blackholeKey = KeyCode.R;               // 黑洞
    [SerializeField] private KeyCode crystalKey = KeyCode.F;                 // 水晶
    [SerializeField] private KeyCode flaskKey = KeyCode.Alpha1;              // 药水瓶
    [SerializeField] private KeyCode dodgeKey = KeyCode.LeftControl;         // 闪避

    // ==================== 语义读取 ====================
    public float MoveX => Input.GetAxisRaw(horizontalAxis);
    public float MoveY => Input.GetAxisRaw(verticalAxis);
    public Vector3 MousePosition => Input.mousePosition;

    /// <summary>跳跃缓冲输入：空格或手柄跳键（原 Player.UpdateJumpBuffer 判定）</summary>
    public bool JumpPressed => Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpAltKey);

    /// <summary>蹬墙跳输入：仅空格（原 PlayerWallSlideState 判定，手柄键不参与）</summary>
    public bool WallJumpPressed => Input.GetKeyDown(jumpKey);

    /// <summary>跳跃释放（可变跳高切断）：仅空格（原 PlayerJumpState 判定）</summary>
    public bool JumpReleased => Input.GetKeyUp(jumpKey);

    public bool AttackHeld => Input.GetKey(attackKey);
    public bool AimSwordPressed => Input.GetKeyDown(aimSwordKey);
    public bool AimSwordReleased => Input.GetKeyUp(aimSwordKey);
    public bool AimSwordHeld => Input.GetKey(aimSwordKey);

    public bool DashPressed => Input.GetKeyDown(dashKey);
    public bool ParryPressed => Input.GetKeyDown(parryKey);
    public bool BlackholePressed => Input.GetKeyDown(blackholeKey);
    public bool CrystalPressed => Input.GetKeyDown(crystalKey);
    public bool FlaskPressed => Input.GetKeyDown(flaskKey);
    public bool DodgePressed => Input.GetKeyDown(dodgeKey);
}
