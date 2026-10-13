using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ParallaxBackground（视差背景）- 实现2D视差滚动效果
/// 功能：根据相机移动产生不同层次背景移动，营造深度感
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    // ==================== 核心组件 ====================
    private GameObject cam;

    // ==================== 视差参数 ====================
    [SerializeField] private float parallaxEffect;
    private float xPosition;
    private float length;

    // ==================== 初始化 ====================
    private void Start()
    {
        cam = GameObject.Find("Main Camera");
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        xPosition = transform.position.x;
    }

    // ==================== 视差滚动 ====================
    private void Update()
    {
        if (cam == null) return;

        float distanceMoved = cam.transform.position.x * (1 - parallaxEffect);
        float distanceToMove = cam.transform.position.x * parallaxEffect;

        transform.position = new Vector3(xPosition + distanceToMove, transform.position.y);

        if (distanceMoved > xPosition + length)
            xPosition = xPosition + length;
        else if (distanceMoved < xPosition - length)
            xPosition = xPosition - length;
    }
}
