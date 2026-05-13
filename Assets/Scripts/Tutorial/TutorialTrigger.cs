using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("教程设置")]
    [SerializeField] private string tutorialId;
     // 教程文本内容
    [SerializeField] private string tutorialText;
    [SerializeField] private float displayDuration = 5f; // 显示持续时间
    [SerializeField] private bool showOnlyOnce = true; // 是否只显示一次

    [Header("触发区域")]
    [SerializeField] private float triggerRadius = 3f; // 触发半径
    [SerializeField] private LayerMask playerLayer; // 玩家层

    [Header("可视化")]
    [SerializeField] private bool showGizmos = true; // 是否在编辑器中显示触发区域

    private bool hasShown = false; // 是否已经显示过
    [SerializeField] private bool playerInTrigger = false; // 玩家是否在触发区域内

    private void Start()
    {
        if (showOnlyOnce && TutorialManager.instance != null)
        {
            hasShown = TutorialManager.instance.HasShownTutorial(tutorialId);
        }
    }

    private void Update()
    {
        // 如果已经显示过且设置为只显示一次，则不再检测
        if (hasShown && showOnlyOnce)
            return;

        // 检测玩家是否在触发区域内
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

        // 玩家进入区域
        if (currentPlayerInTrigger && !playerInTrigger)
        {
            playerInTrigger = true;
            ShowTutorial();
        }
        // 玩家离开区域
        else if (!currentPlayerInTrigger && playerInTrigger)
        {
            playerInTrigger = false;
            HideTutorial();
        }
    }

    // 显示教程
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

    // 隐藏教程
    private void HideTutorial()
    {
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.HideTutorial();
        }
    }

    // 在编辑器中绘制触发区域
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = hasShown ? Color.gray : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // 显示教程文本
        if (!string.IsNullOrEmpty(tutorialText))
        {
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, tutorialText);
            #endif
        }
    }

    // 重置教程状态（用于测试）
    public void ResetTutorial()
    {
        hasShown = false;
        playerInTrigger = false;
    }
}
