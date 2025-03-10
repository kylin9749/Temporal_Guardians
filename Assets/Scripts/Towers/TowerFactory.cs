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

public class TowerFactory : MonoBehaviour 
{
    [SerializeField] private GameObject towerBasePrefab;
    [SerializeField] private TowerData[] towerConfigs;
    public static TowerFactory Instance;

    private void Awake()
    {
       Instance = this;
    }

    // 统一的创建接口，隐藏了复杂的创建过程
    public GameObject CreateTower(TowerType type, Vector3 position)
    {
        var config = System.Array.Find(towerConfigs, x => x.towerType == type);
        if (config != null)
        {
            // 封装所有创建逻辑
            var tower = Instantiate(towerBasePrefab, position, Quaternion.identity);
            if(tower != null)   
                tower.GetComponent<towerCommon>().InitializeTower(config);
            else
                Debug.LogError("towerCommon component not found on towerBasePrefab");
            
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