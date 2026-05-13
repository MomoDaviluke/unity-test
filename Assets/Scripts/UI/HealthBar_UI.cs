using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
 
public class HealthBar_UI : MonoBehaviour
{
    private Entity entity;
    private CharacterStats myStats;
    private RectTransform myTransform;
    private Slider slider;
 
 
    private void Start()
    {
        myTransform = GetComponent<RectTransform>();
        entity = GetComponentInParent<Entity>();
        slider = GetComponentInChildren<Slider>();
        myStats = GetComponentInParent<CharacterStats>();
 
        UpdateHealthUI();
 
        entity.onFlipped += FlipUI;
        myStats.onHealthChanged += UpdateHealthUI;
    }
 
    private void Update()
    {
        // UpdateHealthUI();
    }
 
    private void UpdateHealthUI()//更新血量条函数，此函数由Event触发
    {
        slider.maxValue = myStats.GetMaxHealthValue();
        
        slider.value = myStats.currentHealth;
 
    }
 
    private void FlipUI()//让UI不随着角色翻转
    {
        myTransform.Rotate(0, 180, 0);
    }
 
 
    private void OnDisable()
    {
        entity.onFlipped -= FlipUI;
        myStats.onHealthChanged -= UpdateHealthUI;
    }
}