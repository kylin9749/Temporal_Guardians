using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyType
{
    Stalker,        // 潜行者
    Mutant,         // 变异者
    //Pathmaker,      // 造路者  
    Annihilator,    // 抹杀者
    FrostMage,      // 冰霜法师
    GuardMage,      // 护卫法师
    Werewolf,       // 狼人
    Priest,         // 牧师
    DarkSummoner,   // 暗黑召唤师
    DarkPaladin     // 暗黑圣骑士
}

public class EnemyFactory : MonoBehaviour 
{
    [SerializeField] private GameObject enemyBasePrefab;
    [SerializeField] private EnemyData[] enemyConfigs;
    public static EnemyFactory Instance;

    private void Awake()
    {
       Instance = this;
    }

    // 统一的创建接口，隐藏了复杂的创建过程
    public GameObject CreateEnemy(EnemyType type, Vector3 position, BattleController battleController)
    {
        var config = System.Array.Find(enemyConfigs, x => x.enemyType == type);
        if (config != null)
        {
            // 封装所有创建逻辑
            var enemy = Instantiate(enemyBasePrefab, position, Quaternion.identity);
            if(enemy != null)   
                enemy.GetComponent<EnemyCommon>().InitializeEnemy(config, battleController);
            else
                Debug.LogError("EnemyCommon component not found on enemyBasePrefab");
            
            // 可以在这里添加其他初始化逻辑
            // 比如特效、音效等
            
            return enemy;
        }
        else
        {
            Debug.LogError("EnemyConfig not found for type: " + type);
        }
        return null;
    }
} 