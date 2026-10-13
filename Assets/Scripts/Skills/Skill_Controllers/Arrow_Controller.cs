using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arrow_Controller（箭矢控制器）- 控制箭矢飞行、碰撞和伤害
/// 功能：箭矢发射、目标层伤害、射中后停止、翻转箭矢方向
/// </summary>
public class Arrow_Controller : MonoBehaviour
{
    // ==================== 箭矢参数 ====================
    [SerializeField] private int damage;
    [SerializeField] private string targetLayerName = "Player";
    [SerializeField] private float xVelocity;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool canMove;
    [SerializeField] private bool flipped;
    private CharacterStats myStats;

    // ==================== 生命周期 ====================
    // 控制箭矢飞行
    private void Update()
    {
        if (canMove)
            rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    // ==================== 初始化 ====================
    // 设置箭矢速度和施法者属性
    public void SetUpArrow(float _speed, CharacterStats _myStats)
    {
        xVelocity = _speed;
        myStats = _myStats;
    }

    // ==================== 碰撞检测 ====================
    // 检测目标层或地面，造成伤害并停止箭矢
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(targetLayerName))
        {
            collision.GetComponent<CharacterStats>().TakeDamage(damage);
            StuckInto(collision);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StuckInto(collision);
        }
    }

    // ==================== 射中处理 ====================
    // 停止粒子、禁用碰撞、冻结刚体、跟随目标，延迟销毁
    private void StuckInto(Collider2D collision)
    {
        GetComponentInChildren<ParticleSystem>().Stop();
        GetComponent<Collider2D>().enabled = false;
        canMove = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent = collision.transform;
        Destroy(gameObject, Random.Range(5, 7));
    }

    // ==================== 翻转箭矢 ====================
    // 反转飞行方向和目标层（用于反射箭矢）
    public void FlipArrow()
    {
        if (flipped)
            return;

        xVelocity = xVelocity * -1;
        flipped = true;
        transform.Rotate(0, 180, 0);
        targetLayerName = "Enemy";
    }
}
