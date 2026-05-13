using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonCastSpellState : EnemyState
{
    private Enemy_Demon demon;
    private bool hasCastSpell = false;
    private bool spellCasted = false;

    public DemonCastSpellState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Demon _demon) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.demon = _demon;
    }

    public override void Enter()
    {
        base.Enter();
        hasCastSpell = false;
        spellCasted = false;
        
        // 面向玩家
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

        // 保持原地
        demon.SetZeroVelocity();

        // 立即施放技能（只施放一次）
        if (!hasCastSpell && !spellCasted)
        {
            hasCastSpell = true;
            demon.CastSpell();
            spellCasted = true;
        }

        // 动画结束，触发技能完成
        if (triggerCalled)
        {
            demon.SkillFinished();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
