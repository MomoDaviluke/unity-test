using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager（游戏管理器）- 管理检查点、死亡掉落和场景重启
/// 功能：检查点管理、死亡掉落货币、游戏暂停
/// </summary>
public class GameManager : MonoBehaviour, ISaveManager
{
    // ==================== 单例 ====================
    public static GameManager instance;

    // ==================== 核心引用 ====================
    private Transform player;
    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private string lastActivatedCheckpointId;

    // ==================== 死亡掉落货币 ====================
    [Header("Lost Currency")]
    [SerializeField] private GameObject lostCurrencyPrefab;
    public int lostCurrencyAmount;
    [SerializeField] private float lostCurrencyX;
    [SerializeField] private float lostCurrencyY;

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        checkpoints = FindObjectsOfType<Checkpoint>();
    }

    private void Start()
    {
        player = PlayerManager.instance.player.transform;
    }

    // ==================== 场景重启 ====================
    public void RestartScene()
    {
        SaveManager.instance.SaveGame();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    // ==================== 存档/加载 ====================
    public void LoadData(GameData _data) => StartCoroutine(LoadWithDelay(_data));

    private void LoadCheckpoint(GameData _data)
    {
        foreach (KeyValuePair<string, bool> pair in _data.checkpoints)
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint.id == pair.Key && pair.Value == true)
                    checkpoint.ActivateCheckPoint();
            }
        }
    }

    private void LoadLostCurrency(GameData _data)
    {
        lostCurrencyAmount = _data.lostCurrencyAmount;
        lostCurrencyX = _data.lostCurrencyX;
        lostCurrencyY = _data.lostCurrencyY;

        if (lostCurrencyAmount > 0)
        {
            GameObject newlostCurrency = Instantiate(lostCurrencyPrefab, new Vector3(lostCurrencyX, lostCurrencyY), Quaternion.identity);
            newlostCurrency.GetComponent<LostCurrencyController>().currency = lostCurrencyAmount;
        }

        lostCurrencyAmount = 0;
    }

    private IEnumerator LoadWithDelay(GameData _data)
    {
        yield return new WaitForSeconds(.5f);
        LoadCheckpoint(_data);
        LoadClosetCheckpoint(_data);
        LoadLostCurrency(_data);
    }

    public void SaveData(ref GameData _data)
    {
        _data.lostCurrencyAmount = lostCurrencyAmount;
        _data.lostCurrencyX = player.position.x;
        _data.lostCurrencyY = player.position.y;
        _data.closestCheckpointId = lastActivatedCheckpointId;
        _data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
            _data.checkpoints.Add(checkpoint.id, checkpoint.activationStatus);
    }

    // ==================== 检查点管理 ====================
    private void LoadClosetCheckpoint(GameData _data)
    {
        if (string.IsNullOrEmpty(_data.closestCheckpointId))
            return;

        lastActivatedCheckpointId = _data.closestCheckpointId;

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (lastActivatedCheckpointId == checkpoint.id)
                player.position = checkpoint.transform.position;
        }
    }

    private Checkpoint FindClosestCheckpoint()
    {
        float closestDistance = Mathf.Infinity;
        Checkpoint closestCheckpoint = null;

        foreach (var checkpoint in checkpoints)
        {
            float distanceToCheckpoint = Vector2.Distance(player.position, checkpoint.transform.position);

            if (distanceToCheckpoint < closestDistance && checkpoint.activationStatus == true)
            {
                closestDistance = distanceToCheckpoint;
                closestCheckpoint = checkpoint;
            }
        }
        return closestCheckpoint;
    }

    public void SetLastActivatedCheckpoint(string checkpointId)
    {
        lastActivatedCheckpointId = checkpointId;
    }

    // ==================== 游戏暂停 ====================
    public void PauseGame(bool _pause)
    {
        if (_pause)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }
}
