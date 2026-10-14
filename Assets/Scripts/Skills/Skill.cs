using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill（技能基类）- 管理技能冷却、释放逻辑、解锁状态
/// 设计思路：所有技能继承此类，统一处理冷却计时和释放判定
/// 使用场景：剑投掷、分身、黑洞、冲刺等所有玩家技能
/// </summary>
public class Skill : MonoBehaviour
{
    // ==================== 技能冷却参数 ====================
    // cooldown是技能的基础冷却时间，cooldownTimer是当前剩余冷却时间
    // 当cooldownTimer <= 0时，技能可用；释放后重置为cooldown值
    [Header("技能参数")]
    public float cooldown;          // 技能冷却时间（秒）：两次释放之间的最小间隔
    public float cooldownTimer;     // 当前冷却计时器：<=0时表示技能可用

    // Phase 6：敌人搜索半径按技能配置（默认 25 与原硬编码一致，行为不变）
    [Header("检测")]
    [SerializeField] private float enemySearchRadius = 25;

    // NonAlloc 复用缓冲（半径 25 内碰撞体数量有限，128 足够；超出截断仅极端堆叠场景）
    private readonly Collider2D[] enemySearchBuffer = new Collider2D[128];

    // ==================== 玩家引用 ====================
    // player通过PlayerManager单例获取，子类使用它来获取玩家位置、状态等信息
    protected Player player;        // 玩家引用：用于获取玩家位置和状态

    // ==================== 生命周期方法 ====================
    // Start获取玩家引用并检查解锁状态（子类重写CheckUnlock）
    // Update每帧减少冷却计时器，实现冷却倒计时

    /// <summary>
    /// 启动时获取玩家引用并检查解锁状态
    /// </summary>
    protected virtual void Start()
    {
        player = PlayerManager.instance.player;
        CheckUnlock();
    }

    /// <summary>
    /// 每帧更新冷却计时器
    /// </summary>
    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime; // 每帧减少冷却时间
    }

    // ==================== 技能解锁检查 ====================
    // 子类重写此方法实现具体的解锁逻辑（如消耗技能点、达到等级等）
    /// <summary>
    /// 检查技能是否已解锁：子类重写实现具体解锁逻辑
    /// </summary>
    protected virtual void CheckUnlock() { }

    // ==================== 技能释放接口 ====================
    // CanUseSkill是外部调用入口：检查冷却 → 释放技能 → 重置冷却
    // 冷却中显示"Cooldown"提示，提供即时反馈

    /// <summary>
    /// 检查是否可以使用技能：冷却完成则释放，否则显示提示
    /// </summary>
    /// <returns>是否成功释放技能</returns>
    public virtual bool CanUseSkill()
    {
        if (cooldownTimer < 0)
        {
            UseSkill();
            cooldownTimer = cooldown; // 重置冷却计时器
            return true;
        }
        player.fX.CreatePopUpText("Cooldown"); // 显示冷却提示
        return false;
    }

    /// <summary>
    /// 释放技能：子类重写实现具体技能效果
    /// </summary>
    public virtual void UseSkill() { }

    // ==================== 辅助方法：查找最近敌人 ====================
    // NonAlloc 范围检测，遍历命中碰撞体找到最近的 Enemy
    // 用于追踪类技能（飞剑返回、分身攻击目标选择等）

    /// <summary>
    /// 查找最近的敌人：用于追踪类技能（如飞剑、分身攻击）
    /// Phase 6：OverlapCircleAll 每次分配数组 → NonAlloc 复用缓冲；半径改为可配置（默认 25 等价）
    /// </summary>
    /// <param name="_checkTransform">检测中心点</param>
    /// <returns>最近敌人的Transform，如果没有则返回null</returns>
    protected virtual Transform FindClosestEnemy(Transform _checkTransform)
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(_checkTransform.position, enemySearchRadius, enemySearchBuffer);
        float closestDistance = Mathf.Infinity;
        Transform closesEnemy = null;

        // 遍历命中的碰撞体，找到距离最近的敌人
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = enemySearchBuffer[i];

            if (hit.GetComponent<Enemy>() != null)
            {
                float distanceToEnemy = Vector2.Distance(_checkTransform.position, hit.transform.position);
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closesEnemy = hit.transform;
                }
            }
        }

        return closesEnemy;
    }
}
