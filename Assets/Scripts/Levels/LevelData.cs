using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Tower Defense/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelIndex;                // 关卡编号
    public string levelName;              // 关卡名称
    public int initialMoney;              // 玩家初始金钱
    public int initialHealth;             // 玩家初始血量
    public int totalWaves;                // 总波数

    [System.Serializable]
    public class WaveData
    {
        public float waveDelay;           // 该波次开始前的延迟时间
        public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyType enemyType;       // 怪物类型
        public float spawnDelay;          // 出生延迟时间
        public int spawnPoint;            // 出生点编号
    }

    public List<WaveData> waves = new List<WaveData>();
} 