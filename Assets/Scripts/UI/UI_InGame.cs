using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI_InGame（游戏内UI）- 显示血条、技能冷却、灵魂值
/// 功能：血量显示、技能图标冷却、灵魂值平滑增长
/// Phase 3 解耦：血条沿用 onHealthChanged 事件；灵魂值目标改由 OnCurrencyChanged 推送（消除每帧读 PlayerManager）；
/// 药水瓶装备状态缓存为布尔值并经 OnInventoryUpdated 更新（消除按键时的字典扫描）。
/// 冷却遮罩的逐帧递减是时间驱动动画（非状态轮询），保留在 Update。
/// </summary>
public class UI_InGame : MonoBehaviour
{
    // ==================== 玩家状态 ====================
    [Header("玩家状态")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider slider;

    // ==================== 技能图标 ====================
    [Header("技能图标")]
    [SerializeField] private Image dashImage;
    [SerializeField] private Image parryImage;
    [SerializeField] private Image crystalImage;
    [SerializeField] private Image swordImage;
    [SerializeField] private Image blackholeImage;
    [SerializeField] private Image flaskImage;

    private SkillManager skills;

    // ==================== 灵魂值 ====================
    [Header("灵魂值信息")]
    [SerializeField] private TextMeshProUGUI currentSouls;
    [SerializeField] private float soulsAmount;
    [SerializeField] private float increaseRate;

    // 灵魂值目标：由 OnCurrencyChanged 推送，Update 只做平滑动画
    private int soulsTarget;

    // 药水瓶装备状态缓存：经 OnInventoryUpdated 更新，避免按键时扫描装备字典
    private bool flaskEquipped;

    // ==================== 初始化 ====================
    void Start()
    {
        if (playerStats != null)
            playerStats.onHealthChanged += UpdateHealthUI;

        skills = SkillManager.instance;

        // 订阅后立即主动拉一次初值（防漏首次刷新）
        soulsTarget = PlayerManager.instance != null ? PlayerManager.instance.GetCurrency() : 0;
        soulsAmount = soulsTarget;
        RefreshFlaskState();
    }

    // ==================== 事件订阅 ====================
    private void OnEnable()
    {
        GameEvents.OnCurrencyChanged += OnCurrencyChanged;
        GameEvents.OnInventoryUpdated += OnInventoryUpdated;
    }

    private void OnDisable()
    {
        GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
        GameEvents.OnInventoryUpdated -= OnInventoryUpdated;
    }

    private void OnCurrencyChanged(int _newAmount)
    {
        soulsTarget = _newAmount;
    }

    private void OnInventoryUpdated()
    {
        RefreshFlaskState();
    }

    private void RefreshFlaskState()
    {
        flaskEquipped = Inventory.instance != null
            && Inventory.instance.GetEquipment(EquipmentType.Flask) != null;
    }

    // ==================== 更新 ====================
    // 灵魂值平滑增长、检测技能按键、更新冷却
    void Update()
    {
        UpdataSoulsUI();

        // 检测技能按键，设置冷却（Phase 5 补充：全部经 InputReader 语义读取，
        // 与玩家动作同源——Inspector 改键后图标跟随，不再各自硬编码）
        // 修复：冲刺图标原检测 LeftShift，与实际冲刺键（InputReader.dashKey=Q）不符，
        // 导致释放冲刺后冷却图标从不点亮；现与玩家动作读同一语义输入（改键同步生效）
        if (InputReader.instance.DashPressed && skills.dash.dashUnlocked)
            SetCooldownOf(dashImage);
        if (InputReader.instance.ParryPressed && skills.parry.parryUnlocked)
            SetCooldownOf(parryImage);
        if (InputReader.instance.CrystalPressed && skills.crystal.crystalUnlocked)
            SetCooldownOf(crystalImage);
        if (InputReader.instance.AimSwordPressed && skills.sword.swordUnlocked)
            SetCooldownOf(swordImage);
        if (InputReader.instance.BlackholePressed && skills.blackhole.blackHoleUnlocked)
            SetCooldownOf(blackholeImage);
        if (InputReader.instance.FlaskPressed && flaskEquipped)
            SetCooldownOf(flaskImage);

        // 更新所有技能冷却显示
        CheckCoolDownOf(dashImage, skills.dash.cooldown);
        CheckCoolDownOf(parryImage, skills.parry.cooldown);
        CheckCoolDownOf(crystalImage, skills.crystal.cooldown);
        CheckCoolDownOf(swordImage, skills.sword.cooldown);
        CheckCoolDownOf(blackholeImage, skills.blackhole.cooldown);
        CheckCoolDownOf(flaskImage, Inventory.instance.flaskCooldown);
    }

    // ==================== 灵魂值显示 ====================
    // 平滑增长显示灵魂值（目标值由事件推送，此处仅做动画插值）
    private void UpdataSoulsUI()
    {
        if (soulsAmount < soulsTarget)
            soulsAmount += increaseRate * Time.deltaTime;
        else
            soulsAmount = soulsTarget;

        currentSouls.text = ((int)soulsAmount).ToString();
    }

    // ==================== 血量显示 ====================
    private void UpdateHealthUI()
    {
        slider.maxValue = playerStats.GetMaxHealthValue();
        slider.value = playerStats.currentHealth;
    }

    // ==================== 技能冷却 ====================
    // 设置技能图标为满冷却状态
    private void SetCooldownOf(Image _image)
    {
        if (_image.fillAmount <= 0)
            _image.fillAmount = 1;
    }

    // 每帧减少冷却显示
    private void CheckCoolDownOf(Image _image, float _coolDown)
    {
        if (_image.fillAmount > 0)
            _image.fillAmount -= 1 / _coolDown * Time.deltaTime;
    }
}
