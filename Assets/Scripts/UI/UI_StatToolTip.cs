using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI_StatToolTip（属性提示框UI）- 显示属性描述信息
/// 功能：显示属性描述、防止射线阻挡
/// </summary>
public class UI_StatToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI description;

    private CanvasGroup canvasGroup;

    // ==================== 初始化 ====================
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // 禁用射线阻挡，防止提示框遮挡导致闪烁
        canvasGroup.blocksRaycasts = false;
    }

    // ==================== 显示/隐藏 ====================
    public void ShowStatToolTip(string _text)
    {
        description.text = _text;
        AdjustPosition();

        gameObject.SetActive(true);
    }

    public void HideStatToolTip()
    {
        description.text = "";
        gameObject.SetActive(false);
    }
}
