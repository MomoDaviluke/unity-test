using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Game_Name（游戏标题动画）- 控制标题的缩放和颜色渐变动画
/// 功能：正弦波缩放、颜色循环渐变
/// </summary>
public class Game_Name : MonoBehaviour
{
    // ==================== 文本引用 ====================
    [Header("文本引用")]
    [SerializeField] private TextMeshProUGUI titleText;

    // ==================== 缩放动画 ====================
    [Header("缩放动画设置")]
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float scaleSpeed = 1f;

    // ==================== 颜色渐变 ====================
    [Header("颜色渐变设置")]
    [SerializeField] private Color firstColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color secondColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private float colorSpeed = 1f;

    // ==================== 内部状态 ====================
    private Vector3 originalScale;
    private float scaleTimer = 0f;
    private float colorTimer = 0f;

    // ==================== 初始化 ====================
    private void Start()
    {
        if (titleText == null)
        {
            titleText = GetComponent<TextMeshProUGUI>();

            if (titleText == null)
            {
                Debug.LogError("Game_Name脚本需要一个TextMeshProUGUI组件！");
                enabled = false;
                return;
            }
        }

        originalScale = transform.localScale;
        titleText.color = firstColor;
    }

    // ==================== 动画更新 ====================
    private void Update()
    {
        scaleTimer += Time.deltaTime * scaleSpeed;
        colorTimer += Time.deltaTime * colorSpeed;

        UpdateScale();
        UpdateColor();
    }

    // ==================== 缩放动画 ====================
    private void UpdateScale()
    {
        float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(scaleTimer) + 1f) * 0.5f);
        transform.localScale = originalScale * scaleFactor;
    }

    // ==================== 颜色渐变 ====================
    private void UpdateColor()
    {
        float colorFactor = (Mathf.Sin(colorTimer) + 1f) * 0.5f;
        Color currentColor = Color.Lerp(firstColor, secondColor, colorFactor);
        titleText.color = currentColor;
    }

    // ==================== 编辑器重置 ====================
    private void Reset()
    {
        titleText = GetComponent<TextMeshProUGUI>();
    }
}
