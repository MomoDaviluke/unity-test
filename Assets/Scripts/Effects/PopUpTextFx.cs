using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PopUpTextFx : MonoBehaviour
{
    private TextMeshPro myText;

    [SerializeField] private float speed;//上升速度
    [SerializeField] private float disappearanceSpeed;//文本消失时上升速度的衰减
    [SerializeField] private float colorDisappearanceSpeed;//文本消失时颜色的衰减

    [SerializeField] private float lifeTime;

    private float textTimer;//计时文本的生命周期

    void Start()
    {
        myText = GetComponent<TextMeshPro>();
        textTimer = lifeTime;
    }


    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y + 1), speed * Time.deltaTime);// 文本沿Y轴方向上升


        textTimer -= Time.deltaTime;// 计时器递减

        if (textTimer < 0)
        {
            float alpha = myText.color.a - colorDisappearanceSpeed * Time.deltaTime;// 文本的透明度逐渐减少
            myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, alpha);


            if (myText.color.a < 50)// 当透明度小于50时，减慢文本的上升速度
                speed = disappearanceSpeed;

            if (myText.color.a <= 0)
            {
                Destroy(gameObject);// 当透明度小于0时，销毁文本
            }
        }
    }
}