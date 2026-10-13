using System.Collections;
using UnityEngine;

public class DeathBringerIdleState : EnemyState
{
    // ==================== 核心引用 ====================
    private Enemy_DeathBringer enemy;
    private Transform player;

    // ==================== 构造函数 ====================
    public DeathBringerIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_DeathBringer _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        this.enemy = _enemy;
    }

    // ==================== 状态生命周期 ====================
    // Enter时设置待机计时器，缓存玩家引用
    // Update中检测玩家距离，进入7单位范围标记战斗开始
    // 调试功能：按V键强制传送
    // 战斗开始后计时器归零切换到战斗状态

    public override void Enter()
    {
        base.Enter();

        stateTimer = enemy.idleTime;
        player = PlayerManager.instance.player.transform;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 玩家进入范围，标记Boss战开始
        if(Vector2.Distance(player.transform.position, enemy.transform.position) < 7)
            enemy.bossFightBegun = true;

        // 调试功能：按V键强制进入传送状态
        if (Input.GetKeyDown(KeyCode.V))
            stateMachine.ChangeState(enemy.teleportState);

        // 战斗已开始且待机计时结束，进入战斗状态
        if(stateTimer < 0 && enemy.bossFightBegun)
            stateMachine.ChangeState(enemy.battleState);
    }
}
