using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TowerSkillType
{
    Control,             // 控制类型
    FixedDamage,         // 固定伤害类型 
    AttackBuff           // 攻击加强类型
}

public enum TowerType
{
    Sleep,
    Boom,
    AttackSpeedIncrease
}

public enum TowerColor
{
    White,
    Yellow,
    Red,
    Blue
}

public enum TowerCategory
{
    Single,    // 单体
    Area,      // 群攻
    Support,   // 辅助
    Control    // 控制
}

public class TowerFactory : MonoBehaviour 
{
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] public TowerData[] towerConfigs;
    public static TowerFactory Instance;

    private void Awake()
    {
       Instance = this;
    }

    // 统一的创建接口，隐藏了复杂的创建过程
    public GameObject CreateTower(TowerType type, Vector3 position, BattleController controller)
    {
        var config = System.Array.Find(towerConfigs, x => x.towerType == type);
        if (config != null)
        {
            // 封装所有创建逻辑
            var tower = Instantiate(towerBasePrefab, position, Quaternion.identity);
            if(tower != null)   
                tower.GetComponent<towerCommon>().InitializeTower(config, controller);
            else
                Debug.LogError("towerCommon component not found on towerBasePrefab");
            
            // 添加时间增强组件
            TowerTimeEnhancer timeEnhancer = tower.AddComponent<TowerTimeEnhancer>();
            
            // 可以在这里添加其他初始化逻辑
            // 比如特效、音效等
            
            return tower;
        }
        else
        {
            Debug.LogError("TowerConfig not found for type: " + type);
        }
        return null;
    }
} 