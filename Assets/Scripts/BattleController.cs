using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField] public LevelData[] levelConfigs;
    [SerializeField] public ChapterData[] chapterConfigs;
    private LevelData currentLevelData;
    private ChapterData currentChapterData;
    private MapMaker mapMaker;

    private int currentWave = 0;
    private bool isWaveActive = false;
    private bool isEndGame = false;
    private int currentMoney;
    private int currentHealth;
    private readonly object moneyLock = new object();
    private readonly object healthLock = new object();
    private readonly object enemyNumberLock = new object();

    public static BattleController Instance;
    private string currentLevel;
    private int enemyNumber = 0;
    private MechaClockControl mechaClockControl;
    private DigitalClockControl digitalClockControl;

    public GameObject endGameUI;
    public TextMeshProUGUI endGameUIString;
    public GameObject MechaClockLevel;
    public GameObject DigitalClockLevel;
    public TextMeshProUGUI moneyText;  // 添加对金钱UI的引用
    public TextMeshProUGUI healthText; // 添加对生命值UI的引用
    public TextMeshProUGUI healthTextCenter; // 添加对生命值UI的引用
    void Awake()
    {
        Instance = this;

    }

    void Start()
    {
        // 检查是否有存储的关卡索引
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            currentLevel = PlayerPrefs.GetString("CurrentLevel");
            InitialLevelConfig(currentLevel);
            
            // 用完后清除，避免下次加载场景时重复使用
            PlayerPrefs.DeleteKey("CurrentLevel");
        }
        else
        {
            //默认关卡
            string levelIndex = "1_1";
            InitialLevelConfig(levelIndex);
        }
    }

    public void InitialLevelConfig(string levelIndex)
    {
        currentWave = 0;  // 重置波次
        isWaveActive = false;  // 重置状态
        
        // 加载章节配置
        string[] levelPrefix = levelIndex.Split('_');  //取'_'之前的部分
        Debug.Log("levelPrefix = " + levelPrefix[0]);
        var chapterConfig = System.Array.Find(chapterConfigs, x => x.chapterName == levelPrefix[0]);
        if (chapterConfig == null)
        {
            DebugLevelControl.Log($"未找到章节 {levelPrefix[0]} 的配置", 
                DebugLevelControl.DebugModule.BattleController, 
                DebugLevelControl.LogLevel.Error);
            Debug.LogError($"未找到章节 {levelPrefix[0]} 的配置");
        }

        currentChapterData = chapterConfig;
        mapMaker = GetComponent<MapMaker>();
        mapMaker.InitMap(chapterConfig.xColumn, chapterConfig.yRow);
        
        if (chapterConfig.chapterType == ChapterType.MechaClock)
        {
            MechaClockLevel.SetActive(true);
            mechaClockControl = MechaClockLevel.GetComponent<MechaClockControl>();
            mechaClockControl.SetClockActive(false);
            
        }
        else if (chapterConfig.chapterType == ChapterType.DigitalClock)
        {
            DigitalClockLevel.SetActive(true);
            digitalClockControl = DigitalClockLevel.GetComponent<DigitalClockControl>();
            digitalClockControl.SetClockActive(false);
        }

        // 加载关卡配置
        var levelConfig = System.Array.Find(levelConfigs, x => x.levelName == levelIndex);
        if (levelConfig == null)
        {   
            DebugLevelControl.Log($"未找到关卡 {levelIndex} 的配置", 
                DebugLevelControl.DebugModule.BattleController, 
                DebugLevelControl.LogLevel.Error);
            Debug.LogError($"未找到关卡 {levelIndex} 的配置");
        }

        currentLevelData = levelConfig;
        currentMoney = levelConfig.initialMoney;
        currentHealth = levelConfig.initialHealth;
        InitUI();

        if (chapterConfig.chapterType == ChapterType.MechaClock)
        {
            mechaClockControl.SetClockTime(levelConfig.startTimeHour * 60 * 60);                
        }
        else if (chapterConfig.chapterType == ChapterType.DigitalClock)
        {
            digitalClockControl.SetClockTime(levelConfig.startTimeHour * 60 * 60);
        }
        
    }
    private void InitUI()
    {
        moneyText.text = $"Money: {currentLevelData.initialMoney}";
        healthText.text = $"Health: {currentLevelData.initialHealth}";
        healthTextCenter.text = currentLevelData.initialHealth.ToString();
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public void UpdateMoney(int money)
    {
        lock (moneyLock)
        {
            currentMoney += money;
            moneyText.text = $"Money: {currentMoney}";
        }
    }

    public void UpdateEnemyNumber(int number)
    {
        lock (enemyNumberLock)
        {
            enemyNumber += number;
        }
    }
    
    public void UpdateHealth(int health)
    {
        lock (healthLock)
        {
            currentHealth += health;
            healthText.text = $"Health: {currentHealth}";
            healthTextCenter.text = currentHealth.ToString();
        }
    }

    public void OnStartButtonClick()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("请先初始化关卡配置！");
            return;
        }

        if (currentWave >= currentLevelData.waves.Count)
        {
            Debug.Log("所有波次已结束!" + currentWave + "totalWaves = " + currentLevelData.waves.Count);
            return;
        }

        if (isWaveActive)
        {
            Debug.Log("当前波次正在进行中!");
            return;
        }

        StartCoroutine(SpawnWave(currentWave));

        setClockEnable(true);

        //调试代码，只出1个怪
        // GameObject enemy = Instantiate(enemyPrefab, transform);
        // if (enemy != null)
        // {
        //     enemy.transform.position = currentLevel.spawnPoints[0].GridObject.transform.position;
        // }
    }

    private void setClockEnable(bool enable)
    {
        if (currentLevelData.levelType == LevelType.MechaClock)
        {
            mechaClockControl.SetClockActive(enable);
        }
        else if (currentLevelData.levelType == LevelType.DigitalClock)
        {
            digitalClockControl.SetClockActive(enable);
        }
        else
        {
            Debug.LogError("未找到时钟类型!");
        }
    }
    private IEnumerator SpawnWave(int waveIndex)
    {
        isWaveActive = true;
        LevelData.WaveData wave = currentLevelData.waves[waveIndex];

        // 等待这波开始的延迟
        yield return new WaitForSeconds(wave.waveDelay);

        foreach (LevelData.EnemySpawnData enemyInfo in wave.enemies)
        {
            // 添加出生点索引检查
            if (enemyInfo.spawnPoint < 0 || enemyInfo.spawnPoint >= mapMaker.spawnPoints.Count)
            {
                Debug.LogError($"无效的出生点索引: {enemyInfo.spawnPoint}. 可用出生点数量: {mapMaker.spawnPoints.Count}");
                continue;
            }
            // 等待当前敌人生成的延迟
            yield return new WaitForSeconds(enemyInfo.spawnDelay);

            // 在指定出生点生成敌人
            GameObject enemy = EnemyFactory.Instance.CreateEnemy(enemyInfo.enemyType,
                mapMaker.spawnPoints[enemyInfo.spawnPoint].transform.position);
            
            if (mapMaker.spawnPoints[enemyInfo.spawnPoint].NextSpawnPoint != null)
            {
                mapMaker.spawnPoints[enemyInfo.spawnPoint].NextSpawnPoint.SetActive(false);
            }
            else
            {
                Debug.LogError("未找到下一个出生点!");
            }
            
            if (enemy != null)
            {
                enemyNumber++;
            }
        }

        isWaveActive = false;
        currentWave++;
    }

    // Update is called once per frame
    void Update()
    {
        if (isEndGame) return;

        if (currentWave < currentLevelData.waves.Count)
        {
            //显示下一波敌人的出生点
            LevelData.WaveData wave = currentLevelData.waves[currentWave];
            foreach (LevelData.EnemySpawnData enemyInfo in wave.enemies)
            {
                if (mapMaker.spawnPoints[enemyInfo.spawnPoint].NextSpawnPoint != null)
                {
                    mapMaker.spawnPoints[enemyInfo.spawnPoint].NextSpawnPoint.SetActive(true);
                }
                else
                {
                    Debug.LogError("未找到下一个出生点!");
                }
            }
        }

        if (currentHealth <= 0)
        {
            endGameUI.SetActive(true);
            endGameUIString.text = "Game Over";
            isEndGame = true;

        }
        else if (currentWave >= currentLevelData.waves.Count && enemyNumber <= 0)
        {
            endGameUI.SetActive(true);
            endGameUIString.text = "Game Win";
            isEndGame = true;
        }
        else if (enemyNumber <= 0)
        {
            setClockEnable(false);
        }
        else if (enemyNumber > 0)
        {
            setClockEnable(true);
        }
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

    public void OnClickReturnToLevelSelect()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public MapMaker GetMapMaker()
    {
        return mapMaker;
    }
}
