using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeadZone（死亡区域）- 处理掉入深渊、岩浆等会立即死亡的地方
/// 功能：对角色执行秒杀，对其他物体执行销毁
/// </summary>
public class DeadZone : MonoBehaviour
{
    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterStats>() != null)
            collision.GetComponent<CharacterStats>().KillEntity();
        else
            Destroy(collision.gameObject);
    }
}
