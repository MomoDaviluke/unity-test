using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_FadeScreen（淡入淡出屏幕UI）- 控制屏幕淡入淡出效果
/// 功能：通过Animator触发淡入淡出动画
/// </summary>
public class UI_FadeScreen : MonoBehaviour
{
    private Animator anim;

    // ==================== 初始化 ====================
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // ==================== 淡入淡出 ====================
    public void FadeOut() => anim.SetTrigger("fadeOut");
    public void FadeIn() => anim.SetTrigger("fadeIn");
}