using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 控制黑洞技能的类
public class Blackhole_skill_controller : MonoBehaviour
{
    [SerializeField] private GameObject hotKeyPrefab;
    [SerializeField] private List<KeyCode> keyCodeList;

    private float maxSize;
    private float growSpeed;
    private float shrinkSpeed;
    private float blackholeTimer;

    private bool canGrow = true;
    private bool canShrink;
    private bool canCreateHotKeys = true;
    private bool cloneAttackReleased;
    private bool playerCanDisaper = true;

    private int amountOfAttacks = 4;
    private float cloneAttackCooldown = .3f;
    private float cloneAttackTimer;

    private List<Transform> targets = new List<Transform>();
    private List<GameObject> createdHotKey = new List<GameObject>();

    public bool playerCanExitState { get; private set; }

    // 设置黑洞技能的参数
    public void SetupBlackhole(float _maxSize, float _growSpeed, float _shrinkSpeed, int _amountOfAttacks, float _cloneAttackCooldown, float _blackholeDuration)
    {
        maxSize = _maxSize;
        growSpeed = _growSpeed;
        shrinkSpeed = _shrinkSpeed;
        // 确保攻击次数至少为1
        amountOfAttacks = Mathf.Max(1, _amountOfAttacks);
        cloneAttackCooldown = _cloneAttackCooldown;

        blackholeTimer = _blackholeDuration;

        if (SkillManager.instance.clone.crystalInsteadOfClone)
            playerCanDisaper = false;
    }

    private void Update()
    {
        cloneAttackTimer -= Time.deltaTime;
        blackholeTimer -= Time.deltaTime;

        // 简化R键检测逻辑
        if (Input.GetKeyDown(KeyCode.R) && targets.Count > 0 && !cloneAttackReleased)
        {
            ReleaseCloneAttack();
        }

        if (blackholeTimer < 0)
        {
            blackholeTimer = Mathf.Infinity;
            if (targets.Count > 0 && !cloneAttackReleased)
                ReleaseCloneAttack();
            else
                FinishBlackHoleAbility();
        }

        CloneAttackLogic();

        if (canGrow && !canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), growSpeed * Time.deltaTime);
        }
        if (canShrink)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(-1, -1), shrinkSpeed * Time.deltaTime);
            if (transform.localScale.x < 1f)
                Destroy(gameObject);
        }
    }

    private void ReleaseCloneAttack()
    {
        if (targets.Count <= 0) return;  // 添加保护检查

        cloneAttackReleased = true;
        canCreateHotKeys = false;
        DestroyHotKeys();

        if (playerCanDisaper)
        {
            playerCanDisaper = false;
            PlayerManager.instance.player.fX.MakeTransprent(true);
        }

        // 初始化克隆攻击计时器
        cloneAttackTimer = cloneAttackCooldown;
    }

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
                // 如果目标无效，从列表中移除
                targets.RemoveAt(randomIndex);
            }
        }
    }

    private void FinishBlackHoleAbility()
    {
        DestroyHotKeys();
        playerCanExitState = true;
        canShrink = true;
        cloneAttackReleased = false;

        // 恢复玩家状态
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

    // 当物体进入2D碰撞器时的处理
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(true);
            CreateHotKey(collision);
        }
    }

    // 将敌人添加到目标列表中
    public void AddEnemyToList(Transform _myEnemy)
    {
        targets.Add(_myEnemy);
    }


    // 当物体离开2D碰撞器时的处理
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            collision.GetComponent<Enemy>().FreezeTime(false);

        }
    }

    // 创建热键
    private void CreateHotKey(Collider2D collision)
    {
        if (keyCodeList.Count == 0)//当所有的KeyCode都被去除，就不在创建实例
        {
            return;
        }

        if (!canCreateHotKeys)//这是当角色已经开大了，不在创建实例
        {
            return;
        }

        //创建实例
        GameObject newHotKey = Instantiate(hotKeyPrefab, collision.transform.position + new Vector3(0, 2), Quaternion.identity);

        //将实例添加进列表
        createdHotKey.Add(newHotKey);


        //随机KeyCode传给HotKey，并且传过去一个毁掉一个
        KeyCode choosenKey = keyCodeList[Random.Range(0, keyCodeList.Count)];

        keyCodeList.Remove(choosenKey);

        Blackhole_HotKey_Controller newHotKeyScript = newHotKey.GetComponent<Blackhole_HotKey_Controller>();

        newHotKeyScript.SetupHotKey(choosenKey, collision.transform, this);
    }

}
