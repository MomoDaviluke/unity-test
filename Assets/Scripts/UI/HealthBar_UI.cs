using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HealthBar_UI（血条UI）- 显示实体血量，实体翻转时同步翻转血条
/// 功能：血量显示、事件订阅、翻转同步
/// </summary>
public class HealthBar_UI : MonoBehaviour
{
    // ==================== 核心引用 ====================
    private Entity entity;
    private CharacterStats myStats;
    private RectTransform myTransform;
    private Slider slider;

    // ==================== 初始化与事件订阅 ====================
    private void Start()
    {
        myTransform = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        slider = GetComponentInChildren<Slider>();
        myStats = GetComponentInParent<CharacterStats>();

        UpdateHealthUI();

        entity.onFlipped += FlipUI;
        myStats.onHealthChanged += UpdateHealthUI;
    }

    // ==================== 血量更新 ====================
    // 由事件触发，更新血条最大值和当前值
    private void UpdateHealthUI()
    {
        slider.maxValue = myStats.GetMaxHealthValue();
        slider.value = myStats.currentHealth;
    }

    // ==================== 翻转同步 ====================
    // 实体翻转时血条也翻转，保持血条方向正确
    private void FlipUI()
    {
        myTransform.Rotate(0, 180, 0);
    }

    // ==================== 事件取消订阅 ====================
    private void OnDisable()
    {
        entity.onFlipped -= FlipUI;
        myStats.onHealthChanged -= UpdateHealthUI;
    }
}