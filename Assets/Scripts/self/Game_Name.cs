using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 游戏标题动画效果控制器
/// </summary>
public class Game_Name : MonoBehaviour
{
    [Header("文本引用")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("缩放动画设置")]
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float scaleSpeed = 1f;

    [Header("颜色渐变设置")]
    [SerializeField] private Color firstColor = new Color(1f, 0.5f, 0f); // 橙色
    [SerializeField] private Color secondColor = new Color(0.8f, 0.2f, 0.2f); // 红色
    [SerializeField] private float colorSpeed = 1f;

    // 内部变量
    private Vector3 originalScale;
    private float scaleTimer = 0f;
    private float colorTimer = 0f;

    /// <summary>
    /// 初始化组件和设置
    /// </summary>
    private void Start()
    {
        // 如果没有指定文本组件，尝试获取当前对象上的组件
        if (titleText == null)
        {
            titleText = GetComponent<TextMeshProUGUI>();

            // 如果仍然没有找到，记录错误
            if (titleText == null)
            {
                Debug.LogError("Game_Name脚本需要一个TextMeshProUGUI组件！");
                enabled = false;
                return;
            }
        }

        // 保存原始缩放值
        originalScale = transform.localScale;

        // 初始化颜色
        titleText.color = firstColor;
    }

    /// <summary>
    /// 每帧更新动画效果
    /// </summary>
    private void Update()
    {
        // 更新计时器
        scaleTimer += Time.deltaTime * scaleSpeed;
        colorTimer += Time.deltaTime * colorSpeed;

        // 应用缩放动画
        UpdateScale();

        // 应用颜色渐变
        UpdateColor();
    }

    /// <summary>
    /// 更新文本缩放效果
    /// </summary>
    private void UpdateScale()
    {
        // 使用正弦函数创建平滑的缩放动画
        float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(scaleTimer) + 1f) * 0.5f);

        // 应用缩放
        transform.localScale = originalScale * scaleFactor;
    }

    /// <summary>
    /// 更新文本颜色渐变效果
    /// </summary>
    private void UpdateColor()
    {
        // 使用正弦函数在两种颜色之间平滑过渡
        float colorFactor = (Mathf.Sin(colorTimer) + 1f) * 0.5f;

        // 计算当前颜色
        Color currentColor = Color.Lerp(firstColor, secondColor, colorFactor);

        // 应用颜色
        titleText.color = currentColor;
    }

    /// <summary>
    /// 在Unity编辑器中重置组件时调用
    /// </summary>
    private void Reset()
    {
        // 尝试自动获取文本组件
        titleText = GetComponent<TextMeshProUGUI>();
    }
}
