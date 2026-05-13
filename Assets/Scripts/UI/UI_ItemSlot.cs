using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_itemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image itemImage;
    [SerializeField] protected TextMeshProUGUI itemText;

    protected UI ui;
    public InventoryItem item;
    protected virtual void Start()
    {
        ui = GetComponentInParent<UI>();
    }

    public void UpdateSlots(InventoryItem _newItem)
    {
        item = _newItem;
        // 如果item为null或item.data为null，清空槽位
        if (item == null || item.data == null)
        {
            CleanUpSloat();
            return;
        }


        itemImage.color = Color.white;

        itemImage.sprite = item.data.itemIcon;

        if (item.stackSize > 1)
        {
            itemText.text = item.stackSize.ToString();

        }
        else
        {
            itemText.text = "";
        }
    }
    public void CleanUpSloat()
    {
        item = null;
        itemImage.sprite = null;
        itemImage.color = Color.clear;

        itemText.text = "";
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // 首先检查 item 是否为 null
        if (item.data == null)
            return;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Inventory.instance.RemoveItem(item.data);
            return;
        }

        if (item.data.itemType == ItemType.Equipment)
        {
            Inventory.instance.EquipItem(item.data);
        }
        ui.itemTooltip.HideToolTip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }

        ui.itemTooltip.ShowToolTip(item.data as ItemData_Equipment);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (item == null)
        {
            return;
        }
        ui.itemTooltip.HideToolTip();
    }
}