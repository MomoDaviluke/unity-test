using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Image itemIcon;  // 重命名为更清晰的名称
    [SerializeField] private Button craftButton;

    [SerializeField] private Image[] materialImage;

    private void OnDisable()
    {
        // 当窗口关闭时移除所有监听器
        craftButton.onClick.RemoveAllListeners();
    }

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

        // 确保craftingMaterials不为null
        if (_data.craftRequirements != null)
        {
            // 显示材料图标
            for (int i = 0; i < _data.craftRequirements.Count; i++)
            {
                if (i >= materialImage.Length)
                {
                    Debug.LogWarning("材料数量超出了UI显示上限");
                    break;
                }

                if (_data.craftRequirements[i] != null && _data.craftRequirements[i].data != null)
                {
                    materialImage[i].sprite = _data.craftRequirements[i].data.itemIcon;  // 使用itemIcon而不是icon
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

        // 设置物品信息
        itemIcon.sprite = _data.itemIcon;  // 使用itemIcon而不是icon
        itemName.text = _data.itemName;
        itemDescription.text = _data.GetDescription();

        // 移除旧的监听器并添加新的
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => Inventory.instance.CanCraft(_data, _data.craftRequirements));
    }
}