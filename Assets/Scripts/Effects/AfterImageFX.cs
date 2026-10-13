using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AfterImageFX（残影特效）- 创建玩家移动时的残影效果
/// 功能：复制玩家精灵并逐渐淡出
/// </summary>
public class AfterImageFX : MonoBehaviour
{
    // ==================== 核心组件 ====================
    private SpriteRenderer sr;
    private float colorLooseRate;

    // ==================== 初始化 ====================
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ==================== 残影设置 ====================
    public void SetupAfterImage(float _loosigSpeed, Sprite _spriteImage)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
        sr.sprite = _spriteImage;
        colorLooseRate = _loosigSpeed;
    }

    // ==================== 淡出更新 ====================
    private void Update()
    {
        if (sr == null)
            return;

        float alpha = sr.color.a - colorLooseRate * Time.deltaTime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        if (sr.color.a <= 0)
            Destroy(gameObject);
    }
}
