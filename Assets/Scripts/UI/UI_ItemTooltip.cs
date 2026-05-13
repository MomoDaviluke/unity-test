using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemTooltip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private int defaultFontSize = 32;

    private CanvasGroup canvasGroup;  // 用于控制射线阻挡

    protected virtual void Awake()
    {
        // 获取或添加 CanvasGroup 组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // 禁用射线阻挡，防止提示框遮挡物品槽导致闪烁
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowToolTip(ItemData_Equipment item)
    {
        if (item == null)
        {
            return;
        }
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
