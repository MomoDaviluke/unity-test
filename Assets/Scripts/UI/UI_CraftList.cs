using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI_CraftList（合成列表UI）- 管理可合成装备的列表显示
/// 功能：创建合成槽位、刷新列表、设置默认合成窗口
/// </summary>
public class UI_CraftList : MonoBehaviour, IPointerDownHandler
{
    // ==================== 核心引用 ====================
    [SerializeField] private Transform craftSlotParent;
    [SerializeField] private GameObject craftSlotPrefab;

    // ==================== 合成数据 ====================
    [SerializeField] private List<ItemData_Equipment> craftEquipment;
    [SerializeField] private List<UI_CraftSlot> craftSlots;

    // ==================== 初始化 ====================
    void Start()
    {
        transform.parent.GetChild(0).GetComponent<UI_CraftList>().SetupCraftList();
        SetupDefaultCraftWindow();
    }

    // ==================== 列表管理 ====================
    // 清除现有槽位并为每个可合成装备创建新槽位
    public void SetupCraftList()
    {
        for (int i = 0; i < craftSlotParent.childCount; i++)
        {
            Destroy(craftSlotParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < craftEquipment.Count; i++)
        {
            GameObject newSlot = Instantiate(craftSlotPrefab, craftSlotParent);
            newSlot.GetComponent<UI_CraftSlot>().SetupCraftSlot(craftEquipment[i]);
        }
    }

    // 点击时刷新列表
    public void OnPointerDown(PointerEventData eventData)
    {
        SetupCraftList();
    }

    // 设置默认合成窗口显示第一个装备
    public void SetupDefaultCraftWindow()
    {
        if (craftEquipment[0] != null)
            GetComponentInParent<UI>().craftWindow.SetCraftWindow(craftEquipment[0]);
    }
}