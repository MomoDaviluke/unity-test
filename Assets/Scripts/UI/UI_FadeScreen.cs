using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//2024.11.28
public class UI_FadeScreen : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void FadeOut() => anim.SetTrigger("fadeOut");
    public void FadeIn() => anim.SetTrigger("fadeIn");
}