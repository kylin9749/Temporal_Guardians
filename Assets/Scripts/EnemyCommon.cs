using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Stalker,        // 潜行者
    Mutant,         // 变异者
    Pathmaker,      // 造路者  
    Annihilator,    // 抹杀者
    FrostMage,      // 冰霜法师
    GuardMage,      // 护卫法师
    Werewolf,       // 狼人
    Priest,         // 牧师
    DarkSummoner,   // 暗黑召唤师
    DarkPaladin     // 暗黑圣骑士
}

public class EnemyCommon : MonoBehaviour
{
    public float maxHealth = 1000f;           // 最大生命值
    private float currentHealth;              // 当前生命值
    private float currentMoveSpeed = 1f;      // 移动速度
    public float distanceToEnd;              // 离终点的距离
    public bool isEnraged = false;           // 是否狂暴
    public int coinsDrop = 10;               // 死亡掉落金币数
    public bool isSkilling = false;          // 是否正在释放技能
    public Transform healthBar = null;
    public float originalMoveSpeed = 1f;
    private int stopCount = 0;
    private object lockObj = new object();
    private EnemyType enemyType;
    public bool preferTurn = false;       // 是否偏好转弯
    public MapGrid currentGrid = null;       // 当前格子
    public MapGrid nextGrid = null;         // 下一个格子   
    public Vector2Int currentToward = new Vector2Int(0, 0);
    private bool isDead = false;
    
    // 受到伤害的函数
    public void TakeDamage(float damage)
    {
        Debug.Log($"怪物当前生命值:{currentHealth}, 受到伤害:{damage}");

        // 计算血量百分比
        float healthPercentage = (currentHealth - damage) / maxHealth;

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
    public void commonStart()
    {
        currentHealth = maxHealth;
        currentMoveSpeed = originalMoveSpeed;
    }

    // Update is called once per frame
    /// <summary>
    /// 敌人的通用更新函数
    /// </summary>
    public void commonUpdate()
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
            Vector3 targetPosition = nextGrid.GridObject.transform.position;
            
            // 计算移动方向
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            
            // 以当前速度移动
            transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;
        }

    }

    // 获取下一个移动格子
    protected MapGrid GetNextGrid()
    {
        currentGrid = GetCurrentGridByPosition();
        if(currentGrid == null) return null;

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
            if (Vector3.Distance(transform.position, currentGrid.GridObject.transform.position) > 0.1f)
            {
                return nextGrid;
            }
        }

        List<MapGrid> validNeighbors = new List<MapGrid>();
        foreach (MapGrid neighbor in GetNeighborGrids(currentGrid))
        {
            if (neighbor.Type == GridType.Road || neighbor.Type == GridType.Center)
            {
                validNeighbors.Add(neighbor);
            }
        }

        if(validNeighbors.Count == 0) return null;

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

        // 默认情况下只有1个最佳选择
        nextGrid = bestGrids[0];

        // 如果存在多个最佳选择，则需要根据当前怪物的preferTurn选择
        if (bestGrids.Count == 2)
        {
            // 确认前方的两条路是直行 or 拐弯，再根据当前怪物的preferTurn选择  
            foreach(MapGrid grid in bestGrids)
            {
                Vector2Int nextToward = new Vector2Int(grid.X - currentGrid.X, grid.Y - currentGrid.Y);
                if(nextToward == currentToward && !preferTurn)
                {
                    nextGrid = grid;
                    break;
                }
                else if(nextToward != currentToward && preferTurn)
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
        Vector3 mapOrigin = MapMaker.Instance.MapOrigin;
        float gridWidth = MapMaker.Instance.GridWidth;
        float gridHeight = MapMaker.Instance.GridHeight;
        
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
                if (checkX >= 0 && checkX < MapMaker.Instance.XColumn &&
                    checkY >= 0 && checkY < MapMaker.Instance.YRow)
                {
                    MapGrid grid = MapMaker.Instance.GridObjects[checkX, checkY];
                    Vector3 gridCenter = grid.GridObject.transform.position;
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
            foreach (MapGrid neighbor in GetNeighborGrids(current))
            {
                // 只考虑可行走的格子（Road或Center类型）
                if ((neighbor.Type == GridType.Road || neighbor.Type == GridType.Center) 
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

    // 获取相邻格子
    protected List<MapGrid> GetNeighborGrids(MapGrid currentGrid)
    {
        List<MapGrid> neighbors = new List<MapGrid>();
        // 上下左右四个方向
        int[] dx = {0, 0, -1, 1};
        int[] dy = {-1, 1, 0, 0};

        for (int i = 0; i < 4; i++)
        {
            int newX = currentGrid.X + dx[i];
            int newY = currentGrid.Y + dy[i];
            
            // 检查边界
            if (newX >= 0 && newX < MapMaker.Instance.XColumn &&
                newY >= 0 && newY < MapMaker.Instance.YRow)
            {
                neighbors.Add(MapMaker.Instance.GridObjects[newX, newY]);
            }
        }
        return neighbors;
    }

    // 查找中心格子
    protected MapGrid FindCenterGrid()
    {
        if (MapMaker.Instance.CenterGrid == null)
        {
            return null;
        }

        return MapMaker.Instance.CenterGrid;
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
            BattleController.Instance.UpdateMoney(coinsDrop);
        }
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
}
