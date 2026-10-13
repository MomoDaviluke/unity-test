using System.Collections;
using UnityEngine;

public class DeathBringerSpellCastState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;

    // ==================== 施法控制变量 ====================
    private int amountOfSpells; // 剩余法术数量
    private float spellTimer;   // 法术释放间隔计时器

    // ==================== 构造函数 ====================
    public DeathBringerSpellCastState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时初始化法术数量和首次施法延迟（0.5秒）
    // Update中倒计时并释放法术，所有法术释放完毕后进入传送状态
    // Exit时记录施法时间

    public override void Enter()
    {
        base.Enter();

        amountOfSpells = enemy.amountOfSpells;
        spellTimer = .5f;
    }

    public override void Update()
    {
        base.Update();

        spellTimer -= Time.deltaTime;

        if (CanCast())
            enemy.CastSpell();
        
        if(amountOfSpells <= 0)
            stateMachine.ChangeState(enemy.teleportState);
    }

    public override void Exit()
    {
        base.Exit();

        enemy.lastTimeCast = Time.time;
    }

    // ==================== 施法判定 ====================
    // 检查是否还有剩余法术且冷却完成
    // 每次释放后减少计数并重置冷却
    private bool CanCast()
    {
        if(amountOfSpells > 0 && spellTimer < 0)
        {
            amountOfSpells--;
            spellTimer = enemy.spellCooldown;
            return true;
        }

        return false;
    }
}
