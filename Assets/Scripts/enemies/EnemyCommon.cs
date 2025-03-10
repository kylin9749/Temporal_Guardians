using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void InitializeEnemy(EnemyData data)
    {
        // 初始化状态
        currentHealth = data.maxHealth;
        currentMoveSpeed = data.originalMoveSpeed;
        
        // 保存数据引用
        enemyData = data;

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
        Debug.Log($"怪物当前生命值:{currentHealth}, 受到伤害:{damage}");

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

        mapMaker = BattleController.Instance.GetMapMaker();
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
        currentGrid = GetCurrentGridByPosition();
        if(currentGrid == null) return null;
        // Debug.Log("currentGrid.type = " + currentGrid.Type);
        // Debug.Log("currentGrid.position = " + currentGrid.X + "," + currentGrid.Y);

        distanceToCenter = CalculateDistanceToCenter(currentGrid);

        // 如果怪物已经走到中心点，则销毁当前怪物并触发掉血逻辑
        if(currentGrid.Type == GridType.Center)
        {
            Die(true);
            isDead = true;
            return null;
        }

        // 如果怪物没走到currentGrid，的中间，则维持原方向继续走，暂时不选新路径
        if(nextGrid != null)
        {
            if (Vector3.Distance(transform.position, currentGrid.transform.position) > 0.1f)
            {
                return nextGrid;
            }
        }

        List<MapGrid> validNeighbors = new List<MapGrid>();
        foreach (MapGrid neighbor in currentGrid.GetNeighborGrids())
        {
            if (mapMaker.isRoadType(neighbor.Type) || neighbor.Type == GridType.Center)
            {
                validNeighbors.Add(neighbor);
            }
        }

        if(validNeighbors.Count == 0)
        {
            Debug.LogError("validNeighbors.Count = " + validNeighbors.Count);
            return null;
        }
        // Debug.Log("validNeighbors.Count = " + validNeighbors.Count);
        // foreach(MapGrid grid in validNeighbors)
        // {
        //     Debug.Log("neighbor.type = " + grid.Type);
        //     Debug.Log("neighbor.position = " + grid.X + "," + grid.Y);
        // }
        // 找到距离中心最近的邻居格子
        List<MapGrid> bestGrids = new List<MapGrid>();
        int minDistance = int.MaxValue;

        // 先找出所有最短距离的格子
        foreach (MapGrid neighbor in validNeighbors)
        {
            int distance = CalculateDistanceToCenter(neighbor);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestGrids.Clear();
                bestGrids.Add(neighbor);
            }
            else if (distance == minDistance)
            {
                bestGrids.Add(neighbor);
            }
        }
        // Debug.Log("bestGrids.Count = " + bestGrids.Count);
        // 默认情况下只有1个最佳选择
        nextGrid = bestGrids[0];

        // 如果存在多个最佳选择，则需要根据当前怪物的preferTurn选择
        if (bestGrids.Count == 2)
        {
            // 确认前方的两条路是直行 or 拐弯，再根据当前怪物的preferTurn选择  
            foreach(MapGrid grid in bestGrids)
            {
                Vector2Int nextToward = new Vector2Int(grid.X - currentGrid.X, grid.Y - currentGrid.Y);
                if(nextToward == currentToward && !enemyData.preferTurn)
                {
                    nextGrid = grid;
                    break;
                }
                else if(nextToward != currentToward && enemyData.preferTurn)
                {
                    nextGrid = grid;
                    break;
                }
                else
                {
                    continue;
                }
            }
        }

        currentToward.Set(nextGrid.X - currentGrid.X, nextGrid.Y - currentGrid.Y);
        return nextGrid;

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
            foreach (MapGrid neighbor in current.GetNeighborGrids())
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
            BattleController.Instance.UpdateHealth(-1);
        }
        else
        {
            //玩家获胜，增加金币
            BattleController.Instance.UpdateMoney(enemyData.coinsDrop);
        }
        BattleController.Instance.UpdateEnemyNumber(-1);
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
}
