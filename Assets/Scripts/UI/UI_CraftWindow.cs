using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_CraftWindow（合成窗口UI）- 显示装备合成详情和材料需求
/// 功能：显示装备信息、材料需求、合成按钮
/// </summary>
public class UI_CraftWindow : MonoBehaviour
{
    // ==================== 装备信息 ====================
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button craftButton;

    // ==================== 材料显示 ====================
    [SerializeField] private Image[] materialImage;

    // ==================== 窗口关闭 ====================
    private void OnDisable()
    {
        craftButton.onClick.RemoveAllListeners();
    }

    // ==================== 设置合成窗口 ====================
    // 根据装备数据更新窗口显示，包括图标、材料、描述
    public void SetCraftWindow(ItemData_Equipment _data)
    {
        if (_data == null)
            return;

        // 清除所有材料图标
        for (int i = 0; i < materialImage.Length; i++)
        {
            materialImage[i].color = Color.clear;
            TextMeshProUGUI materialText = materialImage[i].GetComponentInChildren<TextMeshProUGUI>();
            if (materialText != null)
                materialText.color = Color.clear;
        }

        // 显示所需材料
        if (_data.craftRequirements != null)
        {
            for (int i = 0; i < _data.craftRequirements.Count; i++)
            {
                if (i >= materialImage.Length)
                {
                    Debug.LogWarning("材料数量超出了UI显示上限");
                    break;
                }

                if (_data.craftRequirements[i] != null && _data.craftRequirements[i].data != null)
                {
                    materialImage[i].sprite = _data.craftRequirements[i].data.itemIcon;
                    materialImage[i].color = Color.white;

                    TextMeshProUGUI materialSlotText = materialImage[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (materialSlotText != null)
                    {
                        materialSlotText.text = _data.craftRequirements[i].stackSize.ToString();
                        materialSlotText.color = Color.white;
                    }
                }
            }
        }

        // 设置装备信息
        itemIcon.sprite = _data.itemIcon;
        itemName.text = _data.itemName;
        itemDescription.text = _data.GetDescription();

        // 绑定合成按钮事件
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => Inventory.instance.CanCraft(_data, _data.craftRequirements));
    }
}