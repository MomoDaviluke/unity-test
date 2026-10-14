using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerCombat（玩家战斗职责）- Phase 5 从 Player.cs 拆分的 partial 类
/// 设计思路：只做"搬家"不做重写——攻击参数、剑引用与投掷/接剑、忙碌锁定
/// 原样迁移，成员名与访问级别不变（partial 同类，player.attackMovement 等
/// 状态类读法零改动，场景序列化字段名不变）。
/// Player 保留状态机宿主/移动/跳跃/冲刺/闪避职责。
/// </summary>
public partial class Player
{
    // ==================== 攻击与战斗参数 ====================
    // attackMovement数组存储每段攻击的前冲距离，实现攻击位移效果
    // isBusy标志用于锁定输入，防止玩家在攻击时同时执行其他动作
    [Header("攻击参数")]
    public Vector2[] attackMovement;                    // 攻击时的位移（每段攻击向前突进的距离）
    public float counterAttackDuration = 0.2f;          // 反击窗口持续时间（这段时间内被攻击会触发反击）
    public bool isBusy { get; private set; }            // 是否处于忙碌状态（执行动作时无法响应其他输入）
    [HideInInspector] public Transform lastDamageSource; // 上次受到伤害的来源（用于反击时追踪敌人位置）

    // ==================== 剑投掷与接剑系统 ====================
    // sword引用用于追踪投掷出的剑，接剑时销毁并切换状态
    public GameObject sword { get; private set; }       // 当前投掷出的剑对象

    /// <summary>
    /// 分配新剑（投掷技能使用）
    /// </summary>
    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    /// <summary>
    /// 接剑 - 切换到接剑状态并销毁飞剑
    /// </summary>
    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword);
    }

    // ==================== 忙碌状态 ====================
    // BusyFor协程用于锁定玩家输入，常见于攻击前摇/后摇期间

    /// <summary>
    /// 忙碌协程 - 在指定时间内无法执行其他动作（如攻击时禁止移动）
    /// </summary>
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(_seconds);
        isBusy = false;
    }
}
