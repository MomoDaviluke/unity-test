using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ThounderStrike_Controller（雷电打击控制器基类）- 提供基础雷电伤害逻辑
/// 功能：检测敌人并执行魔法伤害，其他技能控制器可继承扩展
/// </summary>
public class ThounderStrike_Controller : MonoBehaviour
{
    // ==================== 碰撞伤害 ====================
    // 检测敌人并执行魔法伤害
    protected void OggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            EnemyStats enemyTarget = collision.GetComponent<EnemyStats>();
            playerStats.DoMagicDamage(enemyTarget);
        }
    }
}
