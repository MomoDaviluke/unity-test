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
    private void Start()
    {
        if (showOnlyOnce && TutorialManager.instance != null)
            hasShown = TutorialManager.instance.HasShownTutorial(tutorialId);
    }

    // ==================== 区域检测 ====================
    private void Update()
    {
        if (hasShown && showOnlyOnce)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, triggerRadius, playerLayer);
        bool currentPlayerInTrigger = false;

        foreach (var hit in hits)
        {
            if (hit.GetComponent<Player>() != null)
            {
                currentPlayerInTrigger = true;
                break;
            }
        }

        if (currentPlayerInTrigger && !playerInTrigger)
        {
            playerInTrigger = true;
            ShowTutorial();
        }
        else if (!currentPlayerInTrigger && playerInTrigger)
        {
            playerInTrigger = false;
            HideTutorial();
        }
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
