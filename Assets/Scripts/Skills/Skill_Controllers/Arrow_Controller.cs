using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Controller : MonoBehaviour
{
    [SerializeField] private int damage;//箭矢造成的伤害值
    [SerializeField] private string targetLayerName = "Player";//箭矢的目标层

    [SerializeField] private float xVelocity;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private bool canMove;
    [SerializeField] private bool flipped;

    private CharacterStats myStats;

    private void Update()
    {
        if (canMove)
            rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }

    public void SetUpArrow(float _speed, CharacterStats _myStats)
    {
        xVelocity = _speed;

        myStats = _myStats;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(targetLayerName))
        {

            collision.GetComponent<CharacterStats>().TakeDamage(damage);//箭的伤害

            // myStats.DoDamage(collision.GetComponent<CharacterStats>());//弓箭手的额外属性

            StuckInto(collision);

        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StuckInto(collision);
        }

    }


    private void StuckInto(Collider2D collision)//射中目标
    {
        GetComponentInChildren<ParticleSystem>().Stop();//停止粒子系统
        GetComponent<Collider2D>().enabled = false;
        canMove = false;
        rb.isKinematic = true;//刚体为运动
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.parent = collision.transform;//箭矢的父物体为碰撞物体

        Destroy(gameObject, Random.Range(5, 7));
    }


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