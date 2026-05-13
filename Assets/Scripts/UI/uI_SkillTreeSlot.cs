using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//2024年11月17日
public class UI_SKillTreeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISaveManager
{
    private UI ui;
    private Image skillImage;//当前技能槽的图片组件

    [SerializeField] private int skillPirce;//技能价格
    [SerializeField] private string skillName;
    [TextArea]
    [SerializeField] private string skillDescription;
    [SerializeField] private Color lockedSkillColor;//技能未解锁时的颜色


    public bool unlocked;

    [SerializeField] private UI_SKillTreeSlot[] shouldBeUnlocked;//解锁前置条件
    [SerializeField] private UI_SKillTreeSlot[] shouldBeLocked;//锁定条件



    private void OnValidate()
    {
        gameObject.name = "SkillTreeSlot_UI - " + skillName;
    }
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());//给 Button 组件绑定点击事件，用于触发技能槽解锁逻辑
    }

    private void Start()
    {
        skillImage = GetComponent<Image>();
        ui = GetComponentInParent<UI>();

        skillImage.color = lockedSkillColor;//设置初始颜色为锁定颜色
        if (unlocked)
        {
            skillImage.color = Color.white;//如果已经解锁，设置颜色为白色
        }
    }

    public void UnlockSkillSlot()
    {
        if (unlocked)
            return; // 如果已经解锁，直接返回

        if (PlayerManager.instance.HaveEnoughMoney(skillPirce) == false)//检查是否有足够的钱
            return;
        for (int i = 0; i < shouldBeUnlocked.Length; i++)//前置解锁检查
        {
            if (shouldBeUnlocked[i].unlocked == false)
            {
                Debug.Log("不能解锁技能");
                return;
            }
        }

        for (int i = 0; i < shouldBeLocked.Length; i++)//锁定检查
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(skillDescription, skillName, skillPirce);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.HideToolTip();//鼠标离开槽位时，隐藏技能描述提示框
    }

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