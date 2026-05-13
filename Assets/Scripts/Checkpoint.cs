using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Checkpoint : MonoBehaviour
{
    private Animator anim;
    public string id;
    public bool activationStatus;


    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    [ContextMenu("产生检查点ID")]//在编辑器中生成一个按钮
    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString();
    }


    private void OnTriggerEnter2D(Collider2D collision)//检测到碰撞
    {
        if (collision.GetComponent<Player>() != null)//检测到玩家
        {
            ActivateCheckPoint();//激活检查点
        }
    }

    public void ActivateCheckPoint()//激活检查点
    {
        if (activationStatus == false)//如果检查点未激活
        {
            AudioManager.instance.PlaySFX(5, transform);//播放音效
            
            // 通知GameManager记录这个检查点为最后激活的检查点
            if (GameManager.instance != null)
            {
                GameManager.instance.SetLastActivatedCheckpoint(id);
            }
        }

        activationStatus = true;
        anim.SetBool("active", true);
    }
}

