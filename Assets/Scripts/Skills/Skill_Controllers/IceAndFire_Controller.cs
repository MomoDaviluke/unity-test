using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IceAndFire_Controller（冰与火控制器）- 继承自雷电打击控制器
/// 功能：处理冰与火技能的碰撞伤害，调用父类基础伤害逻辑
/// </summary>
public class IceAndFire_Controller : ThounderStrike_Controller
{
    // ==================== 碰撞检测 ====================
    // 触发时调用父类伤害逻辑
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        base.OggerEnter2D(collision);
    }
}
