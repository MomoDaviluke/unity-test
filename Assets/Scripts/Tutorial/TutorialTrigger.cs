using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TutorialTrigger（教程触发器）- 检测玩家进入区域并显示教程
/// 功能：区域检测、教程显示/隐藏、只显示一次控制
/// </summary>
public class TutorialTrigger : MonoBehaviour
{
    // ==================== 教程设置 ====================
    [Header("教程设置")]
    [SerializeField] private string tutorialId;
    [SerializeField] private string tutorialText;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool showOnlyOnce = true;

    // ==================== 触发区域 ====================
    [Header("触发区域")]
    [SerializeField] private float triggerRadius = 3f;
    [SerializeField] private LayerMask playerLayer;

    // ==================== 可视化 ====================
    [Header("可视化")]
    [SerializeField] private bool showGizmos = true;

    // ==================== 内部状态 ====================
    private bool hasShown = false;
    [SerializeField] private bool playerInTrigger = false;

    // ==================== 初始化 ====================
    // Phase 6：Update 每帧 OverlapCircleAll 轮询 → 物理触发回调。
    // 宿主对象原无 Collider（6 个场景实例），运行时自动挂接触发圆，零场景资产改动；
    // 初始重叠会在首个物理帧触发 OnTriggerEnter2D，与轮询的"开局即在区域内"行为等价。
    private void Awake()
    {
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = triggerRadius;
        col.isTrigger = true;
    }

    private void Start()
    {
        if (showOnlyOnce && TutorialManager.instance != null)
            hasShown = TutorialManager.instance.HasShownTutorial(tutorialId);
    }

    // ==================== 触发回调 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasShown && showOnlyOnce)
            return;

        if (collision.GetComponent<Player>() == null)
            return;

        if (playerInTrigger)
            return;

        playerInTrigger = true;
        ShowTutorial();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 与原 Update 早退一致：只显示一次且已显示后，离开区域不再触发隐藏
        if (hasShown && showOnlyOnce)
            return;

        if (collision.GetComponent<Player>() == null)
            return;

        if (!playerInTrigger)
            return;

        playerInTrigger = false;
        HideTutorial();
    }

    // ==================== 显示/隐藏教程 ====================
    private void ShowTutorial()
    {
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.ShowTutorial(tutorialText, displayDuration);

            if (showOnlyOnce)
            {
                TutorialManager.instance.MarkTutorialAsShown(tutorialId);
                hasShown = true;
            }
        }
    }

    private void HideTutorial()
    {
        if (TutorialManager.instance != null)
            TutorialManager.instance.HideTutorial();
    }

    // ==================== 编辑器可视化 ====================
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = hasShown ? Color.gray : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        if (!string.IsNullOrEmpty(tutorialText))
        {
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, tutorialText);
            #endif
        }
    }

    // ==================== 重置状态 ====================
    public void ResetTutorial()
    {
        hasShown = false;
        playerInTrigger = false;
    }
}
