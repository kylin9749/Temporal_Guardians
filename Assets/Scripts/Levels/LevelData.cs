using UnityEngine;
using System.Collections.Generic;

public enum LevelType
{
    MechaClock,
    DigitalClock
}

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Tower Defense/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;              // 关卡名称
    public int initialMoney;              // 玩家初始金钱
    public int initialHealth;             // 玩家初始血量
    public LevelType levelType;           // 关卡类型
    public int startTimeHour;            // 开始时间小时

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