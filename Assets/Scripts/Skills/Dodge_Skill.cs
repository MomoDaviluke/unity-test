using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Dodge_Skill（闪避技能）- 继承自Skill
/// 功能：解锁闪避能力，提升闪避率，闪避时可创建幻影分身
/// </summary>
public class Dodge_Skill : Skill
{
    // ==================== 闪避解锁 ====================
    [Header("闪避")]
    [SerializeField] private UI_SKillTreeSlot unlockDodgeButton;
    [SerializeField] private int evasionAmount;
    public bool dodgeUnlocked;

    // ==================== 闪避幻影 ====================
    [Header("躲避幻影")]
    [SerializeField] private UI_SKillTreeSlot unlockMirageDodge;
    public bool dodgeMirageUnlocked;

    // ==================== 初始化与解锁 ====================
    protected override void Start()
    {
        base.Start();

        unlockDodgeButton.GetComponent<Button>().onClick.AddListener(UnlockDodge);
        unlockMirageDodge.GetComponent<Button>().onClick.AddListener(UnlockMirageDodge);
    }

    protected override void CheckUnlock()
    {
        UnlockDodge();
        UnlockMirageDodge();
    }

    // 解锁闪避：增加闪避率属性
    private void UnlockDodge()
    {
        if (unlockDodgeButton != null && unlockDodgeButton.unlocked && !dodgeUnlocked)
        {
            if (player != null && player.stats != null)
            {
                // Phase 3：Stat 变化经 onStatsChanged 自动通知属性面板，无需显式广播
                player.stats.evasion.AddModifier(evasionAmount);
            }
            dodgeUnlocked = true;
        }
    }

    private void UnlockMirageDodge()
    {
        if (unlockMirageDodge.unlocked)
            dodgeMirageUnlocked = true;
    }

    // ==================== 幻影创建接口 ====================
    // 闪避时创建幻影分身
    public void CreateMirageOnDodge()
    {
        if (dodgeMirageUnlocked)
            SkillManager.instance.clone.CreateClone(player.transform, new Vector3(2 * player.facingDir, 0));
    }
}