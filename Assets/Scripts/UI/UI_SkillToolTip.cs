using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class UI_SkillToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI skillText;//显示技能描述的文本组件
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillCost;
    [SerializeField] private float defaultNameFontSize;

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