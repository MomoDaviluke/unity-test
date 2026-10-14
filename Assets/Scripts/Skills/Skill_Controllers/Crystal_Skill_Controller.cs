using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crystal_Skill_Controller（水晶技能控制器）- 控制水晶的行为
/// 功能：水晶持续时间、追踪敌人、爆炸、变大
/// </summary>
public class Crystal_Skill_Controller : MonoBehaviour
{
    // ==================== 核心引用 ====================
    // Phase 6 缓存化：原表达式属性每次访问 GetComponent（Update/爆炸事件高频）→ Awake 缓存一次
    private Animator anim;
    private CircleCollider2D cd;
    private Player player;

    // NonAlloc 复用缓冲（水晶爆炸/黑洞随机选敌的范围检测）
    private readonly Collider2D[] overlapBuffer = new Collider2D[64];

    // ==================== 水晶参数 ====================
    private float crystalExistTimer;
    private bool canExplode;
    private bool canMove;
    private float moveSpeed;
    private bool canGrow;
    private float growSpeed = 5;
    private Transform closestTarget;
    [SerializeField] private LayerMask whatIsEnemy;

    // ==================== 初始化 ====================
    private void Awake()
    {
        anim = GetComponent<Animator>();
        cd = GetComponent<CircleCollider2D>();
    }

    // ==================== 初始化 ====================
    // 设置水晶持续时间、爆炸、追踪等参数
    public void SetupCrystal(float _crystalDuration, bool _canExplode, bool _canMove, float _moveSpeed, Transform _closestTarget, Player _player)
    {
        crystalExistTimer = _crystalDuration;
        canExplode = _canExplode;
        canMove = _canMove;
        closestTarget = _closestTarget;
        player = _player;
        moveSpeed = _moveSpeed;
    }

    // 从黑洞范围内随机选择敌人作为目标（NonAlloc 复用缓冲）
    public void ChooseRandomEnemy()
    {
        float radius = SkillManager.instance.blackhole.GetBlackholeRadius();
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, overlapBuffer, whatIsEnemy);

        if (hitCount > 0)
            closestTarget = overlapBuffer[Random.Range(0, hitCount)].transform;
    }

    // ==================== 生命周期 ====================
    // 水晶倒计时、追踪敌人、变大
    private void Update()
    {
        crystalExistTimer -= Time.deltaTime;
        if (crystalExistTimer < 0)
        {
            FinishCrystal();
        }

        // 追踪最近敌人
        if (canMove && closestTarget != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, closestTarget.position, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, closestTarget.position) < 1)
            {
                FinishCrystal();
                canMove = false;
            }
        }

        // 爆炸前变大
        if (canGrow)
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(3, 3), growSpeed * Time.deltaTime);
    }

    // ==================== 爆炸事件 ====================
    // 动画触发爆炸时对范围内敌人造成伤害（NonAlloc 复用缓冲）
    private void AnimationExplodeEvent()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, cd.radius, overlapBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Entity>().SetupKnockbackDir(transform);
                player.stats.DoDamage(hit.GetComponent<CharacterStats>());

                // 应用护符效果
                ItemData_Equipment equipedAmulet = Inventory.instance.GetEquipment(EquipmentType.Amulet);
                if (equipedAmulet != null)
                    equipedAmulet.Effect(hit.transform);
            }
        }
    }

    // ==================== 水晶结束 ====================
    // 触发爆炸或直接销毁
    public void FinishCrystal()
    {
        if (canExplode)
        {
            canGrow = true;
            anim.SetTrigger("Explode");
        }
        else
            selfDestroy();
    }

    public void selfDestroy() => Destroy(gameObject);
}
