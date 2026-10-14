using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AfterImageFX（残影特效）- 创建玩家移动时的残影效果
/// 功能：复制玩家精灵并逐渐淡出
/// Phase 6 池化：淡出结束回池；OnEnable 恢复初始颜色（复用实例携带上次的淡出残留 alpha）
/// </summary>
public class AfterImageFX : MonoBehaviour
{
    // ==================== 核心组件 ====================
    private SpriteRenderer sr;
    private float colorLooseRate;

    // 初始颜色基准（Awake 捕获一次，OnEnable 恢复用）
    private Color initialColor;

    // ==================== 初始化 ====================
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;
    }

    // ==================== 复用重置 ====================
    private void OnEnable()
    {
        colorLooseRate = 0f;
        sr.color = initialColor;
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
            PoolManager.Instance.Despawn(gameObject);
    }
}
