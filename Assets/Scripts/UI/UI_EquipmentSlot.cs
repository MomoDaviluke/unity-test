using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EquipmentSlot : UI_itemSlot
{
    public EquipmentType slotType;

    private void OnValidate()
    {

    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (item != null) // 确保item不为空
        {
            Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
            Inventory.instance.AddItem(item.data as ItemData_Equipment);

            ui.itemTooltip.HideToolTip();

            CleanUpSloat();
        }
    }
}
