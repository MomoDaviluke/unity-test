using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// UI_SkillToolTip（技能提示框UI）- 显示技能名称、描述和价格
/// 功能：显示技能信息、自动调整位置
/// </summary>
public class UI_SkillToolTip : UI_ToolTip
{
    // ==================== 文本组件 ====================
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillCost;
    [SerializeField] private float defaultNameFontSize;

    // ==================== 显示/隐藏 ====================
    public void ShowToolTip(string _skillDescprtion, string _skillName, int _price)
    {
        skillName.text = _skillName;
        skillText.text = _skillDescprtion;
        skillCost.text = "Cost: " + _price;

        AdjustPosition();
        AdjustFontSize(skillName);

        gameObject.SetActive(true);
    }

    public virtual void HideToolTip()
    {
        skillName.fontSize = defaultNameFontSize;
        gameObject.SetActive(false);
    }
}