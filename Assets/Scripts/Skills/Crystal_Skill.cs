using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Crystal_Skill（水晶技能）- 继承自Skill
/// 功能：创建水晶进行位置互换、爆炸、追踪敌人，支持多重堆栈
/// </summary>
public class Crystal_Skill : Skill
{
    // ==================== 水晶基础参数 ====================
    [SerializeField] private float crystalDuration;
    [SerializeField] private GameObject crystalPrefab;
    private GameObject currentCrystal; // 当前活跃的水晶实例

    // ==================== 水晶解锁 ====================
    [Header("Crystal simple")]
    [SerializeField] private UI_SkillTreeSlot unlockCrystalButton;
    public bool crystalUnlocked { get; private set; }

    // ==================== 水晶幻影 ====================
    [Header("Crystal mirage")]
    [SerializeField] private UI_SkillTreeSlot unlockCloneInsteadButton;
    [SerializeField] private bool cloneInsteadOfCrystal; // 克隆代替水晶

    // ==================== 爆炸水晶 ====================
    [Header("Explosive crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockExplosiveButton;
    [SerializeField] private bool canExplode;

    // ==================== 移动水晶 ====================
    [Header("Moving crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockMovingCrystalButton;
    [SerializeField] private bool canMoveToEnemy;
    [SerializeField] private float moveSpeed;

    // ==================== 多重堆栈水晶 ====================
    [Header("Multi stacking crystal")]
    [SerializeField] private UI_SkillTreeSlot unlockMultiStackButton;
    [SerializeField] private bool canUseMultiStacks;
    [SerializeField] private int amountOfStacks;
    [SerializeField] private float multiStackCooldown;
    [SerializeField] private float useTimerWindow;
    [SerializeField] private List<GameObject> crystalLeft = new List<GameObject>(); // 堆栈列表

    // ==================== 初始化与解锁 ====================
    protected override void Start()
    {
        base.Start();

        unlockCrystalButton.GetComponent<Button>().onClick.AddListener(UnlockCrystal);
        unlockCloneInsteadButton.GetComponent<Button>().onClick.AddListener(UnlockCrystalMirage);
        unlockExplosiveButton.GetComponent<Button>().onClick.AddListener(UnlockExplosiveCrystal);
        unlockMovingCrystalButton.GetComponent<Button>().onClick.AddListener(UnlockMovingCrystal);
        unlockMultiStackButton.GetComponent<Button>().onClick.AddListener(UnlockMultiStackCrystal);
    }

    #region Unlock skill region
    protected override void CheckUnlock()
    {
        UnlockCrystal();
        UnlockCrystalMirage();
        UnlockExplosiveCrystal();
        UnlockMovingCrystal();
        UnlockMultiStackCrystal();
    }

    private void UnlockCrystal()
    {
        if (unlockCrystalButton.unlocked)
            crystalUnlocked = true;
    }

    private void UnlockCrystalMirage()
    {
        if (unlockCloneInsteadButton.unlocked)
            cloneInsteadOfCrystal = true;
    }

    private void UnlockExplosiveCrystal()
    {
        if (unlockExplosiveButton.unlocked)
            canExplode = true;
    }

    private void UnlockMovingCrystal()
    {
        if (unlockMovingCrystalButton.unlocked)
            canMoveToEnemy = true;
    }

    private void UnlockMultiStackCrystal()
    {
        if (unlockMultiStackButton.unlocked)
            canUseMultiStacks = true;
    }
    #endregion

    public override bool CanUseSkill()
    {
        return base.CanUseSkill();
    }


    // ==================== 技能使用 ====================
    // 水晶技能逻辑：
    //   1. 优先使用多重堆栈
    //   2. 无水晶时创建新水晶
    //   3. 有水晶时与水晶位置互换（飞雷神之术）
    public override void UseSkill()
    {
        base.UseSkill();

        if (CanUseMultiStack())
            return;

        // 没有水晶时创建新水晶
        if (currentCrystal == null)
        {
            CreateCrystal();
        }
        else
        {
            // 移动水晶正在追踪敌人时不能互换
            if (canMoveToEnemy)
                return;

            // 保存玩家位置，与水晶位置互换
            Vector2 playerPos = player.transform.position;
            player.transform.position = currentCrystal.transform.position;
            currentCrystal.transform.position = playerPos;

            // 克隆代替水晶：创建分身后销毁水晶
            if (cloneInsteadOfCrystal)
            {
                SkillManager.instance.clone.CreateClone(currentCrystal.transform, new Vector3(0, .15f, 0));
                Destroy(currentCrystal);
            }
            else
            {
                currentCrystal.GetComponent<Crystal_Skill_Controller>().FinishCrystal();
            }
        }
    }

    // ==================== 水晶创建 ====================
    // 在玩家位置创建水晶并设置参数
    public void CreateCrystal()
    {
        currentCrystal = Instantiate(crystalPrefab, player.transform.position, Quaternion.identity);
        Crystal_Skill_Controller currentCrystalScript = currentCrystal.GetComponent<Crystal_Skill_Controller>();
        currentCrystalScript.SetupCrystal(crystalDuration, canExplode, canMoveToEnemy, moveSpeed, FindClosestEnemy(currentCrystal.transform), player);
    }

    // 为当前水晶随机选择目标
    public void CurrentCrystalChooseRandomTarget() => currentCrystal.GetComponent<Crystal_Skill_Controller>().ChooseRandomEnemy();

    // ==================== 多重堆栈系统 ====================
    // 使用堆栈中的水晶，用完进入冷却
    private bool CanUseMultiStack()
    {
        if (canUseMultiStacks)
        {
            if (crystalLeft.Count > 0 && cooldownTimer < 0)
            {
                // 首次使用堆栈时启动计时窗口
                if (crystalLeft.Count == amountOfStacks)
                {
                    Invoke("ResetAbility", useTimerWindow);
                }

                cooldown = 0;
                // 取出最后一个水晶并创建
                GameObject crystalToSpawn = crystalLeft[crystalLeft.Count - 1];
                GameObject newCrystal = Instantiate(crystalToSpawn, player.transform.position, Quaternion.identity);
                crystalLeft.Remove(crystalToSpawn);

                newCrystal.GetComponent<Crystal_Skill_Controller>().
                    SetupCrystal(crystalDuration, canExplode, canMoveToEnemy, moveSpeed, FindClosestEnemy(newCrystal.transform), player);

                // 堆栈用完，进入冷却并补充
                if (crystalLeft.Count <= 0)
                {
                    cooldown = multiStackCooldown;
                    RefilCrystal();
                }
                return true;
            }
        }
        return false;
    }

    // 重新填充水晶堆栈
    private void RefilCrystal()
    {
        int amountToAdd = amountOfStacks - crystalLeft.Count;
        for (int i = 0; i < amountToAdd; i++)
        {
            crystalLeft.Add(crystalPrefab);
        }
    }

    private void ResetAbility()
    {
        if (cooldownTimer > 0)
            return;

        cooldownTimer = multiStackCooldown;
        RefilCrystal();
    }
}