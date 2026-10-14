/// <summary>
/// SfxIds（音效索引常量表）- AudioManager.sfx 数组下标的语义命名
/// 设计思路：
/// - 索引与 MainScene 中 AudioManager 组件的 sfx 列表顺序一一对应（下标即数组位置）；
/// - 新增音效只在数组末尾追加、并在此登记新常量，避免魔法数字散落各调用点；
/// - 保持 PlaySFX(int)/StopSFX(int) 签名不变，AreaSound 等序列化下标场景不受影响。
/// 注意：改动已有常量值会破坏音效映射，只允许追加。
/// </summary>
public static class SfxIds
{
    public const int AttackSwing = 2;            // 玩家挥砍攻击
    public const int Blackhole = 3;              // 黑洞技能释放
    public const int Checkpoint = 5;             // 激活检查点
    public const int UIClick = 7;                // UI 面板交互音
    public const int Portal = 10;                // 传送门切换
    public const int Footsteps = 14;             // 脚步声（循环播放，需配合 StopSFX）
    public const int ItemPickup = 18;            // 拾取物品
    public const int EnemyIdleRoar = 24;         // 敌人进入待机的低吼
    public const int HeavyDamageFirst = 31;      // 重击受伤随机音效起始下标
    public const int HeavyDamageLastExclusive = 35; // 重击受伤随机音效结束下标（Random.Range 上界不含）
}
