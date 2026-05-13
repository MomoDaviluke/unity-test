using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Clone_Skill : Skill
{
    [Header("分身")]
    [SerializeField] private float attackMutiplier;//攻击乘数
    [SerializeField] private GameObject clonePrefab;//克隆原型
    [SerializeField] private float cloneDuration;//克隆持续时间
    [Space]

    [Header("分身攻击")]
    [SerializeField] private UI_SKillTreeSlot cloneAttackUnlockButton;
    [SerializeField] private float cloneAttackMultiplier;
    [SerializeField] private bool canAttack;// 判断是否可以攻击

    [Header("侵略性分身")]
    [SerializeField] private UI_SKillTreeSlot aggressiveCloneUnlockButton;
    [SerializeField] private float aggresiveCloneAttackMultiplier;
    public bool canApplyOnHitEffect { get; private set; }//侵略性分身


    [Header("多重分身")]
    [SerializeField] private UI_SKillTreeSlot multipleUnlockButton;//多重分身
    [SerializeField] private float multiCloneAttackMultiplier;
    [SerializeField] private bool canDuplicateClone;//可以重复clone
    [SerializeField] private float chanceToDuplicate;//重复clone的几率

    [Header("水晶替换分身")]
    [SerializeField] private UI_SKillTreeSlot crystalInsteadUnlockButton;//是否使用水晶代替克隆
    public bool crystalInsteadOfClone;//是否使用水晶代替克隆



    protected override void Start()
    {
        base.Start();


        cloneAttackUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockCloneAttack);
        aggressiveCloneUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockAggressiveClone);
        multipleUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockMultipleClone);
        crystalInsteadUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockCryStalInstead);
    }


    #region Unlock region
    protected override void CheckUnlock()
    {
        UnlockCloneAttack();
        UnlockAggressiveClone();
        UnlockMultipleClone();
        UnlockCryStalInstead();
    }


    private void UnlockCloneAttack()
    {
        if (cloneAttackUnlockButton.unlocked)
        {
            canAttack = true;
            attackMutiplier = cloneAttackMultiplier;
        }
    }

    private void UnlockAggressiveClone()
    {
        if (aggressiveCloneUnlockButton.unlocked)
        {
            canApplyOnHitEffect = true;
            attackMutiplier = aggresiveCloneAttackMultiplier;
        }
    }

    private void UnlockMultipleClone()
    {
        if (multipleUnlockButton.unlocked)
        {
            canDuplicateClone = true;
            attackMutiplier = multiCloneAttackMultiplier;
        }
    }

    private void UnlockCryStalInstead()
    {
        if (crystalInsteadUnlockButton.unlocked)
        {
            crystalInsteadOfClone = true;
        }
    }

    #endregion



    public void CreateClone(Transform _clonePosition, Vector3 _offset)//传入克隆位置
    {

        if (crystalInsteadOfClone)//克隆转换成水晶
        {
            SkillManager.instance.crystal.CreateCrystal();
            SkillManager.instance.crystal.CurrentCrystalChooseRandomTarget();
            return;
        }


        GameObject newClone = Instantiate(clonePrefab);//创建新的克隆//克隆 original 对象并返回克隆对象。


        newClone.GetComponent<Clone_Skill_Controller>().
            SetupClone(_clonePosition, cloneDuration, canAttack, _offset, FindClosestEnemy(newClone.transform), canDuplicateClone, chanceToDuplicate, player, attackMutiplier);

    }



    public void CreateCloneWithDelay(Transform _enemyTransform)//反击成功成克隆
    {
        StartCoroutine(CloneDelayCorotine(_enemyTransform, new Vector3(2 * player.facingDir, -.1f, 0)));
    }

    private IEnumerator CloneDelayCorotine(Transform _transform, Vector3 _offset)//产生克隆之后的延迟
    {
        yield return new WaitForSeconds(.4f);
        CreateClone(_transform, _offset);
    }

}
