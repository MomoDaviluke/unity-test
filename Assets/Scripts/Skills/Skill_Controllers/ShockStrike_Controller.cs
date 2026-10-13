using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShockStrike_Controller（闪电打击控制器）- 控制闪电追踪目标并造成伤害
/// 功能：闪电移动、追踪目标、触发伤害和眩晕效果
/// </summary>
public class ShockStrike_Controller : MonoBehaviour
{
    // ==================== 核心参数 ====================
    [SerializeField] private CharacterStats targetStats;
    [SerializeField] private float speed;
    private int damage = 5;

    // ==================== 组件与状态 ====================
    private Animator anim;
    private bool triggered;

    // ==================== 初始化 ====================
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // 设置闪电打击参数
    public void Setup(int _damage, CharacterStats _targetStats)
    {
        damage = _damage;
        targetStats = _targetStats;
    }

    // ==================== 生命周期 ====================
    // 闪电追踪目标，到达后触发伤害
    void Update()
    {
        if (!targetStats)
            return;

        if (triggered)
            return;

        // 向目标移动并调整朝向
        transform.position = Vector2.MoveTowards(transform.position, targetStats.transform.position, speed * Time.deltaTime);
        transform.right = transform.position - targetStats.transform.position;

        // 到达目标位置时触发伤害
        if (Vector2.Distance(transform.position, targetStats.transform.position) < 0.1f)
        {
            anim.transform.localPosition = new Vector3(0, .5f);
            anim.transform.localRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(3, 3);

            Invoke("DamageAndSelfDestroy", 0.2f);
            triggered = true;
            anim.SetTrigger("Hit");
        }
    }

    // ==================== 伤害与销毁 ====================
    // 对目标应用眩晕并造成伤害，然后销毁自身
    private void DamageAndSelfDestroy()
    {
        targetStats.ApplyShock(true);
        targetStats.TakeDamage(damage);
        Destroy(gameObject, 0.4f);
    }
}
