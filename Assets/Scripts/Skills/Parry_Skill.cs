using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Parry_SKill : Skill
{
    [Header("反击")]//parry
    [SerializeField] private UI_SKillTreeSlot parryUnlockButton;
    public bool parryUnlocked { get; private set; }

    [Header("反击恢复")]//Parry restor
    [SerializeField] private UI_SKillTreeSlot restorUnlockButton;
    [Range(0f, 1f)]
    [SerializeField] private float restoreHealthPercentage;
    public bool restorUnlocked { get; private set; }

    [Header("格挡幻影")]//Parry mirrage
    [SerializeField] private UI_SKillTreeSlot parrtWithMirageUnlockButton;
    public bool parrtWithMirageUnlocked { get; private set; }


    public override void UseSkill()
    {
        base.UseSkill();

        if (restorUnlocked)
        {
            int restoreAmount = Mathf.RoundToInt(player.stats.GetMaxHealthValue() * restoreHealthPercentage);
            player.stats.IncreaseHealthBy(restoreAmount);

        }
    }


    protected override void Start()
    {
        base.Start();

        parryUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParry);
        restorUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParryRestor);
        parrtWithMirageUnlockButton.GetComponent<Button>().onClick.AddListener(UnlockParrtWithMirage);
    }
    protected override void CheckUnlock()
    {
        UnlockParry();
        UnlockParryRestor();
        UnlockParrtWithMirage();
    }


    private void UnlockParry()
    {
        if (parryUnlockButton.unlocked)
            parryUnlocked = true;
    }

    private void UnlockParryRestor()
    {
        if (restorUnlockButton.unlocked)
            restorUnlocked = true;
    }

    private void UnlockParrtWithMirage()
    {
        if (parrtWithMirageUnlockButton.unlocked)
            parrtWithMirageUnlocked = true;
    }


    public void MakeMirageOnParry(Transform _respawnTransform)
    {
        if (parrtWithMirageUnlocked)
            SkillManager.instance.clone.CreateCloneWithDelay(_respawnTransform);
    }
}