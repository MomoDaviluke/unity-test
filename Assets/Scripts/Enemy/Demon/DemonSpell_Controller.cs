using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSpell_Controller : MonoBehaviour
{
    // ==================== 核心参数 ====================
    private CharacterStats myStats;
    private float moveSpeed = 3f;
    private float lifeTime = 5f;
    private int direction = 1;
    private Animator anim;
    private bool hasExploded = false;
    private bool isHoming = false;
    private Transform playerTransform;
    private float damageMultiplier = 0.3f; // 伤害倍率（降低为30%）

    // ==================== 初始化 ====================
    // 设置法术属性：伤害来源、是否跟踪
    // 非跟踪模式时根据玩家位置设置初始方向
    public void SetupSpell(CharacterStats _stats, bool _isHoming = false)
    {
        myStats = _stats;
        isHoming = _isHoming;
        anim = GetComponent<Animator>();
        
        playerTransform = PlayerManager.instance.player?.transform;

        if (!isHoming && playerTransform != null)
        {
            direction = playerTransform.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(direction, 1, 1);
        }
        
        Destroy(gameObject, lifeTime);
    }

    // ==================== 移动逻辑 ====================
    // 跟踪模式：朝向玩家移动
    // 直线模式：沿初始方向直线飞行
    private void Update()
    {
        if (isHoming && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
            
            direction = playerTransform.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(direction, 1, 1);
        }
        else
        {
            transform.Translate(Vector2.right * moveSpeed * direction * Time.deltaTime);
        }
    }

    // ==================== 碰撞检测 ====================
    // 碰到玩家时触发爆炸
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && !hasExploded)
        {
            Explode();
        }
    }

    // ==================== 爆炸处理 ====================
    // 对玩家造成30%基础伤害，播放爆炸动画，0.5秒后销毁
    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            int baseDamage = myStats.damage.GetValue();
            int reducedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            player.GetComponent<CharacterStats>()?.TakeDamage(reducedDamage);
        }

        if (anim != null)
        {
            anim.SetTrigger("Explode");
        }

        Destroy(gameObject, 0.5f);
    }
}
