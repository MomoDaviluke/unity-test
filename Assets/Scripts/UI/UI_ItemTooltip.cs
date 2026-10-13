using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI_ItemTooltip（物品提示框UI）- 显示装备物品的详细信息
/// 功能：显示物品名称、类型、描述，防止射线阻挡
/// </summary>
public class UI_ItemTooltip : UI_ToolTip
{
    // ==================== 文本组件 ====================
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private int defaultFontSize = 32;

    private CanvasGroup canvasGroup;

    // ==================== 初始化 ====================
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // 禁用射线阻挡，防止提示框遮挡物品槽导致闪烁
        canvasGroup.blocksRaycasts = false;
    }

    // ==================== 显示/隐藏 ====================
    public void ShowToolTip(ItemData_Equipment item)
    {
        if (item == null)
            return;

        itemNameText.text = item.itemName;
        itemTypeText.text = item.equipmentType.ToString();
        itemDescription.text = item.GetDescription();

        AdjustFontSize(itemNameText);
        AdjustPosition();

        gameObject.SetActive(true);
    }

    public void HideToolTip()
    {
        itemNameText.fontSize = defaultFontSize;
        gameObject.SetActive(false);
    }
}
