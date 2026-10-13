using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Checkpoint（检查点）- 控制检查点的激活状态和视觉效果
/// 功能：玩家触碰时激活检查点并记录为最后激活的检查点
/// </summary>
public class Checkpoint : MonoBehaviour
{
    // ==================== 核心组件 ====================
    private Animator anim;
    public string id;
    public bool activationStatus;

    // ==================== 初始化 ====================
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // ==================== 生成唯一ID ====================
    [ContextMenu("产生检查点ID")]
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }

    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
            ActivateCheckPoint();
    }

    // ==================== 激活检查点 ====================
    public void ActivateCheckPoint()
    {
        if (activationStatus == false)
        {
            AudioManager.instance.PlaySFX(5, transform);

            if (GameManager.instance != null)
                GameManager.instance.SetLastActivatedCheckpoint(id);
        }

        activationStatus = true;
        anim.SetBool("active", true);
    }
}
