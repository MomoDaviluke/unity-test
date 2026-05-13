using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI description;

    private CanvasGroup canvasGroup;  // 用于控制射线阻挡

    protected virtual void Awake()
    {
        // 获取或添加 CanvasGroup 组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        // 禁用射线阻挡，防止提示框遮挡导致闪烁
        canvasGroup.blocksRaycasts = false;
    }

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
