using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI_StatSlot（属性槽位UI）- 显示角色属性名称和数值
/// 功能：属性显示、鼠标悬停显示属性描述
/// </summary>
public class UI_StatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ==================== 核心引用 ====================
    private UI ui;
    [SerializeField] private string statName;
    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statValueText;
    [SerializeField] private TextMeshProUGUI statNameText;

    [TextArea]
    [SerializeField] private string statDescription;

    // ==================== 编辑器验证 ====================
    private void OnValidate()
    {
        gameObject.name = "Stat -" + statName;

        if (statNameText != null)
        {
            statNameText.text = statName;
        }
    }

    // ==================== 初始化 ====================
    void Start()
    {
        UpdateStatValueUI();
        ui = GetComponentInParent<UI>();
    }

    // ==================== 属性值更新 ====================
    // 根据属性类型计算并显示最终属性值（基础值+加成）
    public void UpdateStatValueUI()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            statValueText.text = playerStats.GetStat(statType).GetValue().ToString();

            // 各属性根据主属性计算最终值
            if (statType == StatType.maxHealth)
            {
                statValueText.text = playerStats.GetMaxHealthValue().ToString();
            }
            if (statType == StatType.damage)
            {
                statValueText.text = (playerStats.damage.GetValue() + playerStats.strength.GetValue()).ToString();
            }
            if (statType == StatType.critPower)
            {
                statValueText.text = (playerStats.critpower.GetValue() + playerStats.strength.GetValue()).ToString();
            }
            if (statType == StatType.critChance)
            {
                statValueText.text = (playerStats.critChance.GetValue() + playerStats.agility.GetValue()).ToString();
            }
            if (statType == StatType.evasion)
            {
                statValueText.text = (playerStats.evasion.GetValue() + playerStats.agility.GetValue()).ToString();
            }
            if (statType == StatType.magicResistance)
            {
                statValueText.text = (playerStats.magicResistance.GetValue() + playerStats.intelligence.GetValue() * 3).ToString();
            }
        }
    }

    // ==================== 鼠标交互 ====================
    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.statToolTip.ShowStatToolTip(statDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.HideStatToolTip();
    }
}
