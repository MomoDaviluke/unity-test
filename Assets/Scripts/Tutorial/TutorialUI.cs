using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 教程UI显示组件，用于显示教程文本
public class TutorialUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI tutorialText; // 教程文本组件

    [Header("样式设置")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f); // 背景颜色
    [SerializeField] private Color textColor = Color.white; // 文本颜色
    [SerializeField] private int fontSize = 24; // 字体大小

    [Header("位置设置")]
    [SerializeField] private Vector2 screenPosition = new Vector2(0.5f, 0.3f); // 屏幕位置（0-1）
    [SerializeField] private Vector2 panelSize = new Vector2(200, 50); // 面板大小

    private RectTransform rectTransform; // RectTransform组件
    private CanvasGroup canvasGroup; // CanvasGroup组件

    private void Awake()
    {
        // 获取组件
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 如果没有CanvasGroup，添加一个
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 自动查找TextMeshProUGUI组件
        if (tutorialText == null)
        {
            tutorialText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // 如果没有找到，创建一个
        if (tutorialText == null)
        {
            CreateTutorialText();
        }

        // 设置UI样式
        SetupUIStyle();
    }

    // 创建教程文本组件
    private void CreateTutorialText()
    {
        // 创建文本对象
        GameObject textObj = new GameObject("TutorialText");
        textObj.transform.SetParent(transform, false);

        // 添加TextMeshProUGUI组件
        tutorialText = textObj.AddComponent<TextMeshProUGUI>();

        // 设置文本对齐
        tutorialText.alignment = TextAlignmentOptions.Center;
        tutorialText.fontSize = fontSize;
        tutorialText.color = textColor;

        // 设置文本位置
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    // 设置UI样式
    private void SetupUIStyle()
    {
        // 设置RectTransform
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = panelSize;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        // 设置文本样式
        if (tutorialText != null)
        {
            tutorialText.fontSize = fontSize;
            tutorialText.color = textColor;
            tutorialText.alignment = TextAlignmentOptions.Center;
        }

        // 设置初始透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    // 获取教程文本组件
    public TextMeshProUGUI GetTutorialText()
    {
        return tutorialText;
    }

    // 设置教程文本
    public void SetTutorialText(string text)
    {
        if (tutorialText != null)
        {
            tutorialText.text = text;
        }
    }

    // 设置面板大小
    public void SetPanelSize(Vector2 size)
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }
    }

    // 设置字体大小
    public void SetFontSize(int size)
    {
        if (tutorialText != null)
        {
            tutorialText.fontSize = size;
        }
    }

    // 设置文本颜色
    public void SetTextColor(Color color)
    {
        if (tutorialText != null)
        {
            tutorialText.color = color;
        }
    }

    // 设置背景颜色（需要背景图片）
    public void SetBackgroundColor(Color color)
    {
        UnityEngine.UI.Image backgroundImage = GetComponent<UnityEngine.UI.Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }

    // 在编辑器中可视化
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
