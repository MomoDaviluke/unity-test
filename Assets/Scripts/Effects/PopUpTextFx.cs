using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// PopUpTextFx（弹出文本特效）- 控制伤害数字等弹出文本的动画
/// 功能：上升移动、透明度渐变、自动销毁
/// </summary>
public class PopUpTextFx : MonoBehaviour
{
    // ==================== 核心组件 ====================
    private TextMeshPro myText;

    // ==================== 移动参数 ====================
    [SerializeField] private float speed;
    [SerializeField] private float disappearanceSpeed;
    [SerializeField] private float colorDisappearanceSpeed;

    // ==================== 生命周期 ====================
    [SerializeField] private float lifeTime;
    private float textTimer;

    // ==================== 初始化 ====================
    void Start()
    {
        myText = GetComponent<TextMeshPro>();
        textTimer = lifeTime;
    }

    // ==================== 上升与渐隐 ====================
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y + 1), speed * Time.deltaTime);

        textTimer -= Time.deltaTime;

        if (textTimer < 0)
        {
            float alpha = myText.color.a - colorDisappearanceSpeed * Time.deltaTime;
            myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, alpha);

            // 修复 B4：原判断 alpha < 50 对 0~1 的 alpha 恒成立，导致文本一进入淡出就立刻加速上飘。
            // Color.a 取值 0~1：应在"淡出过半"后才切换到 disappearanceSpeed 加速消失，
            // 故阈值改为 < 0.5f（与 prefab 数值语义一致：speed=1 缓升，过半后切 15 加速）。
            if (myText.color.a < 0.5f)
                speed = disappearanceSpeed;

            if (myText.color.a <= 0)
                Destroy(gameObject);
        }
    }
}
