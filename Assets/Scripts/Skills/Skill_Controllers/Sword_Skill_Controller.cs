using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sword_Skill_Controller（飞剑技能控制器）- 控制飞剑的行为
/// 功能：飞剑发射、反弹、穿透、旋转、返回、伤害计算
/// </summary>
public class Sword_Skill_Controller : MonoBehaviour
{
    // ==================== 核心引用 ====================
    private Animator anim;
    private Rigidbody2D rb;
    private CircleCollider2D cd;
    private Player player;

    // ==================== 基础状态 ====================
    private bool canRotate = true;
    private bool isReturning;
    private float freezeTimeDuration;
    private float returnSpeed;

    // ==================== 穿透参数 ====================
    private float pierceAmount;

    // ==================== 反弹参数 ====================
    private float bounceSpeed;
    private bool isBouncing;
    private int bounceAmount;
    private List<Transform> enemyTarget;
    private int targetIndex;

    // ==================== 旋转参数 ====================
    private float maxTravelDistance;
    private float spinDuration;
    private float spinTimer;
    private bool wasStopped;
    private bool isSpinning;
    private float hitTimer;
    private float hitCooldown;
    private float spinDirection;

    // ==================== 初始化 ====================
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CircleCollider2D>();
    }

    private void DestroyMe()
    {
        Destroy(gameObject);
    }

    // 设置飞剑基础参数
    public void SetupSword(Vector2 _dir, float _gravityScale, Player _player, float _freezeTimeDuration, float _returnSpeed)
    {
        player = _player;
        freezeTimeDuration = _freezeTimeDuration;
        rb.velocity = _dir;
        rb.gravityScale = _gravityScale;
        returnSpeed = _returnSpeed;

        if (pierceAmount <= 0)
            anim.SetBool("Rotation", true);

        Invoke("DestroyMe", 8f);
        spinDirection = Mathf.Clamp(rb.velocity.x, -1, 1);
    }

    // 设置反弹参数
    public void SetupBounce(bool _isBouncing, int _amountOfBounces, float _bounceSpeed)
    {
        isBouncing = _isBouncing;
        bounceAmount = _amountOfBounces;
        bounceSpeed = _bounceSpeed;
        enemyTarget = new List<Transform>();
    }

    // 设置穿透参数
    public void SetupPierce(int _pierceAmount)
    {
        pierceAmount = _pierceAmount;
    }

    // 设置旋转参数
    public void SetupSpin(bool _isSpinning, float _maxTravelDistance, float _spinDuration, float _hitCooldown)
    {
        isSpinning = _isSpinning;
        maxTravelDistance = _maxTravelDistance;
        spinDuration = _spinDuration;
        hitCooldown = _hitCooldown;
    }

    // 召回飞剑
    public void ReturnSword()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent = null;
        isReturning = true;
    }

    // ==================== 生命周期 ====================
    private void Update()
    {
        if (canRotate)
            transform.right = rb.velocity;

        // 飞剑返回玩家
        if (isReturning)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, returnSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, player.transform.position) < 1)
                player.CatchTheSword();
        }

        BounceLogic();
        SpinLogic();
    }

    // ==================== 旋转逻辑 ====================
    private void SpinLogic()
    {
        if (isSpinning)
        {
            // 到达最大距离后停止
            if (Vector2.Distance(player.transform.position, transform.position) > maxTravelDistance && !wasStopped)
            {
                StopWhenSpinning();
            }

            // 停止旋转后持续伤害
            if (wasStopped)
            {
                spinTimer -= Time.deltaTime;
                if (spinTimer < 0)
                {
                    isReturning = true;
                    isSpinning = true;
                }

                hitTimer -= Time.deltaTime;
                if (hitTimer < 0)
                {
                    hitTimer = hitCooldown;
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1);
                    foreach (var hit in colliders)
                    {
                        if (hit.GetComponent<Enemy>() != null)
                            SwordSkillDamage(hit.GetComponent<Enemy>());
                    }
                }
            }
        }
    }

    private void StopWhenSpinning()
    {
        wasStopped = true;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        spinTimer = spinDuration;
    }

    // ==================== 反弹逻辑 ====================
    private void BounceLogic()
    {
        if (isBouncing && enemyTarget.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemyTarget[targetIndex].position, bounceSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, enemyTarget[targetIndex].position) < .1f)
            {
                SwordSkillDamage(enemyTarget[targetIndex].GetComponent<Enemy>());
                enemyTarget[targetIndex].GetComponent<Enemy>().DamageImpact();
                enemyTarget[targetIndex].GetComponent<Enemy>().StartCoroutine("FreezeTimeFor", freezeTimeDuration);

                targetIndex++;
                bounceAmount--;

                if (bounceAmount <= 0)
                {
                    isBouncing = false;
                    isReturning = true;
                }
                if (targetIndex >= enemyTarget.Count)
                {
                    targetIndex = 0;
                }
            }
        }
    }

    // ==================== 碰撞检测 ====================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReturning)
            return;

        if (collision.GetComponent<Enemy>() != null)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            SwordSkillDamage(enemy);
        }

        SetupTargetsForBounce(collision);
        StuckInto(collision);
    }

    // 对敌人造成伤害并应用被动效果
    private void SwordSkillDamage(Enemy enemy)
    {
        EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
        player.stats.DoDamage(enemy.GetComponent<CharacterStats>());

        if (player.skill.sword.timeStopUnlocked)
            enemy.FreezeTimeFor(freezeTimeDuration);

        if (player.skill.sword.volnurableUnlocked)
            enemyStats.MakeVulnerableFor(freezeTimeDuration);

        ItemData_Equipment equipedAmulet = Inventory.instance.GetEquipment(EquipmentType.Amulet);
        if (equipedAmulet != null)
            equipedAmulet.Effect(enemy.transform);
    }

    // 反弹模式下收集范围内敌人
    private void SetupTargetsForBounce(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            if (isBouncing && enemyTarget.Count <= 0)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10);
                foreach (var hit in colliders)
                {
                    if (hit.GetComponent<Enemy>() != null)
                    {
                        enemyTarget.Add(hit.transform);
                    }
                }
            }
        }
    }

    // 飞剑刺入物体（停止运动并跟随目标）
    private void StuckInto(Collider2D collision)
    {
        if (pierceAmount > 0 && collision.GetComponent<Enemy>() != null)
        {
            pierceAmount--;
            return;
        }
        if (isSpinning)
        {
            return;
        }

        canRotate = false;
        cd.enabled = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GetComponentInChildren<ParticleSystem>().Play();

        if (isBouncing && enemyTarget.Count > 0)
            return;

        anim.SetBool("Rotation", false);
        transform.parent = collision.transform;
    }
}
