using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    private Player player;
    public override void DoDamage(CharacterStats _targetStats)
    {
        base.DoDamage(_targetStats);

    }


    protected override void Start()
    {
        base.Start();
        player = GetComponent<Player>();
    }


    public override void TakeDamage(int _damage)
    {
        if (isDead)
            return;
        if (_damage > GetMaxHealthValue() * .3f)
        {
            player.SetKnockbackPower(new Vector2(7, 10));
            player.fX.ScreenShake(player.fX.shakeHeightDamage);

            int randomSound = Random.Range(31, 35);
            AudioManager.instance.PlaySFX(randomSound, null);
        }

        int originalDamage = _damage;
        ItemData_Equipment armor = null;

        if (Inventory.instance != null)
        {
            armor = Inventory.instance.GetEquipment(EquipmentType.Armor);

            if (armor != null && armor.itemEffects != null)
            {
                foreach (var effect in armor.itemEffects)
                {
                    if (effect is Coelonychia_Effect coelonychiaEffect)
                    {
                        // 设置伤害值
                        coelonychiaEffect.SetLastDamageTaken(originalDamage);

                        // 应用减伤效果
                        float damageReduction = 1f - coelonychiaEffect.damageReductionPercentage / 100f;
                        _damage = Mathf.RoundToInt(_damage * damageReduction);

                        // 如果有攻击者，立即执行反弹
                        if (player.lastDamageSource != null)
                        {
                            effect.ExecuteEffect(player.lastDamageSource);
                        }
                        break;
                    }
                }
            }
        }

        base.DecreaseHealthBy(_damage, true);
    }
    public override void OnEvasion()
    {
        player.skill.dodge.CreateMirageOnDodge();
    }

    public void CloneDoDamage(CharacterStats _targetStats, float _multipliter)
    {
        if (_targetStats == null)
            return;

        // 如果目标是玩家，设置伤害来源
        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
        {
            targetPlayer.lastDamageSource = transform;
        }

        if (TargetCanAvoidAttack(_targetStats))
            return;

        int totalDamage = damage.GetValue() + strength.GetValue();

        if (_multipliter > 0)
        {
            totalDamage = Mathf.RoundToInt(totalDamage * _multipliter);
        }

        //爆伤设置
        if (Cancrit())
        {
            totalDamage = CalculateCriticalDamage(totalDamage);
        }
        totalDamage = CheckTargetArmor(_targetStats, totalDamage);

        _targetStats.TakeDamage(totalDamage);
        DoMagicDamage(_targetStats);
    }

    protected override void Die()
    {
        if (isDead) // 添加isDead标志防止多次触发
            return;

        base.Die();

        if (player != null)
            player.Die();
        GameManager.instance.lostCurrencyAmount = PlayerManager.instance.currency;
        PlayerManager.instance.currency = 0;

        GetComponent<PlayerItemDrop>()?.GenerateDrop();

        // 可能需要禁用玩家输入等
        enabled = false;
    }


}
