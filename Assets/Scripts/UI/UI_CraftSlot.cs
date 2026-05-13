using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CraftSlot : UI_itemSlot
{
    protected override void Start()
    {
        base.Start();
    }
    public void SetupCraftSlot(ItemData_Equipment _data)
    {
        if (_data == null)
            return;
        // 使用正确的构造函数创建InventoryItem
        item = new InventoryItem(_data);

        itemImage.sprite = _data.itemIcon;
        itemText.text = _data.itemName;

        if (itemText.text.Length > 12)
            itemText.fontSize = itemText.fontSize * .7f;
        else
            itemText.fontSize = 24;
    }
    private void OnEnable()
    {
        // 只有当item不为null时才更新槽位
        if (item != null)
        {
            UpdateSlots(item);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        ui.craftWindow.SetCraftWindow(item.data as ItemData_Equipment);
    }
}
