using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleController : MonoBehaviour
{
    [SerializeField] public LevelData[] levelConfigs;
    private LevelData currentLevel;
    private int currentWave = 0;
    private bool isWaveActive = false;
    public GameObject enemyPrefab;  // 敌人预制体
    private int currentMoney;
    private int currentHealth;
    public TextMeshProUGUI moneyText;  // 添加对金钱UI的引用
    public TextMeshProUGUI healthText; // 添加对生命值UI的引用
    public TextMeshProUGUI healthTextCenter; // 添加对生命值UI的引用
    private readonly object moneyLock = new object();
    private readonly object healthLock = new object();
    public static BattleController Instance;

    void Awake()
    {
        Instance = this;
        // 检查是否有存储的关卡索引
        if (PlayerPrefs.HasKey("SelectedLevelIndex"))
        {
            int levelIndex = PlayerPrefs.GetInt("SelectedLevelIndex");
            InitialLevelConfig(levelIndex);
            // 用完后清除，避免下次加载场景时重复使用
            PlayerPrefs.DeleteKey("SelectedLevelIndex");
        }
        else
        {
            //默认关卡
            int levelIndex = 0;
            InitialLevelConfig(levelIndex);
        }
    }

    void Start()
    {
        // 移除任何硬编码的初始化
        // 现在将由外部调用 InitialLevelConfig 来设置关卡
    }

    public void InitialLevelConfig(int levelIndex)
    {
        currentWave = 0;  // 重置波次
        isWaveActive = false;  // 重置状态
        
        var config = System.Array.Find(levelConfigs, x => x.levelIndex == levelIndex);
        if (config != null)
        {   
            currentLevel = config;
            currentMoney = config.initialMoney;
            currentHealth = config.initialHealth;
            InitUI();
        }
        else
        {
            Debug.LogError($"未找到关卡 {levelIndex} 的配置");
        }

        
    }
    private void InitUI()
    {
        moneyText.text = $"Money: {currentLevel.initialMoney}";
        healthText.text = $"Health: {currentLevel.initialHealth}";
        healthTextCenter.text = currentLevel.initialHealth.ToString();
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
        if (currentLevel == null)
        {
            Debug.LogError("请先初始化关卡配置！");
            return;
        }
        
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

        StartCoroutine(SpawnWave(currentWave));

        //调试代码，只出1个怪
        // GameObject enemy = Instantiate(enemyPrefab, transform);
        // if (enemy != null)
        // {
        //     enemy.transform.position = currentLevel.spawnPoints[0].GridObject.transform.position;
        // }
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        isWaveActive = true;
        LevelData.WaveData wave = currentLevel.waves[waveIndex];

        // 等待这波开始的延迟
        yield return new WaitForSeconds(wave.waveDelay);

        foreach (LevelData.EnemySpawnData enemyInfo in wave.enemies)
        {
            // 在指定出生点生成敌人
            GameObject enemy = EnemyFactory.Instance.CreateEnemy(enemyInfo.enemyType,
                MapMaker.Instance.spawnPoints[enemyInfo.spawnPoint].GridObject.transform.position);

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

        //临时调试，让防御塔出现
        GameObject tower = GameObject.FindGameObjectWithTag("towers");
        if (tower != null)
        {
            tower.GetComponent<towerCommon>().EnableTower();
        }
    }

    public void createTower8()
    {
        // createTower(TowerType.Tower8);

        //临时调试，让防御塔消失
        GameObject tower = GameObject.FindGameObjectWithTag("towers");
        if (tower != null)
        {
            tower.GetComponent<towerCommon>().DisableTower();
        }


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
