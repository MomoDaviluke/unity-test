using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemObject_Trigger（物品触发器）- 检测玩家碰撞并触发拾取
/// 功能：玩家碰撞检测、死亡状态检查、调用拾取
/// </summary>
public class ItemObject_Trigger : MonoBehaviour
{
    private ItemObject myItemObject => GetComponentInParent<ItemObject>();

    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            if (collision.GetComponent<CharacterStats>().isDead)
                return;

            myItemObject.PickupItem();
        }
    }
}
