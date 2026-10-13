using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LostCurrencyController（死亡掉落货币控制器）- 控制玩家死亡后掉落的货币
/// 功能：玩家触碰时回收货币
/// </summary>
public class LostCurrencyController : MonoBehaviour
{
    // ==================== 掉落货币数量 ====================
    public int currency;

    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            PlayerManager.instance.currency += currency;
            Destroy(gameObject);
        }
    }
}
