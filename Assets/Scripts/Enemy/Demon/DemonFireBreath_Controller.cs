using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonFireBreath_Controller : MonoBehaviour
{
    // ==================== 核心参数 ====================
    private CharacterStats myStats;
    private float moveSpeed = 12f;
    private float lifeTime = 3f;
    private int direction = 1;
    private float damageInterval = 0.2f;
    private float lastDamageTime;

    // ==================== 初始化 ====================
    // 设置伤害来源并面朝玩家
    public void SetupSpell(CharacterStats _stats)
    {
        myStats = _stats;
        
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            direction = player.transform.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(direction, 1, 1);
        }
        
        Destroy(gameObject, lifeTime);
    }

    // ==================== 移动逻辑 ====================
    // 沿面朝方向直线飞行
    private void Update()
    {
        transform.Translate(Vector2.right * moveSpeed * direction * Time.deltaTime);
    }

    // ==================== 持续伤害检测 ====================
    // 玩家停留在火焰范围内时按间隔造成伤害
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && Time.time >= lastDamageTime + damageInterval)
        {
            lastDamageTime = Time.time;
            myStats.DoDamage(collision.GetComponent<PlayerStats>());
        }
    }
}
