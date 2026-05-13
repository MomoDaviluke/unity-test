using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//2024.11.28 17:04
public class GameManager : MonoBehaviour, ISaveManager
{
    public static GameManager instance;//单例模式,全局变量

    private Transform player;//玩家的位置


    [SerializeField] private Checkpoint[] checkpoints;
    [SerializeField] private string lastActivatedCheckpointId;  // 上一次激活的检查点ID

    [Header("Lost Currency")]
    [SerializeField] private GameObject lostCurrencyPrefab;
    public int lostCurrencyAmount;
    [SerializeField] private float lostCurrencyX;//丢失灵魂的位置
    [SerializeField] private float lostCurrencyY;


    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;

        checkpoints = FindObjectsOfType<Checkpoint>();//查找所有的检查点
    }


    private void Start()
    {
        player = PlayerManager.instance.player.transform;
    }


    public void RestartScene()
    {
        SaveManager.instance.SaveGame();//保存游戏
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }



    public void LoadData(GameData _data) => StartCoroutine(LoadWithDelay(_data));


    private void LoadCheckpoint(GameData _data)//加载检查点
    {
        foreach (KeyValuePair<string, bool> pair in _data.checkpoints)//遍历数据中的检查点
        {
            foreach (Checkpoint checkpoint in checkpoints)//遍历场景中的检查点
            {
                if (checkpoint.id == pair.Key && pair.Value == true) //如果检查点的ID和数据中的ID相同且激活状态为真
                    checkpoint.ActivateCheckPoint();

            }
        }
    }



    private void LoadLostCurrency(GameData _data)//加载丢失的灵魂
    {
        lostCurrencyAmount = _data.lostCurrencyAmount;
        lostCurrencyX = _data.lostCurrencyX;
        lostCurrencyY = _data.lostCurrencyY;



        if (lostCurrencyAmount > 0)
        {
            GameObject newlostCurrency = Instantiate(lostCurrencyPrefab, new Vector3(lostCurrencyX, lostCurrencyY), Quaternion.identity);
            newlostCurrency.GetComponent<LostCurrencyController>().currency = lostCurrencyAmount;


        }

        lostCurrencyAmount = 0;//重置丢失的灵魂数量
    }



    private IEnumerator LoadWithDelay(GameData _data)//延迟加载，防止其他数据未加载
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

        // 保存上一次激活的检查点ID（玩家经过的最后一个检查点）
        _data.closestCheckpointId = lastActivatedCheckpointId;

        _data.checkpoints.Clear();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            _data.checkpoints.Add(checkpoint.id, checkpoint.activationStatus);//将检查点的ID和激活状态存入数据
        }
    }



    private void LoadClosetCheckpoint(GameData _data)//将玩家放在上一次激活的检查点
    {
        if (string.IsNullOrEmpty(_data.closestCheckpointId))
            return;

        lastActivatedCheckpointId = _data.closestCheckpointId;//将上一次激活的检查点ID存入变量

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (lastActivatedCheckpointId == checkpoint.id)
                player.position = checkpoint.transform.position;
        }
    }


    private Checkpoint FindClosestCheckpoint()//找到最近的检查点
    {
        float closestDistance = Mathf.Infinity;//正无穷
        Checkpoint closestCheckpoint = null;

        foreach (var checkpoint in checkpoints)//遍历所有的检查点
        {
            float distanceToCheckpoint = Vector2.Distance(player.position, checkpoint.transform.position);//计算玩家和检查点之间的距离

            if (distanceToCheckpoint < closestDistance && checkpoint.activationStatus == true)//如果距离小于最近距离且检查点激活
            {
                closestDistance = distanceToCheckpoint;//更新最近距离
                closestCheckpoint = checkpoint;//更新最近检查点
            }

        }
        return closestCheckpoint;
    }
    
    // 设置上一次激活的检查点ID
    public void SetLastActivatedCheckpoint(string checkpointId)
    {
        lastActivatedCheckpointId = checkpointId;
    }
    
    public void PauseGame(bool _pause)//暂停游戏
    {
        if (_pause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}