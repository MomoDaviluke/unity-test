using UnityEngine;
using UnityEngine.SceneManagement;

// 关卡传送门：玩家触碰后传送到下一关
public class LevelPortal : MonoBehaviour
{
    [Header("传送设置")]
    [SerializeField] private string nextLevelName = "Level2Scene";  // 下一关场景名
    [SerializeField] private bool requireEnemiesCleared = false;    // 是否需要清空敌人
    [SerializeField] private GameObject portalEffect;               // 传送门特效
    
    private bool canTransfer = true;
    
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
            
            // 检查是否需要清空敌人
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
    
    // 传送到下一关
    private void TransferToNextLevel()
    {
        canTransfer = false;
        Debug.Log("[LevelPortal] 执行传送操作");
        
        // 保存游戏
        if (SaveManager.instance != null)
        {
            Debug.Log("[LevelPortal] 保存游戏");
            SaveManager.instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[LevelPortal] SaveManager 不存在");
        }
        
        // 播放音效
        if (AudioManager.instance != null)
        {
            Debug.Log("[LevelPortal] 播放音效");
            AudioManager.instance.PlaySFX(10, transform);
        }
        else
        {
            Debug.LogWarning("[LevelPortal] AudioManager 不存在");
        }
        
        // 使用 LevelManager 加载下一关（异步叠加加载）
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
    
    // 检查是否清空敌人
    private bool AreEnemiesCleared()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"[LevelPortal] 场景中敌人数量: {enemies.Length}");
        return enemies.Length == 0;
    }
    
    // 设置传送门是否可用
    public void SetPortalActive(bool active)
    {
        canTransfer = active;
        if (portalEffect != null)
        {
            portalEffect.SetActive(active);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(2, 3, 0));
    }
}
