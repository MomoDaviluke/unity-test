using UnityEngine;

/// <summary>
/// EnemyStunnedStateBase（眩晕状态基类）- Phase 4 共享状态基类
/// 消灭 Skeleton/Archer/Slime 三份复制：Enter 红色闪烁 + 眩晕计时 + 击退速度，
/// Update 计时归零回待机，Exit 取消闪烁。
/// 差异钩子：
/// - CancelBlinkOnExit：Slime 的 CancelColorChange 在 Update 落地检测内调用
///   （而非 Exit），覆写为 false 保持行为等价；
/// - OnUpdateExtra / OnExitExtra：Slime 的落地眩晕动画 + 无敌帧逻辑。
/// 红线：RedColorBlink / CancelColorChange 为 EntityFX 的字符串反射目标，方法名冻结不动。
/// </summary>
public abstract class EnemyStunnedStateBase : EnemyState
{
    // ==================== 核心引用 ====================
    protected Enemy enemy;

    // ==================== 子类必须提供的状态 ====================
    protected abstract EnemyState IdleState { get; }     // 眩晕结束后的待机状态

    // ==================== 可覆写钩子 ====================
    protected virtual bool CancelBlinkOnExit => true;    // Slime 在 Update 落地时取消，覆写 false
    protected virtual void OnUpdateExtra() { }
    protected virtual void OnExitExtra() { }

    // ==================== 构造函数 ====================
    protected EnemyStunnedStateBase(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
        : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemyBase;
    }

    // ==================== 状态生命周期 ====================
    public override void Enter()
    {
        base.Enter();

        // 红色闪烁视觉反馈（每 0.1 秒一次）
        enemy.fX.InvokeRepeating("RedColorBlink", 0f, 0.1f);

        stateTimer = enemy.stunDuration;

        // 击退：与面朝方向相反的水平力 + 垂直击飞力
        rb.velocity = new Vector2(-enemy.facingDir * enemy.stunDirection.x, enemy.stunDirection.y);
    }

    public override void Exit()
    {
        base.Exit();

        if (CancelBlinkOnExit)
            enemy.fX.Invoke("CancelColorChange", 0f);

        OnExitExtra();
    }

    public override void Update()
    {
        base.Update();

        OnUpdateExtra();

        if (stateTimer < 0)
            stateMachine.ChangeState(IdleState);
    }
}
