using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DemonCastSpellState（恶魔Boss施法状态）- 面向玩家并施放法术
/// </summary>
public class DemonCastSpellState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_Demon demon;
    private bool hasCastSpell = false;
    private bool spellCasted = false;

    // ==================== 构造函数 ====================
    public DemonCastSpellState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    // ==================== 状态生命周期 ====================
    // Enter时重置施法标志并面朝玩家
    // Update中只执行一次施法，动画完成后返回行走状态

    public override void Enter()
    {
        base.Enter();
        hasCastSpell = false;
        spellCasted = false;

        // 面朝玩家
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            int dir = player.transform.position.x > demon.transform.position.x ? 1 : -1;
            demon.SetupDefaultFacingDir(dir);
        }
    }

    public override void Update()
    {
        base.Update();
        demon.SetZeroVelocity();

        // 只执行一次施法
        if (!hasCastSpell && !spellCasted)
        {
            hasCastSpell = true;
            demon.CastSpell();
            spellCasted = true;
        }

        if (triggerCalled)
            demon.SkillFinished();
    }
}
