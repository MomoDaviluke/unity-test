using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI_SkillTreeSlot（技能树槽位UI）- 显示技能、处理解锁逻辑
/// 功能：技能显示、前置条件检查、金币消耗解锁、存档/加载
/// </summary>
public class UI_SkillTreeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISaveManager
{
    // ==================== 核心引用 ====================
    private UI ui;
    private Image skillImage;

    // ==================== 技能信息 ====================
    [SerializeField] private int skillPirce;
    [SerializeField] private string skillName;
    [TextArea]
    [SerializeField] private string skillDescription;
    [SerializeField] private Color lockedSkillColor;
    public bool unlocked;

    // ==================== 解锁条件 ====================
    [SerializeField] private UI_SkillTreeSlot[] shouldBeUnlocked;
    [SerializeField] private UI_SkillTreeSlot[] shouldBeLocked;

    // ==================== 编辑器验证 ====================
    private void OnValidate()
    {
        gameObject.name = "SkillTreeSlot_UI - " + skillName;
    }

    // ==================== 初始化 ====================
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());
    }

    private void Start()
    {
        skillImage = GetComponent<Image>();
        ui = GetComponentInParent<UI>();

        skillImage.color = lockedSkillColor;
        if (unlocked)
        {
            skillImage.color = Color.white;
        }
    }

    // ==================== 技能解锁 ====================
    // 检查金币、前置条件、互斥条件，满足后解锁技能
    public void UnlockSkillSlot()
    {
        if (unlocked)
            return;

        if (PlayerManager.instance.HaveEnoughMoney(skillPirce) == false)
            return;

        // 前置技能检查
        for (int i = 0; i < shouldBeUnlocked.Length; i++)
        {
            if (shouldBeUnlocked[i].unlocked == false)
            {
                Debug.Log("不能解锁技能");
                return;
            }
        }

        // 互斥技能检查
        for (int i = 0; i < shouldBeLocked.Length; i++)
        {
            if (shouldBeLocked[i].unlocked == true)
            {
                Debug.Log("不能解锁技能");
                return;
            }
        }

        unlocked = true;
        skillImage.color = Color.white;
    }

    // ==================== 鼠标交互 ====================
    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(skillDescription, skillName, skillPirce);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.HideToolTip();
    }

    // ==================== 存档/加载 ====================
    public void LoadData(GameData _data)
    {
        if (_data.skillTree.TryGetValue(skillName, out bool value))
        {
            unlocked = value;
        }
    }

    public void SaveData(ref GameData _data)
    {
        if (_data.skillTree.TryGetValue(skillName, out bool unlocked))
        {
            _data.skillTree.Remove(skillName);
            _data.skillTree.Add(skillName, this.unlocked);
        }
        else
        {
            _data.skillTree.Add(skillName, this.unlocked);
        }
    }
}