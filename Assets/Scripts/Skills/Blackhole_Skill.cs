using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Blackhole_Skill（黑洞技能）- 继承自Skill
/// 功能：在玩家位置创建黑洞，吸引并攻击敌人
/// </summary>
public class Blackhole_Skill : Skill
{
    // ==================== 解锁与配置 ====================
    [SerializeField] private UI_SKillTreeSlot blackHoleUnlockButton;
    public bool blackHoleUnlocked { get; private set; }
    [SerializeField] private int amountOfAttacks;
    [SerializeField] private float cloneCooldown;
    [SerializeField] private float blackholeDuration;

    // ==================== 黑洞预制体参数 ====================
    [Space]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private float maxSize;
    [SerializeField] private float growSpeed;
    [SerializeField] private float shrinkSpeed;

    // ==================== 当前黑洞引用 ====================
    Blackhole_skill_controller currentBlackhole;

    // ==================== 解锁检测 ====================
    private void UnlockBlackhole()
    {
        if (blackHoleUnlockButton.unlocked)
            blackHoleUnlocked = true;
    }

    protected override void CheckUnlock()
    {
        base.CheckUnlock();
        UnlockBlackhole();
    }

    protected override void Start()
    {
        base.Start();
        blackHoleUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockBlackhole);
    }

    // ==================== 技能使用 ====================
    public override bool CanUseSkill()
    {
        return base.CanUseSkill();
    }

    public override void UseSkill()
    {
        base.UseSkill();

        // 在玩家位置创建黑洞
        GameObject newBlackHole = Instantiate(blackHolePrefab, player.transform.position, Quaternion.identity);
        currentBlackhole = newBlackHole.GetComponent<Blackhole_skill_controller>();
        currentBlackhole.SetupBlackhole(maxSize, growSpeed, shrinkSpeed, amountOfAttacks, cloneCooldown, blackholeDuration);

        AudioManager.instance.PlaySFX(3, transform);
    }

    protected override void Update()
    {
        base.Update();
    }

    // ==================== 状态查询接口 ====================
    // 检查黑洞技能是否已完成
    public bool SkillCompleted()
    {
        if (currentBlackhole == null)
            return false;
        if (currentBlackhole.playerCanExitState)
        {
            currentBlackhole = null;
            return true;
        }
        return false;
    }

    // 强制结束黑洞技能（用于状态异常恢复）
    public void ForceCompleteSkill()
    {
        if (currentBlackhole != null)
        {
            Destroy(currentBlackhole.gameObject);
            currentBlackhole = null;
        }
    }

    // 获取黑洞半径
    public float GetBlackholeRadius()
    {
        return maxSize / 2;
    }
}
