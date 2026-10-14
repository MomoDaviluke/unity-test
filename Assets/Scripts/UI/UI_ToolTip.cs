using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// UI_ToolTip（提示框基类）- 所有提示框的基类，处理位置和字体大小
/// 功能：根据鼠标位置调整提示框显示位置、自动调整字体大小
/// </summary>
public class UI_ToolTip : MonoBehaviour
{
    // ==================== 位置偏移参数 ====================
    [SerializeField] private float xOffset = 150;
    [SerializeField] private float yOffset = 150;

    // ==================== 位置调整 ====================
    // 根据鼠标位置在屏幕上的位置，决定提示框偏移方向
    // 翻转阈值运行时读取物理屏幕尺寸的一半：Input.mousePosition 是物理像素坐标，
    // 原先序列化的固定 960/540 只在 1920×1080 下正确，其他分辨率会误判半屏
    public virtual void AdjustPosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        float xLimit = Screen.width * .5f;
        float yLimit = Screen.height * .5f;

        float newxOffset = 0;
        float newyOffset = 0;

        // 鼠标靠近屏幕右侧时，提示框向左偏移；否则向右偏移
        if (mousePosition.x > xLimit)
            newxOffset = -xOffset;
        else
            newxOffset = xOffset;

        // 鼠标靠近屏幕顶部时，提示框向下偏移；否则向上偏移
        if (mousePosition.y > yLimit)
            newyOffset = -yOffset;
        else
            newyOffset = yOffset;

        transform.position = new Vector2(mousePosition.x + newxOffset, mousePosition.y + newyOffset);
    }

    // ==================== 字体调整 ====================
    // 文本过长时自动缩小字体
    public void AdjustFontSize(TextMeshProUGUI text)
    {
        if (text.text.Length > 12)
            text.fontSize = text.fontSize * .8f;
    }
}