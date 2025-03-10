using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public TowerType towerType;
    public TowerColor towerColor;
    public float bulletSpeed = 1f;          // 子弹飞行速度
    public float skillCastTime = 1.0f;      // 技能释放时间
    public int damage = 0;                  // 防御塔的攻击伤害值
    public int attackSpeed = 1;             // 防御塔的攻击速度
    public float totalMp = 100;             // 防御塔的最大魔法值
    public Sprite towerSprite;
    public GameObject bulletPrefab;
    public float attackRange = 5f;          // 防御塔的攻击范围
    public int cost = 50;                   // 防御塔的建造费用

    // 技能特定参数
    public TowerSkillType skillType;             // 当前防御塔的技能类型
    public float skillDamage = 50f;
    public float skillRange = 5f;
    // ... 其他技能相关参数 ...
}