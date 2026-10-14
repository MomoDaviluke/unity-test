using UnityEngine;

/// <summary>
/// EnemyData（敌人战斗参数模板）- Phase 4 数据化
/// 设计思路：ScriptableObject 作为**只读模板**，存放同类敌人的公共战斗数值；
/// Enemy.Awake 将值拷贝到实例字段后运行（moveSpeed/chaseSpeed/attackCooldown 会被
/// 减速/随机冷却等运行时逻辑修改，SO 被所有实例共享，直接写会跨实例污染）。
/// 新增敌人变体（同行为不同数值）= 新建 EnemyData 资产 + 预制体引用，无需改代码。
/// 范围决策（已与用户确认）：仅公共战斗参数；子类专属字段（slimeType/safeDistance/
/// 技能参数等）留在各敌人 Inspector；掉落表保留在 ItemDrop 组件。
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    // ==================== 反击与眩晕 ====================
    [Header("眩晕参数")]
    public float stunDuration = 1;                       // 眩晕持续时间
    public Vector2 stunDirection = new Vector2(10, 12);  // 眩晕击退方向（X 水平 / Y 垂直）

    // ==================== 移动与巡逻 ====================
    [Header("移动参数")]
    public float moveSpeed = 1.5f;                       // 巡逻速度
    public float chaseSpeed = 3;                         // 追击速度
    public float idleTime = 2;                           // 待机停留时长
    public float battleTime = 7;                         // 丢失目标后的警惕持续时长

    // ==================== 攻击 ====================
    [Header("攻击参数")]
    public float attackDistance = 2;                     // 近战攻击触发距离
    public float attackCooldown = 2;                     // 攻击冷却（运行时会被随机化）
    public float minAttackCooldown = 1;                  // 随机冷却下限
    public float maxAttackCooldown = 2;                  // 随机冷却上限
    public float agroDistance = 3;                       // 仇恨距离（进入即追击）
}
