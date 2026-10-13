using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI_CraftSlot（合成槽位UI）- 继承自物品槽位，显示可合成装备
/// 功能：设置合成装备信息、点击打开合成窗口
/// </summary>
public class UI_CraftSlot : UI_itemSlot
{
    // ==================== 初始化 ====================
    protected override void Start()
    {
        base.Start();
    }

    // ==================== 设置合成槽 ====================
    // 使用装备数据初始化槽位显示
    public void SetupCraftSlot(ItemData_Equipment _data)
    {
        if (_data == null)
            return;

        item = new InventoryItem(_data);
        itemImage.sprite = _data.itemIcon;
        itemText.text = _data.itemName;

        // 根据名称长度调整字体大小
        if (itemText.text.Length > 12)
            itemText.fontSize = itemText.fontSize * .7f;
        else
            itemText.fontSize = 24;
    }

    private void OnEnable()
    {
        if (item != null)
        {
            UpdateSlots(item);
        }
    }

    // ==================== 点击交互 ====================
    // 点击时打开对应装备的合成窗口
    public override void OnPointerDown(PointerEventData eventData)
    {
        ui.craftWindow.SetCraftWindow(item.data as ItemData_Equipment);
    }
}
