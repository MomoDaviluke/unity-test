using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 这个类管理整个制作列表UI，包含多个制作槽位
public class UI_CraftList : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Transform craftSlotParent;     // 所有制作槽位的父物体
    [SerializeField] private GameObject craftSlotPrefab;    // 制作槽位的预制体

    [SerializeField] private List<ItemData_Equipment> craftEquipment;  // 可制作的装备列表
    [SerializeField] private List<UI_CraftSlot> craftSlots;            // UI中的制作槽位列表

    void Start()
    {
        transform.parent.GetChild(0).GetComponent<UI_CraftList>().SetupCraftList();
        SetupDefaultCraftWindow();
    }



    // 重新设置制作列表，创建新的槽位
    public void SetupCraftList()
    {
        // 清除所有现有槽位
        for (int i = 0; i < craftSlotParent.childCount; i++)
        {
            Destroy(craftSlotParent.GetChild(i).gameObject);
        }


        // 为每个可制作装备创建一个新槽位
        for (int i = 0; i < craftEquipment.Count; i++)
        {
            GameObject newSlot = Instantiate(craftSlotPrefab, craftSlotParent);
            newSlot.GetComponent<UI_CraftSlot>().SetupCraftSlot(craftEquipment[i]);
        }
    }

    // 当点击制作列表时刷新显示
    public void OnPointerDown(PointerEventData eventData)
    {
        SetupCraftList();
    }
    public void SetupDefaultCraftWindow()
    {
        if (craftEquipment[0] != null)
            GetComponentInParent<UI>().craftWindow.SetCraftWindow(craftEquipment[0]);
    }
}