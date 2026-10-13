using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillManager（技能管理器）- 单例模式管理所有玩家技能
/// 设计思路：集中管理所有技能组件，便于其他系统统一访问
/// 使用场景：UI显示技能冷却、检查技能解锁状态、获取技能引用
/// Phase 2：继承 Singleton&lt;SkillManager&gt;；执行顺序 -200
/// </summary>
[DefaultExecutionOrder(-200)]
public class SkillManager : Singleton<SkillManager>
{
    public Dash_Skill dash { get; private set; }         // 冲刺技能：快速位移并无敌
    public Clone_Skill clone { get; private set; }       // 分身技能：创建分身协助战斗
    public Sword_Skill sword { get; private set; }       // 飞剑技能：投掷可召回的剑
    public Blackhole_Skill blackhole { get; private set; } // 黑洞技能：吸引敌人并造成多段伤害
    public Crystal_Skill crystal { get; private set; }   // 水晶技能：放置爆炸水晶
    public Parry_SKill parry { get; private set; }       // 格挡技能：格挡并反击
    public Dodge_Skill dodge { get; private set; }       // 闪避技能：增加闪避率

    /// <summary>
    /// 启动时获取同物体上的所有技能组件
    /// 设计思路：技能组件挂载在同一GameObject上，通过GetComponent获取
    /// </summary>
    private void Start()
    {
        dash = GetComponent<Dash_Skill>();
        clone = GetComponent<Clone_Skill>();
        sword = GetComponent<Sword_Skill>();
        blackhole = GetComponent<Blackhole_Skill>();
        crystal = GetComponent<Crystal_Skill>();
        parry = GetComponent<Parry_SKill>();
        dodge = GetComponent<Dodge_Skill>();
    }
}
