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
    //private TowerComboControl towerComboControl;

    private int currentWave = 0;
    private bool isWaveActive = false;
    private bool isEndGame = false;
    private int currentMoney;
    private int currentHealth;
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
    public TextMeshProUGUI waveText;  // 添加对波数UI的引用
    [SerializeField] private PauseMenuUI pauseMenuUI;  // 在Inspector中指定

    private int previousWave = -1; // 添加变量跟踪上一次的波次
    private List<GameObject> activeEnemies = new List<GameObject>();

    // 添加 TimeManager 预制体引用
    public GameObject timeManagerPrefab;
    private TimeManager timeManager;

    // 添加LineRenderer和路径指示器预制体
    public GameObject pathIndicatorPrefab;
    public Material pathMaterial;
    private Dictionary<int, LineRenderer> pathRenderers = new Dictionary<int, LineRenderer>();
    private Dictionary<int, GameObject> pathIndicators = new Dictionary<int, GameObject>();

    // 添加配置项
    [SerializeField] private int pathAnimationLoops = -1; // 动画循环次数，-1表示无限循环

    void Awake()
    {

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
        //初始化位置联动系统
        //towerComboControl = new TowerComboControl();
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
            return;
        }

        currentChapterData = chapterConfig;
        mapMaker = GetComponent<MapMaker>();
        mapMaker.InitMap(chapterConfig.xColumn, chapterConfig.yRow);

        // 加载关卡配置
        var levelConfig = System.Array.Find(levelConfigs, x => x.levelName == levelIndex);
        if (levelConfig == null)
        {   
            DebugLevelControl.Log($"未找到关卡 {levelIndex} 的配置", 
                DebugLevelControl.DebugModule.BattleController, 
                DebugLevelControl.LogLevel.Error);
            Debug.LogError($"未找到关卡 {levelIndex} 的配置");
        }

        // 初始化 TimeManager
        if (timeManagerPrefab != null)
        {
            GameObject timeManagerObj = Instantiate(timeManagerPrefab);
            timeManager = timeManagerObj.GetComponent<TimeManager>();
            
            if (timeManager != null)
            {
                timeManager.Initialize(levelConfig.startTimeHour);
            }
            else
            {
                Debug.LogError("无法获取 TimeManager 组件");
            }
        }
        else
        {
            // 如果未指定预制体，则直接在场景中创建
            GameObject timeManagerObj = new GameObject("TimeManager");
            timeManager = timeManagerObj.AddComponent<TimeManager>();
            timeManager.Initialize(levelConfig.startTimeHour);
        }



        currentLevelData = levelConfig;
        currentMoney = levelConfig.initialMoney;
        currentHealth = levelConfig.initialHealth;
        InitUI();

        if (chapterConfig.chapterType == ChapterType.MechaClock)
        {
            MechaClockLevel.SetActive(true);
            mechaClockControl = MechaClockLevel.GetComponent<MechaClockControl>();
            mechaClockControl.Initialize(this);
            
            // 设置时钟控制器
            if (timeManager != null)
            {
                timeManager.SetClockControls(mechaClockControl, null);
            }
        }
        else if (chapterConfig.chapterType == ChapterType.DigitalClock)
        {
            DigitalClockLevel.SetActive(true);
            digitalClockControl = DigitalClockLevel.GetComponent<DigitalClockControl>();
            digitalClockControl.Initialize(this);
            
            // 设置时钟控制器
            if (timeManager != null)
            {
                timeManager.SetClockControls(null, digitalClockControl);
            }
        }
        
        // 初始化后更新生成点状态
        UpdateSpawnPointsVisibility();
    }
    private void InitUI()
    {
        moneyText.text = $"Money: {currentLevelData.initialMoney}";
        healthText.text = $"Health: {currentLevelData.initialHealth}";
        healthTextCenter.text = currentLevelData.initialHealth.ToString();
        UpdateWaveText();  // 初始化波数显示
    }

    // 添加更新波数显示的方法
    private void UpdateWaveText()
    {
        waveText.text = $"Waves: {currentWave}/{currentLevelData.waves.Count}";
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public void UpdateMoney(int money)
    {   
        currentMoney += money;
        moneyText.text = $"Money: {currentMoney}";
    }

    public void UpdateEnemyNumber(int number, EnemyNumChangeType changeType, GameObject enemy)
    {
        if(changeType == EnemyNumChangeType.Add)
        {
            enemyNumber += number;
            activeEnemies.Add(enemy);
        }
        else if(changeType == EnemyNumChangeType.Sub)
        {
            enemyNumber -= number;
            activeEnemies.Remove(enemy);
        }
        else
        {
            Debug.LogError("无效的敌人数量变化类型!");
        }
    }
    
    public void UpdateHealth(int health)
    {
        currentHealth += health;
        healthText.text = $"Health: {currentHealth}";
        healthTextCenter.text = currentHealth.ToString();
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

        // 停止可能正在等待的自动开始协程
        StopCoroutine("AutoStartNextWave");
        
        StartCoroutine(SpawnWave(currentWave));

        // 激活时钟
        setClockEnable(true);
        
        // 启动时间流逝
        if (timeManager != null)
        {
            timeManager.StartTimeFlow();
        }
    }

    private void setClockEnable(bool enable)
    {
        if (currentChapterData.chapterType == ChapterType.MechaClock)
        {
            mechaClockControl.SetClockActive(enable);
        }
        else if (currentChapterData.chapterType == ChapterType.DigitalClock)
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
        
        // 开始生成敌人
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
                mapMaker.spawnPoints[enemyInfo.spawnPoint].transform.position, this);
            
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
                activeEnemies.Add(enemy);
            }
        }
        
        // 清除路径预览
        ClearPathPreviews();
        
        isWaveActive = false;
        currentWave++;
        
        // 波次变化后更新下一波生成点
        UpdateSpawnPointsVisibility();
        
        // 更新波数显示
        UpdateWaveText();
        
        // 添加：如果还有下一波，则自动安排下一波的生成
        if (currentWave < currentLevelData.waves.Count)
        {
            // 安排下一波的自动生成
            StartCoroutine(AutoStartNextWave());
        }
    }

    // 添加新方法：安排下一波自动开始
    private IEnumerator AutoStartNextWave()
    {
        // 使用当前波次的 waveDelay 作为下一波的等待时间
        float nextWaveDelay = currentLevelData.waves[currentWave].waveDelay;
        yield return new WaitForSeconds(nextWaveDelay);
        
        // 如果没有手动启动且仍然需要启动下一波
        if (!isWaveActive && currentWave < currentLevelData.waves.Count && !isEndGame)
        {
            StartCoroutine(SpawnWave(currentWave));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isEndGame) return;

        // 只在波次变化时更新生成点
        if (currentWave < currentLevelData.waves.Count && currentWave != previousWave)
        {
            UpdateSpawnPointsVisibility();
            UpdateWaveText();  // 在波数变化时更新显示
            previousWave = currentWave;
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
    }

    // 提取出专门的方法来更新生成点可见性
    private void UpdateSpawnPointsVisibility()
    {
        if (currentWave < currentLevelData.waves.Count)
        {
            LevelData.WaveData wave = currentLevelData.waves[currentWave];
            
            // 先清除之前的路径预览
            ClearPathPreviews();
            
            // 显示出生点和路径预览
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
            
            // 添加路径预览显示
            ShowPathPreviews(wave);
        }
    }

    private void createTower(TowerType towerType)
    {
        // 检查是否有防御塔正在选择Combo
        /*
        if (towerComboControl.IsInComboMode())
        {
            if (UITipManager.Instance != null)
            {
                UITipManager.Instance.ShowTip("请先完成当前的组合操作");
            }
            return;
        }
        */
        GameObject tower = TowerFactory.Instance.CreateTower(towerType, GetMousePosition(), this);
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
        StartCoroutine(LoadLevelSelectAsync());
    }
    
    private IEnumerator LoadLevelSelectAsync()
    {
        // 先清理资源
        BattleController battleController = FindObjectOfType<BattleController>();
        if (battleController != null)
        {
            battleController.CleanupBeforeSceneChange();
        }
        
        Time.timeScale = 1f;
        
        switch(currentChapterData.chapterType)
        {
            case ChapterType.MechaClock:
                PlayerPrefs.SetInt("SelectedChapter", 1);
                SceneManager.LoadScene("LevelScene");
                break;
            case ChapterType.DigitalClock:
                PlayerPrefs.SetInt("SelectedChapter", 2);
                SceneManager.LoadScene("LevelScene");
                break;
            default:
                Debug.LogError("未找到章节类型!");
                break;
        }

        // 异步加载主菜单场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LevelScene");
        
        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void CleanupBeforeSceneChange()
    {
        // 先销毁所有敌人
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 清理时钟资源
        if (mechaClockControl != null)
        {
            mechaClockControl.CleanupResources();
        }
        
        if (digitalClockControl != null)
        {
            digitalClockControl.CleanupResources();
        }

        // 清理地图资源
        if (mapMaker != null)
        {
            mapMaker.CleanupResources();
        }
        
        // 重置状态变量
        currentWave = 0;
        isWaveActive = false;
        isEndGame = false;
        enemyNumber = 0;
        
        // 恢复正常时间流速（如果暂停状态）
        Time.timeScale = 1f;

        // 清理 TimeManager
        if (timeManager != null)
        {
            Destroy(timeManager.gameObject);
            timeManager = null;
        }

        // 销毁当前对象
        Destroy(this.gameObject);
    }

    // 处理场景卸载事件
    private void OnDestroy()
    {
        CleanupBeforeSceneChange();
    }
    public MapMaker GetMapMaker()
    {
        return mapMaker;
    }

    /*
    public TowerComboControl GetTowerComboControl()
    {
        return towerComboControl;
    }
    */

    // 显示路径预览方法
    private void ShowPathPreviews(LevelData.WaveData wave)
    {
        // 为每个生成点创建一条路径
        HashSet<int> processedSpawnPoints = new HashSet<int>();
        
        foreach (LevelData.EnemySpawnData enemyInfo in wave.enemies)
        {
            // 避免重复处理同一个生成点
            if (processedSpawnPoints.Contains(enemyInfo.spawnPoint))
                continue;
            
            processedSpawnPoints.Add(enemyInfo.spawnPoint);
            
            // 计算从生成点到中心的路径
            List<MapGrid> path = CalculatePathForPreview(mapMaker.spawnPoints[enemyInfo.spawnPoint], mapMaker.CenterGrid);
            
            if (path.Count > 0)
            {
                // 创建路径线
                CreatePathLine(enemyInfo.spawnPoint, path);
                
                // 创建路径指示器
                CreatePathIndicator(enemyInfo.spawnPoint, path);
            }
        }
    }

    // 计算预览路径
    private List<MapGrid> CalculatePathForPreview(MapGrid start, MapGrid end)
    {
        // 使用BFS计算路径
        Queue<MapGrid> queue = new Queue<MapGrid>();
        Dictionary<MapGrid, MapGrid> cameFrom = new Dictionary<MapGrid, MapGrid>();
        
        queue.Enqueue(start);
        cameFrom[start] = null;
        
        bool foundPath = false;
        
        while (queue.Count > 0 && !foundPath)
        {
            MapGrid current = queue.Dequeue();
            
            if (current == end)
            {
                foundPath = true;
                break;
            }
            
            foreach (MapGrid neighbor in mapMaker.GetNeighborGrids(current))
            {
                if ((mapMaker.isRoadType(neighbor.Type) || neighbor.Type == GridType.Center) && !cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }
        
        // 如果找到路径，重建它
        List<MapGrid> path = new List<MapGrid>();
        if (foundPath)
        {
            MapGrid current = end;
            while (current != null)
            {
                path.Insert(0, current);
                if (cameFrom.TryGetValue(current, out MapGrid previous))
                    current = previous;
                else
                    current = null;
            }
        }
        
        return path;
    }

    // 创建路径线
    private void CreatePathLine(int spawnPointIndex, List<MapGrid> path)
    {
        GameObject lineObj = new GameObject("PathLine_" + spawnPointIndex);
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // 配置LineRenderer
        line.material = pathMaterial;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = path.Count;
        line.startColor = new Color(0f, 1f, 1f, 0.5f);  // 半透明蓝色起点
        line.endColor = new Color(0f, 1f, 1f, 0.5f);    // 相同颜色终点

        // 设置路径点
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pointPosition = path[i].transform.position;
            pointPosition.y += 0.05f; // 稍微抬高一点，避免与地面重叠
            line.SetPosition(i, pointPosition);
        }
        
        // 存储LineRenderer以便后续清理
        pathRenderers[spawnPointIndex] = line;
    }

    // 创建路径指示器
    private void CreatePathIndicator(int spawnPointIndex, List<MapGrid> path)
    {
        if (pathIndicatorPrefab == null) return;
        
        // 实例化指示器
        GameObject indicator = Instantiate(pathIndicatorPrefab, path[0].transform.position, Quaternion.identity);
        
        // 存储指示器以便后续清理
        pathIndicators[spawnPointIndex] = indicator;
        
        // 启动指示器动画协程
        StartCoroutine(AnimatePathIndicator(indicator, path));
    }

    // 指示器动画
    private IEnumerator AnimatePathIndicator(GameObject indicator, List<MapGrid> path)
    {
        float speed = 4.0f; // 移动速度
        
        int loopCount = 0;
        while (pathAnimationLoops == -1 || loopCount < pathAnimationLoops)
        {
            // 沿路径移动指示器
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPos = path[i].transform.position;
                Vector3 endPos = path[i + 1].transform.position;
                float distance = Vector3.Distance(startPos, endPos);
                float duration = distance / speed;
                
                float elapsed = 0;
                while (elapsed < duration)
                {
                    if (indicator == null) yield break; // 安全检查
                    
                    float t = elapsed / duration;
                    indicator.transform.position = Vector3.Lerp(startPos, endPos, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                indicator.transform.position = endPos;
            }
            
            // 立即将指示器移回起点
            if (indicator != null && path.Count > 0)
                indicator.transform.position = path[0].transform.position;
            
            yield return new WaitForSeconds(0.5f);
            
            if (pathAnimationLoops != -1)
                loopCount++;
        }
    }

    // 清除路径预览
    private void ClearPathPreviews()
    {
        // 清理路径线
        foreach (var renderer in pathRenderers.Values)
        {
            if (renderer != null)
                Destroy(renderer.gameObject);
        }
        pathRenderers.Clear();
        
        // 清理指示器
        foreach (var indicator in pathIndicators.Values)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        pathIndicators.Clear();
    }

}
