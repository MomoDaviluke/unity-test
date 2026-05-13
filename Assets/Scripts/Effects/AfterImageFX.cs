using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageFX : MonoBehaviour
{
    private SpriteRenderer sr;
    private float colorLooseRate;
    private void Awake()
    {
        // 在 Awake 中获取组件，确保 sr 不为 null
        sr = GetComponent<SpriteRenderer>();
    }


    public void SetupAfterImage(float _loosigSpeed, Sprite _spriteImage)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
        sr.sprite = _spriteImage;
        colorLooseRate = _loosigSpeed;
    }

    private void Update()
    {
        // 添加空检查，防止 null 引用异常
        if (sr == null)
            return;
        float alpha = sr.color.a - colorLooseRate * Time.deltaTime;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

        if (sr.color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
