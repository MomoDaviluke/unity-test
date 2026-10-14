using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clone_Skill（分身技能）- 继承自Skill，创建玩家分身协助战斗
/// 功能：创建分身、分身攻击、侵略性效果、多重分身、水晶替换
/// 设计思路：分身在指定位置创建，自动攻击附近敌人，可配置多种效果
/// </summary>
public class Clone_Skill : Skill
{
    [Header("分身基础")]
    [SerializeField] private float attackMutiplier;     // 攻击倍率：分身造成的伤害比例
    [SerializeField] private GameObject clonePrefab;    // 分身预制体
    [SerializeField] private float cloneDuration;       // 分身持续时间

    [Header("分身攻击")]
    [SerializeField] private UI_SkillTreeSlot cloneAttackUnlockButton; // 分身攻击解锁按钮
    [SerializeField] private float cloneAttackMultiplier; // 分身攻击倍率
    [SerializeField] private bool canAttack;            // 是否可以攻击

    [Header("侵略性分身")]
    [SerializeField] private UI_SkillTreeSlot aggressiveCloneUnlockButton; // 侵略性解锁按钮
    [SerializeField] private float aggresiveCloneAttackMultiplier; // 侵略性攻击倍率
    public bool canApplyOnHitEffect { get; private set; } // 是否可应用击中效果

    [Header("多重分身")]
    [SerializeField] private UI_SkillTreeSlot multipleUnlockButton; // 多重分身解锁按钮
    [SerializeField] private float multiCloneAttackMultiplier; // 多重攻击倍率
    [SerializeField] private bool canDuplicateClone;    // 是否可以复制分身
    [SerializeField] private float chanceToDuplicate;   // 复制概率

    [Header("水晶替换分身")]
    [SerializeField] private UI_SkillTreeSlot crystalInsteadUnlockButton; // 水晶替换解锁按钮
    public bool crystalInsteadOfClone;                  // 是否使用水晶代替分身

    /// <summary>
    /// 启动：绑定技能树解锁按钮
    /// </summary>
    protected override void Start()
    {
        base.Start();

        cloneAttackUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockCloneAttack);
        aggressiveCloneUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockAggressiveClone);
        multipleUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockMultipleClone);
        crystalInsteadUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockCryStalInstead);
    }

    #region 解锁区域
    /// <summary>
    /// 检查所有解锁状态
    /// </summary>
    protected override void CheckUnlock()
    {
        UnlockCloneAttack();
        UnlockAggressiveClone();
        UnlockMultipleClone();
        UnlockCryStalInstead();
    }

    /// <summary>
    /// 解锁分身攻击
    /// </summary>
    private void UnlockCloneAttack()
    {
        if (cloneAttackUnlockButton.unlocked)
        {
            canAttack = true;
            attackMutiplier = cloneAttackMultiplier;
        }
    }

    /// <summary>
    /// 解锁侵略性分身（可触发装备特效）
    /// </summary>
    private void UnlockAggressiveClone()
    {
        if (aggressiveCloneUnlockButton.unlocked)
        {
            canApplyOnHitEffect = true;
            attackMutiplier = aggresiveCloneAttackMultiplier;
        }
    }

    /// <summary>
    /// 解锁多重分身（攻击时概率复制）
    /// </summary>
    private void UnlockMultipleClone()
    {
        if (multipleUnlockButton.unlocked)
        {
            canDuplicateClone = true;
            attackMutiplier = multiCloneAttackMultiplier;
        }
    }

    /// <summary>
    /// 解锁水晶替换（用水晶技能代替分身）
    /// </summary>
    private void UnlockCryStalInstead()
    {
        if (crystalInsteadUnlockButton.unlocked)
        {
            crystalInsteadOfClone = true;
        }
    }
    #endregion

    /// <summary>
    /// 创建分身：在指定位置生成分身
    /// </summary>
    /// <param name="_clonePosition">分身生成位置</param>
    /// <param name="_offset">位置偏移</param>
    public void CreateClone(Transform _clonePosition, Vector3 _offset)
    {
        // 如果解锁了水晶替换，使用水晶技能代替
        if (crystalInsteadOfClone)
        {
            SkillManager.instance.crystal.CreateCrystal();
            SkillManager.instance.crystal.CurrentCrystalChooseRandomTarget();
            return;
        }

        // 创建分身实例
        GameObject newClone = Instantiate(clonePrefab);

        // 设置分身参数
        newClone.GetComponent<Clone_Skill_Controller>().
            SetupClone(_clonePosition, cloneDuration, canAttack, _offset, FindClosestEnemy(newClone.transform), canDuplicateClone, chanceToDuplicate, player, attackMutiplier);
    }

    /// <summary>
    /// 延迟创建分身：用于反击成功后
    /// </summary>
    /// <param name="_enemyTransform">敌人位置</param>
    public void CreateCloneWithDelay(Transform _enemyTransform)
    {
        StartCoroutine(CloneDelayCorotine(_enemyTransform, new Vector3(2 * player.facingDir, -.1f, 0)));
    }

    /// <summary>
    /// 延迟创建分身协程
    /// </summary>
    private IEnumerator CloneDelayCorotine(Transform _transform, Vector3 _offset)
    {
        yield return new WaitForSeconds(.4f);
        CreateClone(_transform, _offset);
    }
}
