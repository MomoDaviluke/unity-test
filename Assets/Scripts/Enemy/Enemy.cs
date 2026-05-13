using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EntityFX))]
[RequireComponent(typeof(ItemDrop))]
public class Enemy : Entity
{
    [SerializeField] protected LayerMask isPlayer;

    [Header("眩晕")]
    public float stunDuration = 1;
    public Vector2 stunDirection = new Vector2(10, 12);
    protected bool canBeStunned;
    [SerializeField] protected GameObject counterImage;

    [Header("移动")]
    public float moveSpeed = 1.5f;
    public float chaseSpeed = 3;    // 追击速度
    public float idleTime = 2;
    public float battleTime = 7;
    private float defaultMoveSpeed;

    [Header("攻击")]
    public float attackDistance = 2;
    public float attackCooldown = 2;
    public float minAttackCooldown = 1;
    public float maxAttackCooldown = 2;
    public float agroDistance = 3;
    [HideInInspector] public float lastTimeAttacked;

    public EnemyStateMachine stateMachine { get; private set; }
    public EntityFX fX { get; private set; }
    public string lastAnimBoolName { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
        defaultMoveSpeed = moveSpeed;
    }
    protected override void Start()
    {
        base.Start();
        fX = GetComponent<EntityFX>();
        
        // 隐藏反击图片
        counterImage?.SetActive(false);
    }
    protected override void Update()
    {
        base.Update();
        
        if (stateMachine.currentState != null)
        {
            stateMachine.currentState.Update();
        }
    }
    public virtual void AssignLastAnimName(string _animBoolName) => lastAnimBoolName = _animBoolName;
    public override void SlowEntityBy(float _slowPercentage, float _slowDuration)
    {
        moveSpeed = moveSpeed * (1 - _slowPercentage);
        chaseSpeed = chaseSpeed * (1 - _slowPercentage);
        anim.speed = anim.speed * (1 - _slowPercentage);

        Invoke("ReturnDefaultSpeed", _slowDuration);
    }
    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();
        moveSpeed = defaultMoveSpeed;
    }
    public virtual void FreezeTime(bool _timeFrozen)
    {
        if (_timeFrozen)
        {
            moveSpeed = 0;
            anim.speed = 0;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1;
        }
    }
    public virtual void FreezeTimeFor(float _duration) => StartCoroutine(FreezeTimerCoroutine(_duration));

    protected virtual IEnumerator FreezeTimerCoroutine(float _seconds)
    {
        FreezeTime(true);

        yield return new WaitForSeconds(_seconds);

        FreezeTime(false);
    }

    #region Counter Attack Window

    public virtual void OpenCountAttackWindow()
    {
        canBeStunned = true;
        // 已禁用CounterImage
    }
    public virtual void CloseCountAttackWindow()
    {
        canBeStunned = false;
        // 已禁用CounterImage
    }
    #endregion
    public virtual bool CanbeStunned()
    {
        if (canBeStunned)
        {
            CloseCountAttackWindow();
            return true;
        }
        return false;
    }

    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    public virtual void AnimationSepcoalAttackTrigger()
    {

    }

    // 检测玩家：从脚底位置发出射线，避免被自己的碰撞体阻挡
    public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(groundCheck.position, Vector2.right * facingDir, 50, isPlayer);

    protected virtual void DamagePlayer()
    {
        Player player = PlayerManager.instance.player;
        if (player != null)
        {
            player.lastDamageSource = transform; // 设置伤害来源
            Debug.Log($"Set player.lastDamageSource to {transform.name}");

            // 对玩家造成伤害
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(CalculateTotalDamage());
            }
        }
    }

    private int CalculateTotalDamage()
    {
        EnemyStats stats = GetComponent<EnemyStats>();
        if (stats == null) return 0;

        // 基础伤害 + 魔法伤害
        return stats.damage.GetValue() + stats.fireDamage.GetValue() +
               stats.iceDamage.GetValue() + stats.lightningDamage.GetValue();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
    }
}
