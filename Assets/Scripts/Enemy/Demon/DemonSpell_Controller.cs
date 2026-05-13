using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonSpell_Controller : MonoBehaviour
{
    private CharacterStats myStats;
    private float moveSpeed = 3f;
    private float lifeTime = 5f;
    private int direction = 1;
    private Animator anim;
    private bool hasExploded = false;
    private bool isHoming = false;
    private Transform playerTransform;
    private float damageMultiplier = 0.3f; // 伤害倍率

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

    private void Update()
    {
        if (isHoming && playerTransform != null)
        {
            // 跟踪玩家
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
            
            // 朝向玩家
            direction = playerTransform.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(direction, 1, 1);
        }
        else
        {
            // 直线飞行
            transform.Translate(Vector2.right * moveSpeed * direction * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && !hasExploded)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            // 获取基础伤害并降低到30%
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
