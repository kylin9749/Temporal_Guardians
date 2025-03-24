using System;
using UnityEngine;

/// <summary>
/// 防御塔时间效果配置
/// </summary>
[Serializable]
public class TowerTimeEffect
{
    [Header("基本设置")]
    public bool isDayTimeEnhanced = true;  // true为白天增强，false为夜晚增强
    
    [Header("增强效果")]
    [Range(1.0f, 3.0f)]
    public float enhancedDamageMultiplier = 1.5f;  // 攻击力增强倍率
    [Range(1.0f, 3.0f)]
    public float enhancedSpeedMultiplier = 1.5f;   // 攻速增强倍率
    [Range(1.0f, 2.0f)]
    public float enhancedRangeMultiplier = 1.2f;   // 范围增强倍率
    [Range(0.5f, 2.0f)]
    public float enhancedSkillTimeMultiplier = 1.5f; // 技能持续时间增强倍率
    
    [Header("狂暴时间")]
    [Range(0, 23)]
    public int frenzyHour = 12;  // 狂暴时刻（小时）
    [Range(0, 59)]
    public int frenzyMinute = 0; // 狂暴时刻（分钟）
    
    [Header("描述")]
    [TextArea(2, 4)]
    public string enhancedEffectDescription = "在特定时间获得属性提升";  // 增强效果描述
    [TextArea(2, 4)]
    public string frenzySkillDescription = "在特定时间点触发强力技能";    // 狂暴技能描述
}