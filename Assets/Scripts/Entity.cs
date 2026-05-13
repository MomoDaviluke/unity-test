using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Entity.cs摘要
// 实体基类，定义了所有实体（玩家、敌人等）共有的属性和方法

public class Entity : MonoBehaviour
{
    #region Components
    // 组件引用
    public Animator anim { get; private set; } // 动画控制器
    public Rigidbody2D rb { get; private set; } // 刚体组件
    public SpriteRenderer sr { get; private set; } // 精灵渲染器
    public CharacterStats stats { get; private set; } // 角色属性
    public CapsuleCollider2D cd { get; private set; } // 胶囊碰撞体
    #endregion

    [Header("Knockback")]
    [SerializeField] protected Vector2 knockbackPower; // 击退力度
    [SerializeField] protected float knockbackDurtion; // 击退持续时间
    protected bool isKnocked; // 是否正在被击退

    [Header("攻击")]
    public Transform attackCheck; // 攻击检测点
    public float attackCheckRadius; // 攻击检测半径
    [SerializeField] protected Transform groundCheck; // 地面检测点
    [SerializeField] protected float groundCheckDistance; // 地面检测距离
    [SerializeField] protected Transform wallCheck; // 墙壁检测点
    [SerializeField] protected float wallCheckDistance; // 墙壁检测距离
    [SerializeField] protected LayerMask isGround; // 地面层

    public int konkcbackDir { get; private set; } // 击退方向
    public int facingDir { get; set; } = 1; // 朝向方向
    private bool facingRight = true; // 是否朝右
    public System.Action onFlipped; // 翻转回调

    // 基本生命周期方法（由子类重写）
    protected virtual void Awake() { }
    protected virtual void Start()
    {
        // 获取组件引用
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cd = GetComponent<CapsuleCollider2D>();
    }
    protected virtual void Update() { }

    // 实体减速方法
    public virtual void SlowEntityBy(float _slowPercentage, float _slowDuration) { }

    // 恢复默认速度
    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1;
    }

    // 伤害冲击反应
    public virtual void DamageImpact() => StartCoroutine("HitKnockback");

    #region Collision
    // 碰撞检测方法
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, isGround);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, isGround);

    // 绘制调试线
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDir, wallCheck.position.y));
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion

    // 设置击退方向
    public virtual void SetupKnockbackDir(Transform _damageDirection)
    {
        if (_damageDirection.position.x > transform.position.x)
        {
            konkcbackDir = -1;
        }
        else if (_damageDirection.position.x < transform.position.x)
        {
            konkcbackDir = 1;
        }
    }

    // 设置击退力度
    public void SetKnockbackPower(Vector2 _knockbackPower) => knockbackPower = _knockbackPower;

    // 击退协程
    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;
        if (knockbackPower.x > 0 || knockbackPower.y > 0)
            rb.velocity = new Vector2(knockbackPower.x * konkcbackDir, knockbackPower.y);

        yield return new WaitForSeconds(knockbackDurtion);
        isKnocked = false;
        SetupZeroKnockbackPower();
    }

    // 重置击退力
    protected virtual void SetupZeroKnockbackPower() { }

    #region Velocity
    // 速度控制方法
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(0, 0);
    }

    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }
    #endregion

    #region Flip
    // 翻转控制方法
    public virtual void Flip()
    {
        facingDir = facingDir * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
        if (onFlipped != null)
            onFlipped();
    }

    // 基于移动方向自动翻转
    public void FlipController(float _x)
    {
        if (_x > 0 && !facingRight || _x < 0 && facingRight)
            Flip();
    }
    #endregion
    public virtual void SetupDefaultFacingDir(int _direction)
    {
        facingDir = _direction;

        if (facingDir == -1)
        {
            facingRight = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            facingRight = true;
            transform.rotation = Quaternion.identity;
        }
    }

    // 死亡方法
    public virtual void Die() { }
}
