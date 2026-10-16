using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity（实体基类）- 所有游戏实体的父类（玩家、敌人）
/// 提供通用功能：移动、翻转、击退、地面/墙壁检测
/// 设计思路：将玩家和敌人的共同行为抽象到基类，避免代码重复
/// 使用场景：所有需要移动、受击、翻转的游戏角色
/// </summary>
public class Entity : MonoBehaviour
{
    // ==================== 组件引用区域 ====================
    // 这里集中声明所有核心组件的自动引用属性
    // 设计思路：通过属性封装，确保外部只能读取不能随意修改，保证数据安全
    #region Components
    public Animator anim { get; private set; }      // 动画控制器：控制角色动画状态机
    public Rigidbody2D rb { get; private set; }     // 2D刚体：处理物理移动和受力
    public SpriteRenderer sr { get; private set; }  // 精灵渲染器：控制角色图像显示和翻转
    public CharacterStats stats { get; private set; } // 角色属性：管理血量、攻击力等数值
    public CapsuleCollider2D cd { get; private set; } // 碰撞体：处理物理碰撞检测
    #endregion

    // ==================== 击退系统参数 ====================
    // 控制角色受击后的击退效果：力度、持续时间、状态锁定
    // 使用场景：被敌人攻击、触发陷阱时产生硬直和位移
    [Header("击退参数")]
    [SerializeField] protected Vector2 knockbackPower;   // 击退力度（X=水平击退距离，Y=垂直击飞高度）
    [SerializeField] protected float knockbackDurtion;   // 击退持续时间：角色被击退后无法控制的时长
    protected bool isKnocked;                            // 是否正在被击退：用于锁定移动输入

    // ==================== 环境检测参数 ====================
    // 通过射线检测实现地面判定和墙壁判定，支持平台跳跃和碰撞阻挡
    // 设计思路：使用Transform作为射线起点，便于在编辑器中可视化调整位置
    [Header("检测参数")]
    public Transform attackCheck;                        // 攻击检测点：攻击判定的中心位置
    public float attackCheckRadius;                      // 攻击检测半径：攻击范围的大小
    [SerializeField] protected Transform groundCheck;    // 地面检测点：射线检测的起点，判断是否着地
    [SerializeField] protected float groundCheckDistance; // 地面检测距离：射线向下发射的长度
    [SerializeField] protected Transform wallCheck;      // 墙壁检测点：射线检测的起点，判断前方是否有墙
    [SerializeField] protected float wallCheckDistance;  // 墙壁检测距离：射线向前发射的长度
    [SerializeField] protected LayerMask isGround;       // 地面层掩码：用于过滤射线检测的层级

    // ==================== 方向与状态变量 ====================
    // 管理角色的朝向和击退方向，支持自动翻转和事件通知
    // facingDir使用1/-1表示方向，便于与速度计算直接相乘
    public int konkcbackDir { get; private set; }       // 击退方向（1=向右击退，-1=向左击退）
    public int facingDir { get; set; } = 1;              // 朝向方向（1=朝右，-1=朝左）
    private bool facingRight = true;                     // 是否朝右：用于判断是否需要翻转
    public System.Action onFlipped;                      // 翻转回调事件：翻转时通知订阅者（如血条UI）

    // ==================== 生命周期方法 ====================
    // Awake在对象实例化时调用，Start在第一帧更新前调用，Update每帧调用
    // 使用虚方法设计，允许子类（Player、Enemy）重写以添加自定义逻辑

    /// <summary>
    /// 初始化虚方法：子类可重写以添加自定义初始化逻辑
    /// </summary>
    protected virtual void Awake() { }

    /// <summary>
    /// 启动时获取所有必要组件引用
    /// 设计思路：使用GetComponent系列方法自动获取组件，减少Inspector手动拖拽
    /// </summary>
    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cd = GetComponent<CapsuleCollider2D>();
    }

    /// <summary>
    /// 每帧更新虚方法：子类重写实现具体逻辑
    /// </summary>
    protected virtual void Update() { }

    // ==================== 速度控制区域 ====================
    // 提供减速和恢复速度的接口，用于技能效果（如冰冻陷阱、减速光环）
    // 子类（Player、Enemy）可重写以实现不同的减速恢复逻辑

    /// <summary>
    /// 减速效果：被技能或陷阱减速时调用
    /// </summary>
    /// <param name="_slowPercentage">减速百分比（0-1）</param>
    /// <param name="_slowDuration">减速持续时间（秒）</param>
    public virtual void SlowEntityBy(float _slowPercentage, float _slowDuration) { }

    /// <summary>
    /// 恢复默认速度：击退或减速效果结束后调用
    /// </summary>
    protected virtual void ReturnDefaultSpeed()
    {
        anim.speed = 1; // 恢复动画播放速度为正常
    }

    // ==================== 击退系统 ====================
    // 受击时触发击退效果：计算方向 → 施加速度 → 等待 → 恢复
    // 击退期间锁定移动输入（isKnocked标志），防止玩家/敌人抵消击退效果

    /// <summary>
    /// 受到伤害冲击：启动击退协程
    /// </summary>
    public virtual void DamageImpact() => StartCoroutine("HitKnockback");

    // ==================== 碰撞检测区域 ====================
    // 使用Physics2D.Raycast进行地面和墙壁检测，支持平台跳跃和边缘阻挡
    // OnDrawGizmos在Scene视图中绘制辅助线，方便调试时查看检测范围
    #region Collision
    /// <summary>
    /// 检测是否在地面上：从groundCheck点向下发射射线
    /// 使用场景：判断角色是否可以跳跃、是否处于地面状态
    /// </summary>
    /// <returns>是否接触地面</returns>
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, isGround);

    /// <summary>
    /// 检测是否碰到墙壁：从wallCheck点向朝向方向发射射线
    /// 使用场景：阻止角色穿墙、判断是否需要翻转方向
    /// </summary>
    /// <returns>是否接触墙壁</returns>
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir, wallCheckDistance, isGround);

    /// <summary>
    /// 在Scene视图中绘制检测辅助线（仅编辑器可见）
    /// 设计思路：便于调试时可视化检测范围
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * facingDir, wallCheck.position.y));
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion

    // ==================== 击退方向计算 ====================
    // 根据伤害来源的位置判断击退方向：实现"从哪边来就往哪边退"的物理效果
    // 击退方向用1/-1表示，与knockbackPower相乘得到最终速度方向

    /// <summary>
    /// 根据伤害来源位置计算击退方向
    /// 设计思路：伤害来自左侧则向右击退，来自右侧则向左击退（符合物理直觉）
    /// </summary>
    /// <param name="_damageDirection">伤害来源的Transform</param>
    public virtual void SetupKnockbackDir(Transform _damageDirection)
    {
        if (_damageDirection.position.x > transform.position.x)
            konkcbackDir = -1; // 伤害来自右侧，向左击退
        else if (_damageDirection.position.x < transform.position.x)
            konkcbackDir = 1;  // 伤害来自左侧，向右击退
    }

    /// <summary>
    /// 设置击退力度
    /// </summary>
    public void SetKnockbackPower(Vector2 _knockbackPower) => knockbackPower = _knockbackPower;

    // ==================== 击退执行协程 ====================
    // 协程流程：锁定移动 → 施加击退速度 → 等待 → 解锁移动 → 重置力度
    // 使用协程而非动画事件，确保击退时间精确可控，不受动画播放影响

    /// <summary>
    /// 执行击退效果协程：施加击退速度并锁定移动
    /// </summary>
    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;
        if (knockbackPower.x > 0 || knockbackPower.y > 0)
            rb.linearVelocity = new Vector2(knockbackPower.x * konkcbackDir, knockbackPower.y);

        yield return new WaitForSeconds(knockbackDurtion); // 等待击退持续时间结束
        isKnocked = false;
        SetupZeroKnockbackPower();
    }

    /// <summary>
    /// 重置击退力度：子类可重写实现自定义逻辑
    /// </summary>
    protected virtual void SetupZeroKnockbackPower() { }

    // ==================== 速度控制接口 ====================
    // 提供SetZeroVelocity和SetVelocity两个接口，状态机通过它们控制角色移动
    // 被击退时（isKnocked=true）拒绝执行，防止击退效果被覆盖
    #region Velocity
    /// <summary>
    /// 停止移动：将速度设为0（被击退时无效，防止打断击退效果）
    /// </summary>
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.linearVelocity = new Vector2(0, 0);
    }

    /// <summary>
    /// 设置移动速度，并根据速度方向自动翻转角色朝向
    /// 设计思路：将速度设置和朝向翻转绑定，简化状态机代码
    /// </summary>
    /// <param name="_xVelocity">X轴速度</param>
    /// <param name="_yVelocity">Y轴速度</param>
    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.linearVelocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }
    #endregion

    // ==================== 角色翻转系统 ====================
    // 通过旋转Transform实现朝向切换，同时更新facingDir和facingRight保持状态一致
    // 提供FlipController自动判断是否需要翻转，简化状态机调用
    #region Flip
    /// <summary>
    /// 翻转角色朝向：旋转180度并更新朝向标记
    /// 使用场景：角色改变移动方向时
    /// </summary>
    public virtual void Flip()
    {
        facingDir = facingDir * -1;   // 反转朝向方向
        facingRight = !facingRight;   // 更新朝向状态标记
        transform.Rotate(0, 180, 0);  // 旋转角色180度
        if (onFlipped != null)
            onFlipped();              // 触发翻转事件，通知血条等UI同步翻转
    }

    /// <summary>
    /// 根据速度方向自动翻转：向右移动则朝右，向左移动则朝左
    /// </summary>
    /// <param name="_x">X轴速度</param>
    public void FlipController(float _x)
    {
        if (_x > 0 && !facingRight || _x < 0 && facingRight)
            Flip();
    }
    #endregion

    /// <summary>
    /// 设置默认朝向：用于初始化时设置角色朝向
    /// </summary>
    /// <param name="_direction">朝向方向（1=右，-1=左）</param>
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

    /// <summary>
    /// 死亡虚方法：子类重写实现具体死亡逻辑
    /// </summary>
    public virtual void Die() { }
}
