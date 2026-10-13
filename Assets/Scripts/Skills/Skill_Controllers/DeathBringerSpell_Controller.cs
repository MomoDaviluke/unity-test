using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeathBringerSpell_Controller（死亡使者法术控制器）- 控制法术范围伤害
/// 功能：盒型区域检测玩家、造成伤害、Gizmos可视化、自毁
/// </summary>
public class DeathBringerSpell_Controller : MonoBehaviour
{
    // ==================== 检测区域 ====================
    [SerializeField] private Transform check;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private LayerMask whatIsPlayer;

    // ==================== 施法者属性 ====================
    private CharacterStats myStats;

    // ==================== 初始化 ====================
    public void SetupSpell(CharacterStats _stats) => myStats = _stats;

    // ==================== 动画事件 ====================
    // 动画特定帧触发，对盒型区域内玩家造成伤害
    private void AnimationTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(check.position, boxSize, whatIsPlayer);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Player>() != null)
            {
                hit.GetComponent<Entity>().SetupKnockbackDir(transform);
                myStats.DoDamage(hit.GetComponent<CharacterStats>());
            }
        }
    }

    // ==================== Gizmos可视化 ====================
    // 在Scene视图中绘制检测区域
    private void OnDrawGizmos() => Gizmos.DrawWireCube(check.position, boxSize);

    // 自我销毁
    private void SelfDestroy() => Destroy(gameObject);
}
