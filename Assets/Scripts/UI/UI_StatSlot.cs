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

    // Phase 3：属性变化订阅目标（装备/Buff/技能修改任意 Stat 时自动刷新本槽）
    private PlayerStats subscribedStats;

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

    // ==================== 事件订阅 ====================
    // 角色面板打开时激活并订阅（面板关闭时不订阅、不刷新；重新打开由 Start 拉初值，行为等价）
    private void OnEnable()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return;

        subscribedStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (subscribedStats != null)
            subscribedStats.onStatsChanged += UpdateStatValueUI;
    }

    private void OnDisable()
    {
        if (subscribedStats != null)
        {
            subscribedStats.onStatsChanged -= UpdateStatValueUI;
            subscribedStats = null;
        }
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
