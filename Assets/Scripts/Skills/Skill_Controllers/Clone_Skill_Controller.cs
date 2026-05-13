using UnityEngine;

//P65克隆控制
//2024年11月20日加入到技能树中
public class Clone_Skill_Controller : MonoBehaviour
{
    private Player player;
    private SpriteRenderer sr;
    private Animator anim;
    [SerializeField] private float cloneLosingSpeed;//加速消失时间

    private float cloneTimer;
    private float attackMulitiplier;
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = .8f;
    private Transform closestEnemy;
    private int facingDir = 1;
    private bool canDuplicateClone;
    private float chanceToDuplicate;



    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }


    private void Update()
    {
        cloneTimer -= Time.deltaTime;

        if (cloneTimer < 0)
        {
            sr.color = new Color(1, 1, 1, sr.color.a - (Time.deltaTime * cloneLosingSpeed));//逐渐消失

            if (sr.color.a < 0)
                Destroy(gameObject);//当透明度小于等于0时销毁克隆体
        }
    }


    public void SetupClone(Transform _newTransform, float _cloneDuration, bool _canAttack, Vector3 _offset, Transform _closetEnemy, bool _canDuplicate, float _chanceToDuplicate, Player _player, float _attackMulitiplier)
    {
        if (_canAttack)
        {
            anim.SetInteger("AttackNumber", Random.Range(1, 3));//随机clone攻击的动画
        }

        attackMulitiplier = _attackMulitiplier;
        player = _player;
        transform.position = _newTransform.position + _offset;//将克隆体的位置设置为传入的位置

        cloneTimer = _cloneDuration;//设置克隆体的持续时间

        closestEnemy = _closetEnemy;
        canDuplicateClone = _canDuplicate;
        chanceToDuplicate = _chanceToDuplicate;
        FaceClosestTarget();
    }


    private void AnimationTrigger()
    {
        cloneTimer = -.1f;
    }

    private void AttackTrigger()//攻击碰撞触发函数
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheck.position, attackCheckRadius);//这行代码在 Unity 中用于检测一个圆形区域内的所有碰撞体，并返回它们的数组。

        foreach (var hit in colliders)//for循环遍历碰撞体数组
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                //player.stats.DoDamage(hit.GetComponent<CharacterStats>());
                hit.GetComponent<Entity>().SetupKnockbackDir(transform);

                PlayerStats playerStats = player.GetComponent<PlayerStats>();//获取玩家的stats
                EnemyStats enemyStats = hit.GetComponent<EnemyStats>(); //获取敌人的stats

                playerStats.CloneDoDamage(enemyStats, attackMulitiplier);//对敌人造成伤害

                if (player.skill.clone.canApplyOnHitEffect)//如果可以应用击中效果
                {
                    ItemData_Equipment weaponData = Inventory.instance.GetEquipment(EquipmentType.Weapon);

                    if (weaponData != null)//应用武器特殊效果
                        weaponData.Effect(hit.transform);
                }

                if (canDuplicateClone)//如果可以多重影分身
                {
                    if (Random.Range(0, 100) <= chanceToDuplicate)
                    {
                        SkillManager.instance.clone.CreateClone(hit.transform, new Vector3(.5f * facingDir, 0));//在敌人身上创建克隆
                    }
                }
            }
        }
    }

    private void FaceClosestTarget()//面对最近的目标函数
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