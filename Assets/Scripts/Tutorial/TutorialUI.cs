using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// TutorialUI（教程UI）- 显示教程文本的UI组件
/// 功能：创建文本、设置样式、控制显示
/// </summary>
public class TutorialUI : MonoBehaviour
{
    // ==================== UI组件 ====================
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    // ==================== 样式设置 ====================
    [Header("样式设置")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;

    // ==================== 位置设置 ====================
    [Header("位置设置")]
    [SerializeField] private Vector2 screenPosition = new Vector2(0.5f, 0.3f);
    [SerializeField] private Vector2 panelSize = new Vector2(200, 50);

    // ==================== 内部组件 ====================
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // ==================== 初始化 ====================
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (tutorialText == null)
            tutorialText = GetComponentInChildren<TextMeshProUGUI>();

        if (tutorialText == null)
            CreateTutorialText();

        SetupUIStyle();
    }

    // ==================== 创建文本组件 ====================
    private void CreateTutorialText()
    {
        GameObject textObj = new GameObject("TutorialText");
        textObj.transform.SetParent(transform, false);

        tutorialText = textObj.AddComponent<TextMeshProUGUI>();
        tutorialText.alignment = TextAlignmentOptions.Center;
        tutorialText.fontSize = fontSize;
        tutorialText.color = textColor;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    // ==================== 设置UI样式 ====================
    private void SetupUIStyle()
    {
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = panelSize;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        if (tutorialText != null)
        {
            tutorialText.fontSize = fontSize;
            tutorialText.color = textColor;
            tutorialText.alignment = TextAlignmentOptions.Center;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // ==================== 公开接口 ====================
    public TextMeshProUGUI GetTutorialText() => tutorialText;

    public void SetTutorialText(string text)
    {
        if (tutorialText != null)
            tutorialText.text = text;
    }

    public void SetPanelSize(Vector2 size)
    {
        if (rectTransform != null)
            rectTransform.sizeDelta = size;
    }

    public void SetFontSize(int size)
    {
        if (tutorialText != null)
            tutorialText.fontSize = size;
    }

    public void SetTextColor(Color color)
    {
        if (tutorialText != null)
            tutorialText.color = color;
    }

    public void SetBackgroundColor(Color color)
    {
        UnityEngine.UI.Image backgroundImage = GetComponent<UnityEngine.UI.Image>();
        if (backgroundImage != null)
            backgroundImage.color = color;
    }

    // ==================== 编辑器可视化 ====================
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
