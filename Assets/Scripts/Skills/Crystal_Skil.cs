using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Crystal_Skill : Skill   //放在技能控制器里面
{
    [SerializeField] private float crystalDuration;
    [SerializeField] private GameObject crystalPrefab;
    private GameObject currentCrystal;//预制件创建得临时水晶

    [Header("Crystal simple")]
    [SerializeField] private UI_SKillTreeSlot unlockCrystalButton;//解锁水晶按钮
    public bool crystalUnlocked { get; private set; }

    [Header("Crystal mirage")]
    [SerializeField] private UI_SKillTreeSlot unlockCloneInsteadButton;
    [SerializeField] private bool cloneInsteadOfCrystal;//克隆代替水晶


    [Header("Explosive crystal")]
    [SerializeField] private UI_SKillTreeSlot unlockExplosiveButton;//解锁爆炸水晶按钮
    [SerializeField] private bool canExplode;

    [Header("Moving crystal")]
    [SerializeField] private UI_SKillTreeSlot unlockMovingCrystalButton;//解锁移动水晶按钮
    [SerializeField] private bool canMoveToEnemy;
    [SerializeField] private float moveSpeed;


    [Header("Multi stacking crystal")]//水晶堆栈
    [SerializeField] private UI_SKillTreeSlot unlockMultiStackButton;//解锁多重水晶按钮
    [SerializeField] private bool canUseMultiStacks;
    [SerializeField] private int amountOfStacks;
    [SerializeField] private float multiStackCooldown;
    [SerializeField] private float useTimerWindow;
    [SerializeField] private List<GameObject> crystalLeft = new List<GameObject>(); //堆栈使用的列表


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


    public override void UseSkill()
    {
        base.UseSkill();


        if (CanUseMultiStack())
            return;


        if (currentCrystal == null)//如果没有水晶
        {
            CreateCrystal();
        }
        else
        {
            if (canMoveToEnemy)//有水晶的情况下，如果可以移动到敌人
                return;

            Vector2 playerPos = player.transform.position;//先保存玩家位置
            player.transform.position = currentCrystal.transform.position;//飞雷神之术
            currentCrystal.transform.position = playerPos;//位置互换


            if (cloneInsteadOfCrystal)//如果是克隆代替水晶
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

    public void CreateCrystal()
    {
        currentCrystal = Instantiate(crystalPrefab, player.transform.position, Quaternion.identity);//运行技能如果水晶没有那就创建一个

        Crystal_Skill_Controller currentCysalScrit = currentCrystal.GetComponent<Crystal_Skill_Controller>();//获得控制器中的组件

        currentCysalScrit.SetupCrystal(crystalDuration, canExplode, canMoveToEnemy, moveSpeed, FindClosestEnemy(currentCrystal.transform), player);//设置水晶函数

        //currentCysalScrit.ChooseRandomEnemy();

    }


    public void CurrentCrystalChooseRandomTarget() => currentCrystal.GetComponent<Crystal_Skill_Controller>().ChooseRandomEnemy();



    private bool CanUseMultiStack()//是否可以使用多个水晶
    {
        if (canUseMultiStacks)
        {
            if (crystalLeft.Count > 0 && cooldownTimer < 0)
            {
                if (crystalLeft.Count == amountOfStacks)
                {
                    Invoke("ResetAbility", useTimerWindow);
                }

                cooldown = 0;
                GameObject crystalToSpawn = crystalLeft[crystalLeft.Count - 1];//在列表中找到最后一个水晶
                GameObject newCrystal = Instantiate(crystalToSpawn, player.transform.position, Quaternion.identity);//创建一个新的水晶

                crystalLeft.Remove(crystalToSpawn);

                newCrystal.GetComponent<Crystal_Skill_Controller>().
                    SetupCrystal(crystalDuration, canExplode, canMoveToEnemy, moveSpeed, FindClosestEnemy(newCrystal.transform), player);//调用放置函数

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


    private void RefilCrystal()//重新填充水晶函数
    {
        int amountToAdd = amountOfStacks - crystalLeft.Count;

        for (int i = 0; i < amountToAdd; i++)//总是补充到3个
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