using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Parry_Skill（反击技能）- 继承自Skill
/// 功能：反击姿态，成功反击时恢复生命并创建幻影分身
/// </summary>
public class Parry_Skill : Skill
{
    // ==================== 反击解锁 ====================
    [Header("反击")]
    [SerializeField] private UI_SkillTreeSlot parryUnlockButton;
    public bool parryUnlocked { get; private set; }

    // ==================== 反击恢复 ====================
    [Header("反击恢复")]
    [SerializeField] private UI_SkillTreeSlot restorUnlockButton;
    [Range(0f, 1f)]
    [SerializeField] private float restoreHealthPercentage;
    public bool restorUnlocked { get; private set; }

    // ==================== 格挡幻影 ====================
    [Header("格挡幻影")]
    [SerializeField] private UI_SkillTreeSlot parrtWithMirageUnlockButton;
    public bool parrtWithMirageUnlocked { get; private set; }

    // ==================== 技能使用 ====================
    // 成功反击时恢复生命值
    public override void UseSkill()
    {
        base.UseSkill();

        if (restorUnlocked)
        {
            int restoreAmount = Mathf.RoundToInt(player.stats.GetMaxHealthValue() * restoreHealthPercentage);
            player.stats.IncreaseHealthBy(restoreAmount);
        }
    }

    // ==================== 初始化与解锁 ====================
    protected override void Start()
    {
        base.Start();

        parryUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParry);
        restorUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParryRestor);
        parrtWithMirageUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParrtWithMirage);
    }

    protected override void CheckUnlock()
    {
        UnlockParry();
        UnlockParryRestor();
        UnlockParrtWithMirage();
    }

    private void UnlockParry()
    {
        if (parryUnlockButton.unlocked)
            parryUnlocked = true;
    }

    private void UnlockParryRestor()
    {
        if (restorUnlockButton.unlocked)
            restorUnlocked = true;
    }

    private void UnlockParrtWithMirage()
    {
        if (parrtWithMirageUnlockButton.unlocked)
            parrtWithMirageUnlocked = true;
    }

    // ==================== 幻影创建接口 ====================
    // 成功反击时在敌人位置创建幻影分身
    public void MakeMirageOnParry(Transform _respawnTransform)
    {
        if (parrtWithMirageUnlocked)
            SkillManager.instance.clone.CreateCloneWithDelay(_respawnTransform);
    }
}