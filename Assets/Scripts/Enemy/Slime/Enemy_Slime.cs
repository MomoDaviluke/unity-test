// Enemy_Slime.cs
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

// 史莱姆类型枚举
public enum SlimeType { big, Medium, small }

public class Enemy_Slime : Enemy
{
    // ==================== 史莱姆专属参数 ====================
    // slimeType: 史莱姆类型（大/中/小），决定死亡时是否分裂
    // slimesToCreate: 死亡时分裂生成的小史莱姆数量
    // slimePrefab: 小史莱姆预制体
    // min/maxCreationVelocity: 分裂时的随机速度范围
    [Header("Slime spesific")]
    [SerializeField] private SlimeType slimeType; // 史莱姆类型
    [SerializeField] private int slimesToCreate; // 死亡时创建的小史莱姆数量
    [SerializeField] private GameObject slimePrefab; // 小史莱姆预制体
    [SerializeField] private Vector2 minCreationVelocity; // 创建史莱姆的最小速度
    [SerializeField] private Vector2 maxCreationVelocity; // 创建史莱姆的最大速度

    // ==================== 状态机声明 ====================
    // Slime拥有6个状态：待机、移动、战斗、攻击、眩晕、死亡
    #region States
    public SlimeIdleState idleState { get; private set; }
    public SlimeMoveState moveState { get; private set; }
    public SlimeBattleState battleState { get; private set; }
    public SlimeAttackState attackState { get; private set; }
    public SlimeStunState stunnedState { get; private set; }
    public SlimeDeathState deadState { get; private set; }
    #endregion

    // ==================== 生命周期 ====================
    // Awake中创建所有状态，设置初始面朝方向为右（1）
    // Start时初始化状态机为待机状态

    protected override void Awake()
    {
        base.Awake();
        facingDir = 1; // 设置初始朝向为右

        idleState = new SlimeIdleState(this, stateMachine, "Idle", this);
        moveState = new SlimeMoveState(this, stateMachine, "Move", this);
        battleState = new SlimeBattleState(this, stateMachine, "Move", this);
        attackState = new SlimeAttackState(this, stateMachine, "Attack", this);
        stunnedState = new SlimeStunState(this, stateMachine, "Stun", this);
        deadState = new SlimeDeathState(this, stateMachine, "Dead", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    // ==================== 调试功能 ====================
    // 按U键强制眩晕（调试用）
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.U))
        {
            stateMachine.ChangeState(stunnedState);
        }
    }

    // ==================== 状态切换接口 ====================
    public override bool CanbeStunned()
    {
        if (base.CanbeStunned())
        {
            stateMachine.ChangeState(stunnedState);
            return true;
        }
        return false;
    }

    // ==================== 死亡处理：分裂机制 ====================
    // 死亡时切换到死亡状态
    // 如果不是最小的史莱姆，则分裂生成多个小史莱姆
    // 小史莱姆继承当前面朝方向并带有随机击退速度
    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deadState);

        // 小史莱姆不再分裂
        if (slimeType == SlimeType.small)
            return;
        CreateSlimes(slimesToCreate, slimePrefab);
    }

    // ==================== 分裂生成 ====================
    // 循环生成指定数量的小史莱姆，每个都调用SetupSlime初始化
    private void CreateSlimes(int _amountOfSlimes, GameObject _slimePrefab)
    {
        for (int i = 0; i < _amountOfSlimes; i++)
        {
            GameObject newSlime = Instantiate(_slimePrefab, transform.position, Quaternion.identity);
            newSlime.GetComponent<Enemy_Slime>().SetupSlime(facingDir);
        }
    }

    // ==================== 新史莱姆初始化 ====================
    // 设置新生成史莱姆的朝向和初始击退速度
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