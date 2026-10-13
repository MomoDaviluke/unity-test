using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 黑洞热键控制器类
/// 控制黑洞技能的热键显示和交互
/// 当按下对应热键时，将敌人添加到黑洞的目标列表中
/// </summary>
public class Blackhole_HotKey_Controller : MonoBehaviour
{
    // 精灵渲染器组件，用于控制显示
    private SpriteRenderer sr;
    // 当前热键对应的按键
    private KeyCode myHotKey;
    // 文本组件，用于显示热键提示
    private TextMeshProUGUI myText;

    // 关联的敌人变换组件
    private Transform myEnemy;
    // 黑洞技能控制器的引用
    private Blackhole_skill_controller blackHole;

    /// <summary>
    /// 设置热键的参数
    /// </summary>
    /// <param name="_mynewHotKey">热键按键</param>
    /// <param name="_myEnemy">关联的敌人</param>
    /// <param name="_myBlackHole">黑洞技能控制器</param>
    public void SetupHotKey(KeyCode _mynewHotKey, Transform _myEnemy, Blackhole_skill_controller _myBlackHole)
    {
        // 获取精灵渲染器组件
        sr = GetComponent<SpriteRenderer>();
        // 获取文本组件
        myText = GetComponentInChildren<TextMeshProUGUI>();

        // 初始化敌人和黑洞引用
        myEnemy = _myEnemy;
        blackHole = _myBlackHole;

        // 设置热键和显示文本
        myHotKey = _mynewHotKey;
        myText.text = _mynewHotKey.ToString();
    }

    /// <summary>
    /// 每帧检测热键输入
    /// 当按下对应热键时，将敌人添加到黑洞目标列表
    /// </summary>
    private void Update()
    {
        // 检测热键是否被按下
        if (Input.GetKeyDown(myHotKey))
        {
            // 将敌人添加到黑洞的目标列表中
            blackHole.AddEnemyToList(myEnemy);

            // 隐藏文本和精灵显示
            myText.color = Color.clear;
            sr.color = Color.clear;
        }
    }
}
