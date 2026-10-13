using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AreaSound（区域音效）- 玩家进入/离开区域时播放/停止音效
/// 功能：触发器检测、区域音效控制
/// </summary>
public class AreaSound : MonoBehaviour
{
    [SerializeField] private int areaSoundIndex;

    // ==================== 触发器检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            AudioManager.instance.PlaySFX(areaSoundIndex, null);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            AudioManager.instance.StopSFXWithTime(areaSoundIndex);
        }
    }
}
