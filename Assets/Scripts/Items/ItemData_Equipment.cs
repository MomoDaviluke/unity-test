using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum EquipmentType
{
    Weapon,
    Armor,
    Amulet,
    Flask
}
//装备物品数据
[CreateAssetMenu(fileName = "ItemData", menuName = "Data/Equipment")]
public class ItemData_Equipment : ItemData
{
    public EquipmentType equipmentType;
    public float itemCooldown;
    public ItemEffect[] itemEffects;


    [Header("主要属性")]
    public int strength;      // 力量
    public int agility;       // 敏捷
    public int intelligence;  // 智力
    public int vitality;      // 体力

    [Header("攻击属性")]
    public int damage;        // 伤害
    public int critChance;    // 暴击率
    public int critPower;     // 暴击伤害

    [Header("防御属性")]
    public int health;            // 生命值
    public int armor;             // 护甲值
    public int evasion;          // 闪避率
    public int magicResistance;   // 魔法抗性

    [Header("魔法属性")]
    public int fireDamage;        // 火焰伤害
    public int iceDamage;         // 冰霜伤害
    public int lightingDamage;    // 闪电伤害

    [Header("Craft requirements")]
    public List<InventoryItem> craftRequirements;
    private int minDescriptionLength;

    public void Effect(Transform _enemyPosition)
    {
        foreach (var item in itemEffects)
        {
            item.ExecuteEffect(_enemyPosition);
        }
    }

    public void AddModifiers()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        // 增加力量属性
        playerStats.strength.AddModifier(strength);
        // 增加敏捷属性
        playerStats.agility.AddModifier(agility);
        // 增加智力属性
        playerStats.intelligence.AddModifier(intelligence);
        // 增加体力属性
        playerStats.vitality.AddModifier(vitality);

        // 增加伤害属性
        playerStats.damage.AddModifier(damage);
        // 增加暴击几率属性
        playerStats.critChance.AddModifier(critChance);
        // 增加暴击伤害属性
        playerStats.critpower.AddModifier(critPower);

        // 增加最大生命值属性
        playerStats.maxHealth.AddModifier(health);
        // 增加护甲属性
        playerStats.armor.AddModifier(armor);
        // 增加闪避属性
        playerStats.evasion.AddModifier(evasion);
        // 增加魔法抗性属性
        playerStats.magicResistance.AddModifier(magicResistance);

        // 增加火焰伤害属性
        playerStats.fireDamage.AddModifier(fireDamage);
        // 增加冰霜伤害属性
        playerStats.iceDamage.AddModifier(iceDamage);
        // 增加闪电伤害属性
        playerStats.lightningDamage.AddModifier(lightingDamage);
    }


    public void RemoveModifiers()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.strength.RemoveModifier(strength);
        playerStats.agility.RemoveModifier(agility);
        playerStats.intelligence.RemoveModifier(intelligence);
        playerStats.vitality.RemoveModifier(vitality);

        playerStats.damage.RemoveModifier(damage);
        playerStats.critChance.RemoveModifier(critChance);
        playerStats.critpower.RemoveModifier(critPower);

        playerStats.maxHealth.RemoveModifier(health);
        playerStats.armor.RemoveModifier(armor);
        playerStats.evasion.RemoveModifier(evasion);
        playerStats.magicResistance.RemoveModifier(magicResistance);

        playerStats.fireDamage.RemoveModifier(fireDamage);
        playerStats.iceDamage.RemoveModifier(iceDamage);
        playerStats.lightningDamage.RemoveModifier(lightingDamage);

    }
    public override string GetDescription()
    {
        sb.Length = 0;
        minDescriptionLength = 0;
        //XG: 这里的描述生成逻辑可以根据需要进行调整，目前是简单地将每个属性添加到描述中，如果属性值不为0的话
        //将英文属性名称替换为中文
        AddItemDescription(strength, "力量");
        AddItemDescription(agility, "敏捷");
        AddItemDescription(intelligence, "智力");
        AddItemDescription(vitality, "体力");

        AddItemDescription(damage, "伤害");
        AddItemDescription(critChance, "暴击率");
        AddItemDescription(critPower, "暴击伤害");

        AddItemDescription(health, "生命值");
        AddItemDescription(evasion, "闪避率");
        AddItemDescription(armor, "护甲值");
        AddItemDescription(magicResistance, "魔法抗性");

        AddItemDescription(fireDamage, "火焰伤害");
        AddItemDescription(iceDamage, "冰霜伤害");
        AddItemDescription(lightingDamage, "闪电伤害");


        if (itemEffects != null && itemEffects.Length > 0)
        {
            for (int i = 0; i < itemEffects.Length; i++)
            {
                if (itemEffects[i] != null && !string.IsNullOrEmpty(itemEffects[i].effectDescription))
                {
                    sb.AppendLine();
                    sb.AppendLine("Unique:" + itemEffects[i].effectDescription);
                    minDescriptionLength++;
                }
            }
        }


        if (minDescriptionLength < 5)
        {
            for (int i = 0; i < 5 - minDescriptionLength; i++)
            {
                sb.AppendLine();
                sb.Append("");
            }
        }

        return sb.ToString();
    }
    private void AddItemDescription(int _value, string _name)
    {
        if (_value != 0)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            if (_value > 0)
            {
                sb.Append(_name + " :" + _value);
            }
            minDescriptionLength++;
        }
    }
}
