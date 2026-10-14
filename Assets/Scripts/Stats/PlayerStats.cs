using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerStats（玩家属性）- 继承自CharacterStats，处理玩家特有的伤害计算和死亡逻辑
/// </summary>
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
            int randomSound = Random.Range(SfxIds.HeavyDamageFirst, SfxIds.HeavyDamageLastExclusive);
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
                        coelonychiaEffect.SetLastDamageTaken(originalDamage);
                        float damageReduction = 1f - coelonychiaEffect.damageReductionPercentage / 100f;
                        _damage = Mathf.RoundToInt(_damage * damageReduction);

                        if (player.lastDamageSource != null)
                            effect.ExecuteEffect(player.lastDamageSource);
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

        Player targetPlayer = _targetStats.GetComponent<Player>();
        if (targetPlayer != null)
            targetPlayer.lastDamageSource = transform;

        if (TargetCanAvoidAttack(_targetStats))
            return;

        int totalDamage = damage.GetValue() + strength.GetValue();

        if (_multipliter > 0)
            totalDamage = Mathf.RoundToInt(totalDamage * _multipliter);

        if (Cancrit())
            totalDamage = CalculateCriticalDamage(totalDamage);

        totalDamage = CheckTargetArmor(_targetStats, totalDamage);
        _targetStats.TakeDamage(totalDamage);
        DoMagicDamage(_targetStats);
    }

    protected override void Die()
    {
        if (isDead)
            return;

        base.Die();

        if (player != null)
            player.Die();

        GameManager.instance.lostCurrencyAmount = PlayerManager.instance.currency;
        PlayerManager.instance.currency = 0;
        GetComponent<PlayerItemDrop>()?.GenerateDrop();
        enabled = false;
    }
}
