using System;

/// <summary>
/// GameEvents（全局静态事件总线）- Phase 2 基建
/// 设计思路：
/// - 纯 C# event（Action），不引入第三方库；逻辑层为生产者、表现层为订阅者，反转依赖方向；
/// - 订阅方必须在 OnEnable 订阅 / OnDisable 退订，防止悬空引用（静态事件持有已销毁对象是常见泄漏源）；
/// - Phase 2 只提供总线与最小生产者示例（OnCurrencyChanged 已在 PlayerManager.HaveEnoughMoney 接入）；
///   UI 轮询改事件、其余事件的生产者接入属 Phase 3（技术方案 §3.3）。
/// </summary>
public static class GameEvents
{
    // ==================== 事件声明 ====================
    public static event Action<int> OnCurrencyChanged;                 // 货币变化（新值）
    public static event Action<ItemData, int> OnItemPickedUp;          // 拾取物品（物品、数量）
    public static event Action<int> OnSlotUpdated;                     // 背包槽位刷新（槽位索引）
    public static event Action OnInventoryUpdated;                     // 库存整体刷新（Phase 3：增删/装备后触发）
    public static event Action<string> OnSkillCooldown;                // 技能进入冷却（技能标识）
    public static event Action OnPlayerDead;                           // 玩家死亡（Phase 3 接入生产者）

    // ==================== 触发入口（集中定义，便于检索调用点） ====================
    public static void CurrencyChanged(int _newAmount) => OnCurrencyChanged?.Invoke(_newAmount);
    public static void ItemPickedUp(ItemData _item, int _amount) => OnItemPickedUp?.Invoke(_item, _amount);
    public static void SlotUpdated(int _slotIndex) => OnSlotUpdated?.Invoke(_slotIndex);
    public static void InventoryUpdated() => OnInventoryUpdated?.Invoke();
    public static void SkillCooldown(string _skillId) => OnSkillCooldown?.Invoke(_skillId);
    public static void PlayerDead() => OnPlayerDead?.Invoke();
}
