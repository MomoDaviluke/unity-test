using UnityEngine;

/// <summary>
/// ArcherBattleState（弓箭手战斗状态）- Phase 4 迁移至共享基类
/// 公共逻辑（Enter 缓存玩家、玩家死亡放弃、CanAttack）在 EnemyBattleStateBase；
/// 弓箭手特有：过近后跳拉开距离（CanJump 安全检查 + 冷却）、始终面朝玩家、不主动移动。
/// </summary>
public class ArcherBattleState : EnemyBattleStateBase
{
    // ==================== 核心引用 ====================
    private Enemy_Archer archer;

    // ==================== 基类钩子实现 ====================
    protected override EnemyState MoveState => archer.moveState;

    // ==================== 构造函数 ====================
    public ArcherBattleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Archer enemy)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        archer = enemy;
    }

    // ==================== 状态生命周期 ====================
    public override void Update()
    {
        base.Update();

        // ---- 检测到玩家时：处理距离逻辑 ----
        if (archer.IsPlayerDetected())
        {
            stateTimer = archer.battleTime;

            // 玩家过近（小于安全距离）：尝试后跳拉开距离
            if (archer.IsPlayerDetected().distance < archer.safeDistance)
            {
                if (CanJump())
                    stateMachine.ChangeState(archer.ArcherJumpState);
            }

            // 玩家进入攻击距离：尝试射箭
            if (archer.IsPlayerDetected().distance < archer.attackDistance)
            {
                if (CanAttack())
                    stateMachine.ChangeState(archer.attackState);
            }
        }

        // 始终面朝玩家
        BattleStateFlipController();
    }

    // ==================== 面朝控制 ====================
    private void BattleStateFlipController()
    {
        if (player.position.x > archer.transform.position.x && archer.facingDir == -1)
            archer.Flip();
        else if (player.position.x < archer.transform.position.x && archer.facingDir == 1)
            archer.Flip();
    }

    // ==================== 跳跃（后跳）判定 ====================
    // 检查身后是否有地面和墙壁，确保安全后才允许后跳；同时检查跳跃冷却时间
    private bool CanJump()
    {
        // 安全检查：身后无地面或有墙壁时不允许后跳
        if (archer.GroundBehindCheck() == false || archer.WallBehind() == true)
            return false;

        if (Time.time >= archer.lastTimeJumped + archer.jumpCooldown)
        {
            archer.lastTimeJumped = Time.time;
            return true;
        }

        return false;
    }
}
