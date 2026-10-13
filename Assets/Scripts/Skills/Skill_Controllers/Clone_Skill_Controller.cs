using UnityEngine;

/// <summary>
/// Clone_Skill_Controller（分身技能控制器）- 控制分身的行为和生命周期
/// 功能：分身创建、攻击、逐渐消失、面向最近敌人
/// </summary>
public class Clone_Skill_Controller : MonoBehaviour
{
    // ==================== 核心引用 ====================
    private Player player;
    private SpriteRenderer sr;
    private Animator anim;

    // ==================== 分身参数 ====================
    [SerializeField] private float cloneLosingSpeed; // 消失速度
    private float cloneTimer;
    private float attackMulitiplier;
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = .8f;
    private Transform closestEnemy;
    private int facingDir = 1;
    private bool canDuplicateClone;
    private float chanceToDuplicate;

    // ==================== 初始化 ====================
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // ==================== 生命周期 ====================
    // 分身倒计时结束后逐渐消失
    private void Update()
    {
        cloneTimer -= Time.deltaTime;

        if (cloneTimer < 0)
        {
            // 逐渐消失
            sr.color = new Color(1, 1, 1, sr.color.a - (Time.deltaTime * cloneLosingSpeed));
            if (sr.color.a < 0)
                Destroy(gameObject);
        }
    }

    // ==================== 分身设置 ====================
    // 初始化分身的位置、攻击、持续时间等参数
    public void SetupClone(Transform _newTransform, float _cloneDuration, bool _canAttack, Vector3 _offset, Transform _closetEnemy, bool _canDuplicate, float _chanceToDuplicate, Player _player, float _attackMulitiplier)
    {
        if (_canAttack)
        {
            anim.SetInteger("AttackNumber", Random.Range(1, 3)); // 随机攻击动画
        }

        attackMulitiplier = _attackMulitiplier;
        player = _player;
        transform.position = _newTransform.position + _offset;
        cloneTimer = _cloneDuration;

        closestEnemy = _closetEnemy;
        canDuplicateClone = _canDuplicate;
        chanceToDuplicate = _chanceToDuplicate;
        FaceClosestTarget();
    }

    // ==================== 动画事件 ====================
    // 动画结束时触发，让分身开始消失
    private void AnimationTrigger()
    {
        cloneTimer = -.1f;
    }

    // 攻击动画触发时检测范围内敌人并造成伤害
    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                hit.GetComponent<Entity>().SetupKnockbackDir(transform);

                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                EnemyStats enemyStats = hit.GetComponent<EnemyStats>();
                playerStats.CloneDoDamage(enemyStats, attackMulitiplier);

                // 应用武器击中效果
                if (player.skill.clone.canApplyOnHitEffect)
                {
                    ItemData_Equipment weaponData = Inventory.instance.GetEquipment(EquipmentType.Weapon);
                    if (weaponData != null)
                        weaponData.Effect(hit.transform);
                }

                // 多重影分身：概率在敌人位置创建新分身
                if (canDuplicateClone)
                {
                    if (Random.Range(0, 100) <= chanceToDuplicate)
                    {
                        SkillManager.instance.clone.CreateClone(hit.transform, new Vector3(.5f * facingDir, 0));
                    }
                }
            }
        }
    }

    // ==================== 朝向目标 ====================
    // 让分身面向最近的敌人
    private void FaceClosestTarget()
    {
        if (closestEnemy != null)
        {
            if (transform.position.x > closestEnemy.position.x)
            {
                facingDir = -1;
                transform.Rotate(0, 180, 0);
            }
        }
    }
}