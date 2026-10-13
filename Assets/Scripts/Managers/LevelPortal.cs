using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelPortal（关卡传送门）- 控制玩家传送到下一关
/// 功能：条件判断、存档保存、特效播放
/// </summary>
public class LevelPortal : MonoBehaviour
{
    // ==================== 传送设置 ====================
    [Header("传送设置")]
    [SerializeField] private string nextLevelName = "Level2Scene";
    [SerializeField] private bool requireEnemiesCleared = false;
    [SerializeField] private GameObject portalEffect;

    private bool canTransfer = true;

    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[LevelPortal] 触发器触发，碰撞对象: {collision.gameObject.name}");

        if (!canTransfer)
        {
            Debug.Log("[LevelPortal] 传送门不可用，跳过");
            return;
        }

        if (collision.GetComponent<Player>() != null)
        {
            Debug.Log("[LevelPortal] 检测到玩家");

            if (requireEnemiesCleared && !AreEnemiesCleared())
            {
                Debug.Log("[LevelPortal] 需要先清空所有敌人！");
                return;
            }

            Debug.Log($"[LevelPortal] 开始传送，目标关卡: {nextLevelName}");
            TransferToNextLevel();
        }
        else
        {
            Debug.Log($"[LevelPortal] 碰撞对象不是玩家: {collision.gameObject.name}");
        }
    }

    // ==================== 传送逻辑 ====================
    private void TransferToNextLevel()
    {
        canTransfer = false;
        Debug.Log("[LevelPortal] 执行传送操作");

        if (SaveManager.instance != null)
        {
            Debug.Log("[LevelPortal] 保存游戏");
            SaveManager.instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[LevelPortal] SaveManager 不存在");
        }

        if (AudioManager.instance != null)
        {
            Debug.Log("[LevelPortal] 播放音效");
            AudioManager.instance.PlaySFX(10, transform);
        }
        else
        {
            Debug.LogWarning("[LevelPortal] AudioManager 不存在");
        }

        if (LevelManager.instance != null)
        {
            Debug.Log($"[LevelPortal] 调用 LevelManager 加载关卡: {nextLevelName}");
            LevelManager.instance.LoadLevel(nextLevelName);
        }
        else
        {
            Debug.LogError("[LevelPortal] LevelManager 不存在！");
        }
    }

    // ==================== 敌人检查 ====================
    private bool AreEnemiesCleared()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"[LevelPortal] 场景中敌人数量: {enemies.Length}");
        return enemies.Length == 0;
    }

    // ==================== 传送门控制 ====================
    public void SetPortalActive(bool active)
    {
        canTransfer = active;
        if (portalEffect != null)
            portalEffect.SetActive(active);
    }
}
