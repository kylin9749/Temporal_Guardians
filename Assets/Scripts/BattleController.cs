using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 描述单个怪物的生成信息
public struct EnemySpawnInfo
{
    public EnemyType enemyType; // 怪物类型
    public int spawnPoint;     // 出生点编号
    public float spawnDelay;   // 出生延迟时间
}

// 描述单波怪物的信息
public struct WaveInfo 
{
    public List<EnemySpawnInfo> enemies;  // 该波次所有怪物的生成信息
    public float waveDelay;               // 该波次开始前的延迟时间
}

// 关卡配置
public class LevelConfig
{
    public int totalWaves;                // 总波数
    public List<WaveInfo> waves;          // 每波的具体信息
    public List<MapGrid> spawnPoints;
    public LevelConfig()
    {
        waves = new List<WaveInfo>();
    }
}

public class BattleController : MonoBehaviour
{
    public LevelConfig currentLevel;  // 当前关卡配置
    private int currentWave = 0;       // 当前波次
    private bool isWaveActive = false; // 当前波次是否正在进行
    // Start is called before the first frame update
    public GameObject enemyPrefab;  // 敌人预制体
    public static BattleController Instance;



    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化关卡配置
        currentLevel = InitialLevelConfig();

        // 记录出怪点
        MapMaker mapMaker = this.transform.GetComponent<MapMaker>();
        if (mapMaker.gridObjects != null)
        {
            for(int i = 0; i < mapMaker.xColumn; i++)
            {
                for(int j = 0; j < mapMaker.yRow; j++)
                {
                    if(mapMaker.gridObjects[i, j].SpawnType == SpawnPointType.Right
                        || mapMaker.gridObjects[i, j].SpawnType == SpawnPointType.Left
                        || mapMaker.gridObjects[i, j].SpawnType == SpawnPointType.Top
                        || mapMaker.gridObjects[i, j].SpawnType == SpawnPointType.Bottom)
                    {  
                        currentLevel.spawnPoints.Add(mapMaker.gridObjects[i, j]);
                    }
                }

            }
        }

    }

    public static LevelConfig InitialLevelConfig()
    {
        LevelConfig config = new LevelConfig();
        config.totalWaves = 3;
        System.Random random = new System.Random();
        config.spawnPoints = new List<MapGrid>();

        for(int wave = 0; wave < config.totalWaves; wave++)
        {
            WaveInfo waveInfo = new WaveInfo();
            waveInfo.enemies = new List<EnemySpawnInfo>();
            waveInfo.waveDelay = random.Next(3, 8); // 每波延迟3-8秒

            for(int i = 0; i < 10; i++) // 每波10个敌人
            {
                EnemySpawnInfo enemyInfo = new EnemySpawnInfo();
                enemyInfo.enemyType = (EnemyType)random.Next(0, 10); // 随机选择敌人类型
                enemyInfo.spawnPoint = random.Next(0, 40); // 随机选择出生点(0-40)(6 * 2 + 14 * 2)
                enemyInfo.spawnDelay = random.Next(1, 5); // 每个敌人延迟1-5秒出生
                waveInfo.enemies.Add(enemyInfo);
            }
            config.waves.Add(waveInfo);
        }

        return config;
    }
    
    public void OnStartButtonClick()
    {
        Debug.Log("OnStartButtonClick");
        if (currentWave >= currentLevel.totalWaves)
        {
            Debug.Log("所有波次已结束!");
            return;
        }

        if (isWaveActive)
        {
            Debug.Log("当前波次正在进行中!");
            return;
        }

        // StartCoroutine(SpawnWave(currentWave));

        //调试代码，只出1个怪
        GameObject enemy = Instantiate(enemyPrefab, transform);
        if (enemy != null)
        {
            enemy.transform.position = currentLevel.spawnPoints[0].GridObject.transform.position;
        }
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        isWaveActive = true;
        WaveInfo wave = currentLevel.waves[waveIndex];

        // 等待这波开始的延迟
        yield return new WaitForSeconds(wave.waveDelay);

        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            // 在指定出生点生成敌人
            GameObject enemy = Instantiate(enemyPrefab, transform);
            if (enemy != null)
            {
                enemy.transform.position = currentLevel.spawnPoints[enemyInfo.spawnPoint].GridObject.transform.position;
            }

            // 设置敌人类型
            EnemyCommon enemyComponent = enemy.GetComponent<EnemyCommon>();
            if (enemyComponent != null)
            {

            }

            // 等待下一个敌人生成的延迟
            yield return new WaitForSeconds(enemyInfo.spawnDelay);
        }

        isWaveActive = false;
        currentWave++;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void createTower(TowerType towerType)
    {
        GameObject tower = TowerFactory.Instance.CreateTower(towerType, GetMousePosition());
    }

    public void createTower1()
    {
        createTower(TowerType.Sleep);
    }

    public void createTower2() 
    {
        createTower(TowerType.Boom);
    }

    public void createTower3()
    {
        createTower(TowerType.AttackSpeedIncrease);
    }

    public void createTower4()
    {
        // createTower(TowerType.Tower4);
    }

    public void createTower5()
    {
        // createTower(TowerType.Tower5);
    }

    public void createTower6()
    {
        // createTower(TowerType.Tower6);
    }

    public void createTower7()
    {
        // createTower(TowerType.Tower7);
    }

    public void createTower8()
    {
        // createTower(TowerType.Tower8);
    }

    private Vector3 GetMousePosition()
    {
        // 获取鼠标在屏幕上的位置
        Vector3 mousePos = Input.mousePosition;
        // 将屏幕坐标转换为世界坐标
        mousePos.z = Camera.main.transform.position.y;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
