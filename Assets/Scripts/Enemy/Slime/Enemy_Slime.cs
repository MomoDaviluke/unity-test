// Enemy_Slime.cs
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

// 史莱姆类型枚举
public enum SlimeType { big, Medium, small }

public class Enemy_Slime : Enemy
{
    [Header("Slime spesific")]
    [SerializeField] private SlimeType slimeType; // 史莱姆类型
    [SerializeField] private int slimesToCreate; // 死亡时创建的小史莱姆数量
    [SerializeField] private GameObject slimePrefab; // 小史莱姆预制体
    [SerializeField] private Vector2 minCreationVelocity; // 创建史莱姆的最小速度
    [SerializeField] private Vector2 maxCreationVelocity; // 创建史莱姆的最大速度

    #region States
    // 各种状态引用
    public SlimeIdleState idleState { get; private set; }
    public SlimeMoveState moveState { get; private set; }
    public SlimeBattleState battleState { get; private set; }
    public SlimeAttackState attackState { get; private set; }
    public SlimeStunState stunnedState { get; private set; }
    public SlimeDeathState deadState { get; private set; }
    #endregion

    // 初始化
    protected override void Awake()
    {
        base.Awake();
        facingDir = 1; // 设置初始朝向为右

        // 初始化所有状态
        idleState = new SlimeIdleState(this, stateMachine, "Idle", this);
        moveState = new SlimeMoveState(this, stateMachine, "Move", this);
        battleState = new SlimeBattleState(this, stateMachine, "Move", this);
        attackState = new SlimeAttackState(this, stateMachine, "Attack", this);
        stunnedState = new SlimeStunState(this, stateMachine, "Stun", this);
        deadState = new SlimeDeathState(this, stateMachine, "Dead", this);
    }

    // 开始时设置初始状态
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState); // 初始状态为空闲
    }

    // 更新
    protected override void Update()
    {
        base.Update();
        // 测试代码：按U键进入眩晕状态
        if (Input.GetKeyDown(KeyCode.U))
        {
            stateMachine.ChangeState(stunnedState);
        }
    }

    // 是否可以被眩晕（重写基类方法）
    public override bool CanbeStunned()
    {
        if (base.CanbeStunned())
        {
            stateMachine.ChangeState(stunnedState);
            return true;
        }
        return false;
    }

    // 死亡处理（重写基类方法）
    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deadState); // 切换到死亡状态

        // 如果不是小史莱姆，则分裂出其他的史莱姆
        if (slimeType == SlimeType.small)
            return;
        CreateSlimes(slimesToCreate, slimePrefab);
    }

    // 创建小史莱姆的方法
    private void CreateSlimes(int _amountOfSlimes, GameObject _slimePrefab)
    {
        for (int i = 0; i < _amountOfSlimes; i++)
        {
            // 实例化新史莱姆并设置初始属性
            GameObject newSlime = Instantiate(_slimePrefab, transform.position, Quaternion.identity);
            newSlime.GetComponent<Enemy_Slime>().SetupSlime(facingDir);
        }
    }

    // 设置新生成史莱姆的属性
    public void SetupSlime(int _facingDir)
    {
        // 如果朝向不同则翻转
        if (_facingDir != facingDir)
            Flip();

        // 随机设置初始速度
        float xVelocity = Random.Range(minCreationVelocity.x, maxCreationVelocity.x);
        float yVelocity = Random.Range(minCreationVelocity.y, maxCreationVelocity.y);

        // 设置击退状态和速度
        isKnocked = true;
        GetComponent<Rigidbody2D>().velocity = new Vector2(xVelocity * -facingDir, yVelocity);

        // 延时取消击退状态
        Invoke("CancelKnockback", 1.5f);
    }

    // 取消击退状态
    private void CancelKnockback() => isKnocked = false;
}