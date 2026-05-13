using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonFireBreath_Controller : MonoBehaviour
{
    private CharacterStats myStats;
    private float moveSpeed = 12f;
    private float lifeTime = 3f;
    private int direction = 1;
    private float damageInterval = 0.2f;
    private float lastDamageTime;

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

    private void Update()
    {
        transform.Translate(Vector2.right * moveSpeed * direction * Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && Time.time >= lastDamageTime + damageInterval)
        {
            lastDamageTime = Time.time;
            myStats.DoDamage(collision.GetComponent<PlayerStats>());
        }
    }
}
