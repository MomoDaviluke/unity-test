using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//P67仍刀技能

public enum SwordType//刀的类型,枚举（enum）是一种特殊的数据类型，它允许你为一组相关的常量赋予有意义的名称。这样可以提高代码的可读性和可维护性。
{
    Regular,//常规
    Bounce,//反弹
    Pierce,//穿透
    Spin//旋转
}



public class Sword_Skill : Skill
{
    public SwordType swordType = SwordType.Regular;//刀的类型

    [Header("Bounce info")]
    [SerializeField] private UI_SKillTreeSlot bounceUnlockButton;//解锁按钮
    [SerializeField] private int bounceAmount;//反弹次数
    [SerializeField] private float bounceGravity;//反弹速度
    [SerializeField] private float bounceSpeed;

    [Header("Peirce info")]
    [SerializeField] private UI_SKillTreeSlot pierceUnlockButton;//解锁按钮
    [SerializeField] private int pierceAmount;//穿透次数
    [SerializeField] private float pierceGravity;//穿透速度

    [Header("Spin info")]
    [SerializeField] private UI_SKillTreeSlot spinUnlockButton;//解锁按钮
    [SerializeField] private float hitCooldown = .35f;//击中冷却
    [SerializeField] private float maxTravelDistance = 7;//最大旋转距离
    [SerializeField] private float spinDuration = 2;//旋转持续时间
    [SerializeField] private float spinGravity = 1;//旋转重力

    [Header("Skill info")]
    [SerializeField] private UI_SKillTreeSlot swordUnlockButton;//解锁按钮
    public bool swordUnlocked { get; private set; }//解锁
    [SerializeField] private GameObject swordPrefab;//飞行物体
    [SerializeField] private Vector2 launchForce;//飞行方向
    [SerializeField] private float swordGravity;//飞行重力
    [SerializeField] private float freezeTimeDuration;//冻结时间
    [SerializeField] private float returnSpeed;


    [Header("Passive skills")]//被动技能
    [SerializeField] private UI_SKillTreeSlot timeStopUnlockButton;//时间停止解锁按钮
    public bool timeStopUnlocked { get; private set; }//时间停止解锁
    [SerializeField] private UI_SKillTreeSlot volnurableUnlockButton;//易伤解锁按钮
    public bool volnurableUnlocked { get; private set; }//易伤解锁


    private Vector2 finalDir;

    [Header("Anim dots")]
    [SerializeField] private int numberOfDots;
    [SerializeField] private float spaceBetweenDots;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotsParent;

    private GameObject[] dots;

    protected override void Start()
    {
        base.Start();

        GenerateDots();

        SetupGravity();

        swordUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSword);
        bounceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockBounceSword);
        pierceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockPierceSword);
        spinUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSpinSword);
        timeStopUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockTimeStop);
        volnurableUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockVolnurable);
    }

    public void SetupGravity()
    {
        if (swordType == SwordType.Bounce)
            swordGravity = bounceGravity;
        else if (swordType == SwordType.Pierce)
            swordGravity = pierceGravity;
        else if (swordType == SwordType.Spin)
            swordGravity = spinGravity;

    }




    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse1))
            finalDir = new Vector2(AimDirection().normalized.x * launchForce.x, AimDirection().normalized.y * launchForce.y);

        if (Input.GetKey(KeyCode.Mouse1))
        {
            for (int i = 0; i < dots.Length; i++)
            {

                dots[i].transform.position = DotsPosition(i * spaceBetweenDots);
            }
        }
    }


    public void CreateSword()
    {
        GameObject newSword = Instantiate(swordPrefab, player.transform.position, transform.rotation);//获取物体，位置，旋转,它可以根据现有的预制件（Prefab）、场景中的对象或其他资源来生成新的实例
        Sword_Skill_Controller newSwordScript = newSword.GetComponent<Sword_Skill_Controller>();


        if (swordType == SwordType.Bounce)
            newSwordScript.SetupBounce(true, bounceAmount, bounceSpeed);
        else if (swordType == SwordType.Pierce)
            newSwordScript.SetupPierce(pierceAmount);
        else if (swordType == SwordType.Spin)
            newSwordScript.SetupSpin(true, maxTravelDistance, spinDuration, hitCooldown);





        newSwordScript.SetupSword(finalDir, swordGravity, player, freezeTimeDuration, returnSpeed);

        player.AssignNewSword(newSword);

        DotsActive(false);
    }


    #region Unlock region//解锁区域
    protected override void CheckUnlock()
    {
        UnlockSword();
        UnlockBounceSword();
        UnlockPierceSword();
        UnlockSpinSword();
        UnlockTimeStop();
        UnlockVolnurable();
    }

    private void UnlockTimeStop()//解锁时间停止
    {
        if (timeStopUnlockButton.unlocked)
            timeStopUnlocked = true;
    }

    private void UnlockVolnurable()//解锁易伤
    {
        if (volnurableUnlockButton.unlocked)
            volnurableUnlocked = true;
    }

    private void UnlockSword()//解锁投掷武器
    {
        if (swordUnlockButton.unlocked)
        {
            swordType = SwordType.Regular;
            swordUnlocked = true;
        }
    }


    private void UnlockBounceSword()
    {
        if (bounceUnlockButton.unlocked)
            swordType = SwordType.Bounce;
    }


    private void UnlockPierceSword()
    {
        if (pierceUnlockButton.unlocked)
            swordType = SwordType.Pierce;
    }

    private void UnlockSpinSword()
    {
        if (spinUnlockButton.unlocked)
            swordType = SwordType.Spin;
    }
    #endregion



    #region Aim region
    public Vector2 AimDirection()//瞄准方向
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - playerPosition;//两者相减就是瞄准方向

        return direction;

    }

    public void DotsActive(bool _isActive)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].SetActive(_isActive);
        }
    }

    private void GenerateDots()
    {
        dots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            dots[i] = Instantiate(dotPrefab, player.transform.position, Quaternion.identity, dotsParent);
            dots[i].SetActive(false);
        }
    }

    private Vector2 DotsPosition(float t)//传入顺序相关的点间距
    {
        Vector2 position = (Vector2)player.transform.position + new Vector2
            (AimDirection().normalized.x * launchForce.x,
             AimDirection().normalized.y * launchForce.y) * t + .5f * (Physics2D.gravity * swordGravity) * (t * t);
        //t是控制之间点间距的
        return position;//返回位置
    }//设置点间距函数
    #endregion
}