using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SwordType（剑类型枚举）- 定义飞剑的不同模式
/// </summary>
public enum SwordType
{
    Regular,    // 常规模式：投掷后直线飞行，可召回
    Bounce,     // 反弹模式：在敌人之间弹跳
    Pierce,     // 穿透模式：穿透多个敌人
    Spin        // 旋转模式：在目标位置旋转造成伤害
}

/// <summary>
/// Sword_Skill（剑技能）- 继承自Skill，实现可投掷的飞剑
/// 功能：瞄准、投掷、多种剑类型、被动技能
/// 设计思路：鼠标右键瞄准，松开投掷，技能树解锁不同模式
/// </summary>
public class Sword_Skill : Skill
{
    // ==================== 剑类型 ====================
    public SwordType swordType = SwordType.Regular;

    // ==================== 反弹模式参数 ====================
    [Header("反弹模式")]
    [SerializeField] private UI_SKillTreeSlot bounceUnlockButton;
    [SerializeField] private int bounceAmount;
    [SerializeField] private float bounceGravity;
    [SerializeField] private float bounceSpeed;

    // ==================== 穿透模式参数 ====================
    [Header("穿透模式")]
    [SerializeField] private UI_SKillTreeSlot pierceUnlockButton;
    [SerializeField] private int pierceAmount;
    [SerializeField] private float pierceGravity;

    // ==================== 旋转模式参数 ====================
    [Header("旋转模式")]
    [SerializeField] private UI_SKillTreeSlot spinUnlockButton;
    [SerializeField] private float hitCooldown = .35f;
    [SerializeField] private float maxTravelDistance = 7;
    [SerializeField] private float spinDuration = 2;
    [SerializeField] private float spinGravity = 1;

    // ==================== 技能基础参数 ====================
    [Header("技能参数")]
    [SerializeField] private UI_SKillTreeSlot swordUnlockButton;
    public bool swordUnlocked { get; private set; }
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private Vector2 launchForce;
    [SerializeField] private float swordGravity;
    [SerializeField] private float freezeTimeDuration;
    [SerializeField] private float returnSpeed;

    // ==================== 被动技能 ====================
    [Header("被动技能")]
    [SerializeField] private UI_SKillTreeSlot timeStopUnlockButton;
    public bool timeStopUnlocked { get; private set; }
    [SerializeField] private UI_SKillTreeSlot volnurableUnlockButton;
    public bool volnurableUnlocked { get; private set; }

    // ==================== 瞄准系统 ====================
    private Vector2 finalDir;
    [Header("瞄准辅助点")]
    [SerializeField] private int numberOfDots;
    [SerializeField] private float spaceBetweenDots;
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform dotsParent;
    private GameObject[] dots;

    // ==================== 初始化 ====================
    protected override void Start()
    {
        base.Start();

        GenerateDots();
        SetupGravity();

        // 绑定技能树解锁按钮
        swordUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSword);
        bounceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockBounceSword);
        pierceUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockPierceSword);
        spinUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockSpinSword);
        timeStopUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockTimeStop);
        volnurableUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockVolnurable);
    }

    // 根据剑类型设置重力系数
    public void SetupGravity()
    {
        if (swordType == SwordType.Bounce)
            swordGravity = bounceGravity;
        else if (swordType == SwordType.Pierce)
            swordGravity = pierceGravity;
        else if (swordType == SwordType.Spin)
            swordGravity = spinGravity;
    }

    // ==================== 更新与创建 ====================
    // 瞄准期间显示辅助点，松开右键时计算发射方向
    protected override void Update()
    {
        // 松开鼠标时记录最终发射方向
        if (Input.GetKeyUp(KeyCode.Mouse1))
            finalDir = new Vector2(AimDirection().normalized.x * launchForce.x, AimDirection().normalized.y * launchForce.y);

        // 按住鼠标时更新辅助点位置
        if (Input.GetKey(KeyCode.Mouse1))
        {
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].transform.position = DotsPosition(i * spaceBetweenDots);
            }
        }
    }

    // 创建飞剑：实例化预制体并根据剑类型设置参数
    public void CreateSword()
    {
        GameObject newSword = Instantiate(swordPrefab, player.transform.position, transform.rotation);
        Sword_Skill_Controller newSwordScript = newSword.GetComponent<Sword_Skill_Controller>();

        // 根据剑类型设置特殊参数
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

    // ==================== 解锁区域 ====================
    protected override void CheckUnlock()
    {
        UnlockSword();
        UnlockBounceSword();
        UnlockPierceSword();
        UnlockSpinSword();
        UnlockTimeStop();
        UnlockVolnurable();
    }

    private void UnlockTimeStop()
    {
        if (timeStopUnlockButton.unlocked)
            timeStopUnlocked = true;
    }

    private void UnlockVolnurable()
    {
        if (volnurableUnlockButton.unlocked)
            volnurableUnlocked = true;
    }

    private void UnlockSword()
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

    // ==================== 瞄准区域 ====================
    // 计算瞄准方向：从玩家指向鼠标位置
    public Vector2 AimDirection()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - playerPosition;
        return direction;
    }

    // 设置辅助点显示状态
    public void DotsActive(bool _isActive)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].SetActive(_isActive);
        }
    }

    // 生成瞄准辅助点
    private void GenerateDots()
    {
        dots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            dots[i] = Instantiate(dotPrefab, player.transform.position, Quaternion.identity, dotsParent);
            dots[i].SetActive(false);
        }
    }

    // 计算辅助点位置：模拟抛物线轨迹
    private Vector2 DotsPosition(float t)
    {
        Vector2 position = (Vector2)player.transform.position + new Vector2(
            AimDirection().normalized.x * launchForce.x,
            AimDirection().normalized.y * launchForce.y) * t + .5f * (Physics2D.gravity * swordGravity) * (t * t);
        return position;
    }
}
