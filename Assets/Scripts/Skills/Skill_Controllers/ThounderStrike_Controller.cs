using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThounderStrike_Controller : MonoBehaviour
{

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
