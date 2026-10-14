using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Blackhole_skill_controller（黑洞技能控制器）- 控制黑洞技能的完整行为
/// 功能：黑洞生长、敌人冻结、热键创建、分身攻击、黑洞收缩
/// </summary>
public class Blackhole_skill_controller : MonoBehaviour
{
    // ==================== 热键系统 ====================
    [SerializeField] private GameObject hotKeyPrefab;
    [SerializeField] private List<KeyCode> keyCodeList;

    // ==================== 黑洞参数 ====================
    private float maxSize;
    private float growSpeed;
    private float shrinkSpeed;
    private float blackholeTimer;

    // ==================== 状态标志 ====================
    private bool canGrow = true;
    private bool canShrink;
    private bool canCreateHotKeys = true;
    private bool cloneAttackReleased;
    private bool playerCanDisaper = true;

    // ==================== 分身攻击参数 ====================
    private int amountOfAttacks = 4;
    private float cloneAttackCooldown = .3f;
    private float cloneAttackTimer;

    // ==================== 目标管理 ====================
    private List<Transform> targets = new List<Transform>();
    private List<GameObject> createdHotKey = new List<GameObject>();
    public bool playerCanExitState { get; private set; }

    // ==================== 初始化 ====================
    // 设置黑洞大小、生长/收缩速度、攻击次数、持续时间等
    public void SetupBlackhole(float _maxSize, float _growSpeed, float _shrinkSpeed, int _amountOfAttacks, float _cloneAttackCooldown, float _blackholeDuration)
    {
        maxSize = _maxSize;
        growSpeed = _growSpeed;
        shrinkSpeed = _shrinkSpeed;
        amountOfAttacks = Mathf.Max(1, _amountOfAttacks);
        cloneAttackCooldown = _cloneAttackCooldown;
        blackholeTimer = _blackholeDuration;

        // 水晶替代分身时玩家不消失
        if (SkillManager.instance.clone.crystalInsteadOfClone)
            playerCanDisaper = false;
    }

    // ==================== 生命周期 ====================
    // 黑洞生长、R键触发分身攻击、时间到自动释放
    private void Update()
    {
        cloneAttackTimer -= Time.deltaTime;
        blackholeTimer -= Time.deltaTime;

        // 按黑洞键提前释放分身攻击（Phase 5：输入经 InputReader，与玩家黑洞输入同键）
        if (InputReader.instance.BlackholePressed && targets.Count > 0 && !cloneAttackReleased)
        {
            ReleaseCloneAttack();
        }

        // 黑洞时间到，自动释放攻击或结束
        if (blackholeTimer < 0)
        {
            blackholeTimer = Mathf.Infinity;
            if (targets.Count > 0 && !cloneAttackReleased)
                ReleaseCloneAttack();
            else
                FinishBlackHoleAbility();
        }

        CloneAttackLogic();

        // 黑洞生长
        if (canGrow && !canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), growSpeed * Time.deltaTime);
        }
        // 黑洞收缩
        if (canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(-1, -1), shrinkSpeed * Time.deltaTime);
            if (transform.localScale.x < 1f)
                Destroy(gameObject);
        }
    }

    // ==================== 分身攻击释放 ====================
    // 玩家按下R键或时间到时触发，开始分身攻击
    private void ReleaseCloneAttack()
    {
        if (targets.Count <= 0) return;

        cloneAttackReleased = true;
        canCreateHotKeys = false;
        DestroyHotKeys();

        // 让玩家隐身
        if (playerCanDisaper)
        {
            playerCanDisaper = false;
            PlayerManager.instance.player.fX.MakeTransprent(true);
        }

        cloneAttackTimer = cloneAttackCooldown;
    }

    // ==================== 分身攻击逻辑 ====================
    // 按冷却时间对随机目标创建分身/水晶攻击
    private void CloneAttackLogic()
    {
        if (cloneAttackTimer < 0 && cloneAttackReleased && amountOfAttacks > 0 && targets.Count > 0)
        {
            cloneAttackTimer = cloneAttackCooldown;

            int randomIndex = Random.Range(0, targets.Count);
            Transform selectedTarget = targets[randomIndex];

            if (selectedTarget != null)
            {
                float xOffset = Random.Range(0, 100) > 50 ? 2 : -2;

                // 水晶替代分身
                if (SkillManager.instance.clone.crystalInsteadOfClone)
                {
                    SkillManager.instance.crystal.CreateCrystal();
                    SkillManager.instance.crystal.CurrentCrystalChooseRandomTarget();
                }
                else
                {
                    SkillManager.instance.clone.CreateClone(selectedTarget, new Vector3(xOffset, 0));
                }
                amountOfAttacks--;

                if (amountOfAttacks <= 0)
                {
                    Invoke("FinishBlackHoleAbility", 0.5f);
                }
            }
            else
            {
                targets.RemoveAt(randomIndex);
            }
        }
    }

    // ==================== 黑洞结束 ====================
    // 销毁热键、恢复玩家、开始收缩
    private void FinishBlackHoleAbility()
    {
        DestroyHotKeys();
        playerCanExitState = true;
        canShrink = true;
        cloneAttackReleased = false;

        // 恢复玩家可见
        if (!playerCanDisaper)
        {
            playerCanDisaper = true;
            PlayerManager.instance.player.fX.MakeTransprent(false);
        }
    }

    // 销毁所有热键
    private void DestroyHotKeys()
    {
        if (createdHotKey.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < createdHotKey.Count; i++)
        {
            Destroy(createdHotKey[i]);
        }
    }

    // ==================== 碰撞检测 ====================
    // 敌人进入黑洞范围时冻结并创建热键
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(true);
            CreateHotKey(collision);
        }
    }

    // 将敌人添加到目标列表
    public void AddEnemyToList(Transform _myEnemy)
    {
        targets.Add(_myEnemy);
    }

    // 敌人离开黑洞范围时解除冻结
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(false);
        }
    }

    // ==================== 热键创建 ====================
    // 在敌人头顶创建随机热键提示
    private void CreateHotKey(Collider2D collision)
    {
        if (keyCodeList.Count == 0)
        {
            return;
        }

        if (!canCreateHotKeys)
        {
            return;
        }

        GameObject newHotKey = Instantiate(hotKeyPrefab, collision.transform.position + new Vector3(0, 2), Quaternion.identity);
        createdHotKey.Add(newHotKey);

        // 随机分配一个KeyCode并移除
        KeyCode choosenKey = keyCodeList[Random.Range(0, keyCodeList.Count)];
        keyCodeList.Remove(choosenKey);

        Blackhole_HotKey_Controller newHotKeyScript = newHotKey.GetComponent<Blackhole_HotKey_Controller>();
        newHotKeyScript.SetupHotKey(choosenKey, collision.transform, this);
    }
}
