using UnityEngine;

/// <summary>
/// UI_InventoryPresenter（库存 UI 呈现器）- Phase 3 表现层组件
/// 设计思路：依赖方向反转为"表现层→逻辑层"——本组件读取 Inventory 数据并刷新槽位，
/// Inventory 不再持有任何 UI 引用（原 UpdateSloatUI/UpdateStatsUI 逻辑迁移至此）。
/// 订阅 GameEvents.OnInventoryUpdated（OnEnable 订阅 / OnDisable 退订防悬空），
/// Start 订阅后立即主动拉一次初值（覆盖 Inventory.Start 与本组件 Start 的顺序不确定性）。
/// 挂载位置：与 Inventory 同一 GameObject（MainScene 的 inventory 节点）。
/// </summary>
public class UI_InventoryPresenter : MonoBehaviour
{
    // ==================== UI 槽位父节点（原 Inventory 字段，场景引用平移） ====================
    [Header("背包UI")]
    [SerializeField] private Transform inventorySlotParent;
    [SerializeField] private Transform stashSloatParent;
    [SerializeField] private Transform equipmentSlotParent;
    [SerializeField] private Transform statSlotParent;

    // ==================== 槽位组件缓存 ====================
    private UI_ItemSlot[] inventoryItemSlot;
    private UI_ItemSlot[] stashItemSlot;
    private UI_EquipmentSlot[] equipmentSlot;
    private UI_StatSlot[] statSlot;

    // ==================== 初始化 ====================
    void Start()
    {
        inventoryItemSlot = inventorySlotParent.GetComponentsInChildren<UI_ItemSlot>();
        stashItemSlot = stashSloatParent.GetComponentsInChildren<UI_ItemSlot>();
        equipmentSlot = equipmentSlotParent.GetComponentsInChildren<UI_EquipmentSlot>();
        statSlot = statSlotParent.GetComponentsInChildren<UI_StatSlot>();

        RefreshAll(); // 订阅后立即拉初值（标准做法，防漏首次刷新）
    }

    // ==================== 事件订阅 ====================
    private void OnEnable()
    {
        GameEvents.OnInventoryUpdated += OnInventoryUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnInventoryUpdated -= OnInventoryUpdated;
    }

    private void OnInventoryUpdated()
    {
        RefreshAll();
    }

    // ==================== 全量刷新（逻辑迁移自 Inventory.UpdateSloatUI） ====================
    private void RefreshAll()
    {
        Inventory inv = Inventory.instance;
        if (inv == null)
            return;

        // 清空装备槽
        for (int i = 0; i < equipmentSlot.Length; i++)
            equipmentSlot[i].CleanUpSlot();

        // 更新装备槽
        for (int i = 0; i < equipmentSlot.Length; i++)
        {
            foreach (var item in inv.equipmentDictianory)
            {
                if (item.Key.equipmentType == equipmentSlot[i].slotType)
                    equipmentSlot[i].UpdateSlots(item.Value);
            }
        }

        // 清空背包和仓库槽
        for (int i = 0; i < inventoryItemSlot.Length; i++)
            inventoryItemSlot[i].CleanUpSlot();
        for (int i = 0; i < stashItemSlot.Length; i++)
            stashItemSlot[i].CleanUpSlot();

        // 更新背包槽
        for (int i = 0; i < inv.inventory.Count && i < inventoryItemSlot.Length; i++)
            inventoryItemSlot[i].UpdateSlots(inv.inventory[i]);

        // 更新仓库槽
        for (int i = 0; i < inv.stash.Count && i < stashItemSlot.Length; i++)
            stashItemSlot[i].UpdateSlots(inv.stash[i]);

        // 更新属性槽
        for (int i = 0; i < statSlot.Length; i++)
            statSlot[i].UpdateStatValueUI();
    }
}
