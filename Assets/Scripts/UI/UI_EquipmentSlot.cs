using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI_EquipmentSlot（装备槽位UI）- 继承自物品槽位，专门用于装备
/// 功能：显示装备、点击卸下装备
/// </summary>
public class UI_EquipmentSlot : UI_ItemSlot
{
    // ==================== 装备类型 ====================
    public EquipmentType slotType;

    private void OnValidate()
    {
    }

    // ==================== 点击交互 ====================
    // 点击装备槽时卸下装备并放回背包
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item != null)
        {
            Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
            Inventory.instance.AddItem(item.data as ItemData_Equipment);

            ui.itemTooltip.HideToolTip();
            CleanUpSlot();
        }
    }
}
