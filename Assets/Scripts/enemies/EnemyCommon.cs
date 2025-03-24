using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyNumChangeType
{
    Add = 1,
    Sub = -1,
}

public class EnemyCommon : MonoBehaviour
{
    private EnemyData enemyData;
    private float currentHealth;              // 当前生命值
    private float currentMoveSpeed = 1f;      // 移动速度
    public bool isSkilling = false;          // 是否正在释放技能
    private MapMaker mapMaker;
    public Transform healthBar = null;
    public GameObject imageObject = null;
    private int stopCount = 0;
    private object lockObj = new object();
    private MapGrid currentGrid = null;       // 当前格子
    private MapGrid nextGrid = null;         // 下一个格子   
    private Vector2Int currentToward = new Vector2Int(0, 0);
    private bool isDead = false;
    private EnemySkillCommon skillComponent;
    private float distanceToCenter;
    private BattleController battleController;
    private List<MapGrid> pathToCenter = new List<MapGrid>();
    private bool needRecalculatePath = true;
    private Vector3 targetPosition;

    public void InitializeEnemy(EnemyData data, BattleController controller)
    {
        // 初始化状态
        currentHealth = data.maxHealth;
        currentMoveSpeed = data.originalMoveSpeed;

        // 保存数据引用
        enemyData = data;
        
        // 保存战斗控制器引用
        battleController = controller;
        
        // 移除现有的技能组件（如果有）
        var oldSkill = GetComponent<EnemySkillCommon>();
        if (oldSkill != null)
            Destroy(oldSkill);

        // 添加新的技能组件
        System.Type skillType = GetSkillType(data.enemyType);
        if (skillType != null)
        {
            skillComponent = gameObject.AddComponent(skillType) as EnemySkillCommon;
            if (skillComponent == null)
            {
                Debug.LogError($"Failed to add skill component of type {skillType}");
                return;
            }
        }
        else
        {
            Debug.LogError($"Invalid enemy type: {data.enemyType}");
        }

        // 设置敌人的图片
        imageObject.GetComponent<SpriteRenderer>().sprite = data.enemySprite;

    }
    private System.Type GetSkillType(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Stalker:
                return typeof(HideInDarkSkill);
            case EnemyType.Mutant:
                return typeof(HideInDarkSkill);
            case EnemyType.Annihilator:
                return typeof(HideInDarkSkill);
            default:
                return typeof(EmptySkill);
        }
    }

    // 受到伤害的函数
    public void TakeDamage(float damage)
    {
        DebugLevelControl.Log($"怪物当前生命值:{currentHealth}, 受到伤害:{damage}",
            DebugLevelControl.DebugModule.Enemy,
            DebugLevelControl.LogLevel.Debug);

        // 计算血量百分比
        float healthPercentage = (currentHealth - damage) / enemyData.maxHealth;

        // 限制百分比在0-1之间
        healthPercentage = Mathf.Clamp01(healthPercentage);
        
        // 更新血条的缩放
        if (healthBar != null)
        {
            Vector3 scale = healthBar.localScale;
            scale.x = healthPercentage;
            healthBar.localScale = scale;
        }
        currentHealth -= damage;
        
        if(currentHealth <= 0)
        {
            Die(false);
            isDead = true;
        }
    }

    // 死亡处理函数
    // Start is called before the first frame update
    public void Start()
    {
        currentHealth = enemyData.maxHealth;
        currentMoveSpeed = enemyData.originalMoveSpeed;

        mapMaker = battleController.GetMapMaker();
    }

    // Update is called once per frame
    /// <summary>
    /// 敌人的通用更新函数
    /// </summary>
    public void Update()
    {
        if (stopCount > 0 || isDead)
        {
            return;
        }

        // 获取下一个目标格子
        MapGrid nextGrid = GetNextGrid();

        if (nextGrid != null)
        {
            // 获取目标位置
            Vector3 targetPosition = nextGrid.transform.position;
            
            // 计算移动方向
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            
            // 以当前速度移动
            transform.position += moveDirection * (currentMoveSpeed / 2) * Time.deltaTime;
        }

    }

    // 获取下一个移动格子
    protected MapGrid GetNextGrid()
    {
        // 获取当前格子
        currentGrid = GetCurrentGridByPosition();
        if (currentGrid == null) return null;
        
        // 如果到达中心点，触发死亡逻辑
        if (currentGrid.Type == GridType.Center)
        {
            Die(true);
            isDead = true;
            return null;
        }
        
        // 如果需要重新计算路径
        if (needRecalculatePath)
        {
            CalculatePathToCenter();
            needRecalculatePath = false;
        }
        
        // 如果路径为空或只有一个点(当前点)，返回null
        if (pathToCenter.Count <= 1) return null;
        
        // 找到路径中的下一个点
        int currentIndex = pathToCenter.IndexOf(currentGrid);
        if (currentIndex >= 0 && currentIndex < pathToCenter.Count - 1)
        {
            nextGrid = pathToCenter[currentIndex + 1];
            // 更新移动方向
            currentToward.Set(nextGrid.X - currentGrid.X, nextGrid.Y - currentGrid.Y);
            return nextGrid;
        }
        
        // 如果当前点不在路径中，重新计算路径
        needRecalculatePath = true;
        return null;
    }

    // 新增路径计算方法
    private void CalculatePathToCenter()
    {
        pathToCenter.Clear();
        
        // 找到中心点
        MapGrid centerGrid = FindCenterGrid();
        if (centerGrid == null || currentGrid == null) return;
        
        // 使用A*算法计算最短路径
        pathToCenter = CalculatePathAStar(currentGrid, centerGrid);
        
        // 如果没找到路径，尝试使用BFS计算
        if (pathToCenter.Count == 0)
        {
            Debug.LogWarning($"A*算法无法找到从({currentGrid.X},{currentGrid.Y})到中心点的路径，尝试使用BFS");
            pathToCenter = CalculatePathBFS(currentGrid, centerGrid);
            
            if (pathToCenter.Count > 0)
            {
                Debug.Log("BFS成功找到路径");
            }
        }
    }

    // A*寻路算法实现
    private List<MapGrid> CalculatePathAStar(MapGrid start, MapGrid goal)
    {
        // 定义开放和关闭列表
        List<MapGrid> openSet = new List<MapGrid>();
        HashSet<MapGrid> closedSet = new HashSet<MapGrid>();
        
        // 记录每个节点的来源节点，用于重建路径
        Dictionary<MapGrid, MapGrid> cameFrom = new Dictionary<MapGrid, MapGrid>();
        
        // 记录起点到每个节点的实际代价
        Dictionary<MapGrid, float> gScore = new Dictionary<MapGrid, float>();
        
        // 记录每个节点的估计总代价
        Dictionary<MapGrid, float> fScore = new Dictionary<MapGrid, float>();
        
        // 初始化起点
        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = HeuristicCost(start, goal);
        
        while (openSet.Count > 0)
        {
            // 找到开放列表中f值最小的节点
            MapGrid current = GetLowestFScore(openSet, fScore);
            
            // 如果到达目标，重建路径并返回
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }
            
            // 将当前节点从开放列表移到关闭列表
            openSet.Remove(current);
            closedSet.Add(current);
            
            // 检查所有邻居节点
            foreach (MapGrid neighbor in mapMaker.GetNeighborGrids(current))
            {
                // 忽略不可通行的格子和已在关闭列表的格子
                if (!IsWalkable(neighbor) || closedSet.Contains(neighbor))
                    continue;
                
                // 计算通过当前节点到达邻居的代价
                float tentativeGScore = gScore[current] + 1;
                
                // 如果邻居不在开放列表中，添加它
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                // 如果找到了更好的路径，更新代价
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }
                
                // 更新邻居的信息
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCost(neighbor, goal);
            }
        }
        
        // 没找到路径
        return new List<MapGrid>();
    }

    // 获取f值最小的节点
    private MapGrid GetLowestFScore(List<MapGrid> openSet, Dictionary<MapGrid, float> fScore)
    {
        MapGrid lowest = openSet[0];
        float lowestFScore = fScore.GetValueOrDefault(lowest, float.MaxValue);
        
        foreach (var node in openSet)
        {
            float f = fScore.GetValueOrDefault(node, float.MaxValue);
            if (f < lowestFScore)
            {
                lowest = node;
                lowestFScore = f;
            }
        }
        
        return lowest;
    }

    // 计算两点间的启发式代价（曼哈顿距离）
    private float HeuristicCost(MapGrid a, MapGrid b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    // 检查格子是否可行走
    private bool IsWalkable(MapGrid grid)
    {
        return mapMaker.isRoadType(grid.Type) || grid.Type == GridType.Center;
    }

    // 重建路径
    private List<MapGrid> ReconstructPath(Dictionary<MapGrid, MapGrid> cameFrom, MapGrid current)
    {
        List<MapGrid> path = new List<MapGrid>();
        path.Add(current);
        
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        
        return path;
    }

    // 根据当前位置获取所在格子
    protected MapGrid GetCurrentGridByPosition()
    {
        Vector3 currentPos = transform.position;
        Vector3 mapOrigin = mapMaker.MapOrigin;

        // 目前编辑器中配置的格子大小均为1
        float gridWidth = 1;
        float gridHeight = 1;
        
        DebugLevelControl.Log("currentPos.x = " + currentPos.x,
            DebugLevelControl.DebugModule.Enemy,
            DebugLevelControl.LogLevel.Error);
        DebugLevelControl.Log("mapOrigin.x = " + mapOrigin.x,
            DebugLevelControl.DebugModule.Enemy,
            DebugLevelControl.LogLevel.Error);
        DebugLevelControl.Log("gridWidth = " + gridWidth + " gridHeight = " + gridHeight,
            DebugLevelControl.DebugModule.Enemy,
            DebugLevelControl.LogLevel.Error);

        // 先用除法得到大致范围
        int baseX = Mathf.FloorToInt((currentPos.x - mapOrigin.x) / gridWidth);
        int baseY = Mathf.FloorToInt((currentPos.y - mapOrigin.y) / gridHeight);

        // 检查周围3x3范围内的格子
        float minDistance = float.MaxValue;
        MapGrid closestGrid = null;
        
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                int checkX = baseX + xOffset;
                int checkY = baseY + yOffset;
                
                // 确保在地图范围内
                if (checkX >= 0 && checkX < mapMaker.XColumn &&
                    checkY >= 0 && checkY < mapMaker.YRow)
                {
                    MapGrid grid = mapMaker.GridObjects[checkX, checkY];
                    // Debug.Log("checkX = " + checkX + ", checkY = " + checkY);
                    // Debug.Log("grid.position = " + grid.X + "," + grid.Y);
                    Vector3 gridCenter = grid.transform.position;

                    float distance = Vector3.Distance(currentPos, gridCenter);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestGrid = grid;
                    }
                }
            }
        }
        
        return closestGrid;
    }

    // 计算指定格子到中心点的距离
    protected int CalculateDistanceToCenter(MapGrid startGrid)
    {
        MapGrid centerGrid = FindCenterGrid();
        if (centerGrid == null) return int.MaxValue;

        // 使用BFS计算最短路径长度
        Queue<MapGrid> queue = new Queue<MapGrid>();
        Dictionary<MapGrid, int> distances = new Dictionary<MapGrid, int>();
        
        queue.Enqueue(startGrid);
        distances[startGrid] = 0;

        while (queue.Count > 0)
        {
            MapGrid current = queue.Dequeue();
            
            // 如果找到终点，返回距离
            if (current == centerGrid)
            {
                return distances[current];
            }

            // 遍历相邻格子
            foreach (MapGrid neighbor in mapMaker.GetNeighborGrids(current))
            {
                // 只考虑可行走的格子（Road或Center类型）
                if ((mapMaker.isRoadType(neighbor.Type) || neighbor.Type == GridType.Center) 
                    && !distances.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    distances[neighbor] = distances[current] + 1;
                }
            }
        }

        // 如果找不到路径到终点，返回最大值
        return int.MaxValue;
    }

    // 查找中心格子
    protected MapGrid FindCenterGrid()
    {
        if (mapMaker.CenterGrid == null)
        {
            return null;
        }

        return mapMaker.CenterGrid;
    }

    protected virtual void Die(bool isEnemyWin)
    {
        if(isEnemyWin)
        {
            //怪物获胜，减少生命值
            battleController.UpdateHealth(-1);
        }
        else
        {
            //玩家获胜，增加金币
            battleController.UpdateMoney(enemyData.coinsDrop);
        }
        battleController.UpdateEnemyNumber(1, EnemyNumChangeType.Sub, gameObject);
        Destroy(gameObject);
    }

    // 技能释放虚函数，供子类重载
    protected virtual void CastSkill()
    {
        // 基类中为空实现
        // 具体技能效果由子类实现
    }

    // 停止敌人移动,加锁避免多个防御塔同时操作
    public void Stop()
    {
        lock (lockObj)
        {
            stopCount++;
        }
    }

    // 恢复敌人移动,加锁避免多个防御塔同时操作 
    public void Resume()
    {
        lock (lockObj)
        {
            stopCount--;
        }
    }

    public float DistanceToCenter
    {
        get { return distanceToCenter; }
        set { distanceToCenter = value; }
    }

    // BFS寻路算法实现
    private List<MapGrid> CalculatePathBFS(MapGrid start, MapGrid goal)
    {
        // 使用队列进行BFS搜索
        Queue<MapGrid> queue = new Queue<MapGrid>();
        // 记录已访问的节点
        HashSet<MapGrid> visited = new HashSet<MapGrid>();
        // 记录每个节点的来源节点，用于重建路径
        Dictionary<MapGrid, MapGrid> cameFrom = new Dictionary<MapGrid, MapGrid>();
        
        // 初始化
        queue.Enqueue(start);
        visited.Add(start);
        cameFrom[start] = null;
        
        bool foundPath = false;
        
        // BFS主循环
        while (queue.Count > 0 && !foundPath)
        {
            MapGrid current = queue.Dequeue();
            
            // 如果到达目标，结束搜索
            if (current == goal)
            {
                foundPath = true;
                break;
            }
            
            // 检查所有相邻节点
            foreach (MapGrid neighbor in mapMaker.GetNeighborGrids(current))
            {
                // 只考虑可行走且未访问的节点
                if (IsWalkable(neighbor) && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }
        
        // 如果找到路径，重建并返回
        if (foundPath)
        {
            return ReconstructPath(cameFrom, goal);
        }
        
        // 没找到路径，返回空列表
        Debug.LogWarning("BFS也无法找到路径，这可能是地图设计问题");
        return new List<MapGrid>();
    }
}
