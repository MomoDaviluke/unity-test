using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_DeathBringerTriggers : EnemyAnimationTriggers
{
    private Enemy_DeathBringer enemyDeathBringer => GetComponentInParent<Enemy_DeathBringer>();

    private void Relocate() => enemyDeathBringer.FindPosition();

    private void MakeInVisible() => enemyDeathBringer.fX.MakeTransprent(true);

    private void MakeVisible() => enemyDeathBringer.fX.MakeTransprent(false);
}
