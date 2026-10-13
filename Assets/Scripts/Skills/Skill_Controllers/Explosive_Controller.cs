using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Explosive_Controller（爆炸控制器）- 控制爆炸物成长和范围伤害
/// 功能：爆炸物膨胀、接近最大尺寸时触发爆炸、范围伤害检测
/// </summary>
public class Explosive_Controller : MonoBehaviour
{
    // ==================== 核心组件与参数 ====================
    private Animator anim;
    private CharacterStats myStats;
    private float growSpeed = 15;
    private float maxSize = 6;
    private float explosionRadius;
    private bool canGrow = true;

    // ==================== 初始化 ====================
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // ==================== 生命周期 ====================
    // 爆炸物逐渐变大，接近最大尺寸时触发爆炸动画
    private void Update()
    {
        if (canGrow)
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), growSpeed * Time.deltaTime);

        if (maxSize - transform.localScale.x < .5f)
        {
            canGrow = false;
            if (anim != null)
                anim.SetTrigger("Explode");
        }
    }

    // 设置爆炸物参数
    public void SetupExplosive(CharacterStats _myStats, float _growSpeed, float _maxSize, float _radius)
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        myStats = _myStats;
        growSpeed = _growSpeed;
        maxSize = _maxSize;
        explosionRadius = _radius;
    }

    // ==================== 爆炸事件 ====================
    // 动画触发时对范围内敌人造成伤害
    private void AnimationExplodeEvent()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in colliders)
        {
            if (hit.GetComponent<CharacterStats>() != null)
            {
                hit.GetComponent<Entity>().SetupKnockbackDir(transform);
                myStats.DoDamage(hit.GetComponent<CharacterStats>());
            }
        }
    }

    // 自我销毁
    private void SelfDestroy() => Destroy(gameObject);
}
