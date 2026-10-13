using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_DeathBringerTriggers : EnemyAnimationTriggers
{
    // ==================== 敌人引用 ====================
    // 通过表达式属性自动获取父级DeathBringer组件
    private Enemy_DeathBringer enemyDeathBringer => GetComponentInParent<Enemy_DeathBringer>();

    // ==================== 动画事件回调 ====================
    // Relocate: 传送时调用，在arena内寻找新的位置
    // MakeInVisible/MakeVisible: 控制传送时的显隐效果

    private void Relocate() => enemyDeathBringer.FindPosition();

    private void MakeInVisible() => enemyDeathBringer.fX.MakeTransprent(true);

    private void MakeVisible() => enemyDeathBringer.fX.MakeTransprent(false);
}
