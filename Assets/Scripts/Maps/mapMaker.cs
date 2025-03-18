using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapMaker : MonoBehaviour
{
    //行数
    private int yRow = 13; 
    //列数
    private int xColumn = 29;
    //防御塔基台
    public GameObject gridBase;
    //格子对象数组
    public MapGrid[,] gridObjects;
    //中心格子
    private MapGrid centerGrid;
    //出怪点
    public List<MapGrid> spawnPoints;
    //地图原点
    private Vector3 mapOrigin;

    public int XColumn
    {
        get { return xColumn; }
    }

    public int YRow
    {
        get { return yRow; }
    }

    public MapGrid[,] GridObjects
    {
        get { return gridObjects; }
        set { gridObjects = value; }
    }

    public MapGrid CenterGrid
    {
        get { return centerGrid; }
        set { centerGrid = value; } 
    }

    public Vector3 MapOrigin
    {
        get { return mapOrigin; }
    }

    //初始化地图
    public void InitMap(int X, int Y)
    {
        if (!isSingleNum(X) || !isSingleNum(Y))
        {
            Debug.LogError("行数和列数必须是单数");
        }

        this.yRow = Y;
        this.xColumn = X;

        //创建格子对象数组
        gridObjects = new MapGrid[xColumn, yRow];
        spawnPoints = new List<MapGrid>();

        //生成格子
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                GridType type = GetGridType(x, y);
                //根据格子类型生成不同的预制件
                GameObject itemGo = Instantiate(gridBase, transform.position, transform.rotation);
                MapGrid gridObject = itemGo.GetComponent<MapGrid>();
                gridObject.Type = type;
                gridObject.X = x;
                gridObject.Y = y;

                if(isRoadType(type))
                {
                    if (type == GridType.RoadHorizontal)
                    {
                        if(x == 0)
                        {
                            gridObject.SpawnType = SpawnPointType.Left;
                            spawnPoints.Add(gridObject);
                        }
                        else if(x == xColumn - 1)
                        {
                            gridObject.SpawnType = SpawnPointType.Right;
                            spawnPoints.Add(gridObject);
                        }
                        gridObject.HorizontalRoadImage.SetActive(true);
                    }
                    else if (type == GridType.RoadVertical)
                    {
                        if(y == 0)
                        {
                            gridObject.SpawnType = SpawnPointType.Bottom;
                            spawnPoints.Add(gridObject);
                        }
                        else if(y == yRow - 1)
                        {
                            gridObject.SpawnType = SpawnPointType.Top;
                            spawnPoints.Add(gridObject);
                        }
                        gridObject.VerticalRoadImage.SetActive(true);
                    }
                    else if (type == GridType.Cross)
                    {
                        gridObject.CrossRoadImage.SetActive(true);
                    }
                }
                else if(type == GridType.Center)
                {
                    CenterGrid = itemGo.GetComponent<MapGrid>();
                    gridObject.Type = GridType.Center;
                    gridObject.CenterImage.SetActive(true);
                    //将当前对象的Sprite的显示等级设置为5
                    gridObject.CenterImage.GetComponent<SpriteRenderer>().sortingOrder = 5;
                }
                else
                {
                    gridObject.Type = GridType.Base;
                    gridObject.BaseImage.SetActive(true);
                }
                //设置gridObjects属性
                gridObjects[x, y] = gridObject;
                // Debug.Log("gridObject.position = " + gridObject.X + "," + gridObject.Y);

                itemGo.transform.localScale = new Vector3(1, 1, 1);
                itemGo.transform.position = CorretPositon(x , y);
                itemGo.transform.SetParent(transform);
            }
        }

        //设置地图原点
        mapOrigin = new Vector3(gridObjects[0,0].transform.position.x,
                                gridObjects[0,0].transform.position.y, 0);
    }

    //判断是否是道路
    public bool isRoadType(GridType type)
    {
        return type == GridType.RoadVertical || type == GridType.RoadHorizontal || type == GridType.Cross;
    }

    private  bool isSingleNum(int num)
    {
        return num % 2 == 1;
    }

    //获取格子类型
    public GridType GetGridType(int x, int y)
    {
        if (isSingleNum(y))
        {
            // 直接检查x是否为垂直道路的条件
            if (isSingleNum(x))
            {
                return GridType.Cross;
            }
        }

        if (isSingleNum(x))
        {
            return GridType.RoadVertical;
        }
        else if (isSingleNum(y))
        {
            return GridType.RoadHorizontal;
        }
        else if ((x == (xColumn - 1) / 2) && (y == (yRow - 1) / 2))
        {
            return GridType.Center;
        }
        else
        {
            return GridType.Base;
        }
    }

    //纠正预制件的起始位置
    public Vector3 CorretPositon(float x,float y)
    {
        return new Vector3(x - xColumn / 2 , y - yRow / 2);
    }

    //获取离防御塔最近的base类型的格子
    public MapGrid GetNearestBase(Vector3 position)
    {
        float minDistance = float.MaxValue;
        MapGrid nearestGrid = null;

        // 遍历所有格子，找到离防御塔最近的base类型的格子
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                // 如果当前格子是base类型
                if (gridObjects[x, y].Type == GridType.Base)
                {
                    // 获取当前格子的世界坐标
                    Vector3 gridPosition = gridObjects[x, y].transform.position;
                    
                    // 计算与防御塔的距离
                    float distance = Vector3.Distance(position, gridPosition);
                    
                    // 记录最小距离和对应的格子
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        nearestGrid = gridObjects[x, y];
                    }
                }
            }
        }

        return nearestGrid;
    }

    // 获取相邻格子
    public List<MapGrid> GetNeighborGrids(MapGrid baseGrid)
    {
        List<MapGrid> neighbors = new List<MapGrid>();
        // 上下左右四个方向
        int[] dx = {0, 0, -1, 1};
        int[] dy = {-1, 1, 0, 0};

        for (int i = 0; i < 4; i++)
        {
            int newX = baseGrid.X + dx[i];
            int newY = baseGrid.Y + dy[i];
            
            // 检查边界
            if (newX >= 0 && newX < xColumn &&
                newY >= 0 && newY < yRow)
            {
                // Debug.Log("newX = " + newX + ", newY = " + newY);
                neighbors.Add(gridObjects[newX, newY]);
            }
        }
        return neighbors;
    }

    public List<MapGrid> GetNeighborBaseGrids(MapGrid baseGrid)
    {
        // 获取防御塔基台的相邻格子
        if (baseGrid.Type != GridType.Base)
        {
            Debug.LogError("当前格子不是防御塔基台!");
            return null;
        }

        // 上下左右四个方向
        List<MapGrid> neighbors = new List<MapGrid>();
        // 上下左右四个方向
        int[] dx = {0, 0, -2, 2};
        int[] dy = {-2, 2, 0, 0};

        for (int i = 0; i < 4; i++)
        {
            int newX = baseGrid.X + dx[i];
            int newY = baseGrid.Y + dy[i];
            
            // 检查边界
            if (newX >= 0 && newX < xColumn &&
                newY >= 0 && newY < yRow)
            {
                // Debug.Log("newX = " + newX + ", newY = " + newY);
                neighbors.Add(gridObjects[newX, newY]);
            }
        }
        return neighbors;
    }

    /// <summary>
    /// 清理地图资源，在场景转换前调用
    /// </summary>
    public void CleanupResources()
    {
        // 清理格子上的防御塔引用
        if (gridObjects != null)
        {
            for (int x = 0; x < xColumn; x++)
            {
                for (int y = 0; y < yRow; y++)
                {
                    if (gridObjects[x, y] != null)
                    {
                        gridObjects[x, y].Tower = null;
                    }
                }
            }
        }

        // 清理出生点列表
        if (spawnPoints != null)
        {
            spawnPoints.Clear();
        }
        
        // 重置中心格子引用
        centerGrid = null;
        
        // 清理地图上的所有特效和临时游戏对象
        CleanupMapEffects();

        // 清理所有的格子对象
        if (gridObjects != null)
        {
            for (int x = 0; x < xColumn; x++)
            {
                for (int y = 0; y < yRow; y++)
                {
                    if (gridObjects[x, y] != null)
                    {
                        Destroy(gridObjects[x, y].gameObject);
                    }
                }
            }
            gridObjects = null;
        }
    }

    /// <summary>
    /// 清理地图上的特效和临时对象
    /// </summary>
    private void CleanupMapEffects()
    {
        if (gridObjects != null)
        {
            for (int x = 0; x < xColumn; x++)
            {
                for (int y = 0; y < yRow; y++)
                {
                    if (gridObjects[x, y] != null)
                    {
                        // 禁用NextSpawnPoint提示
                        if (gridObjects[x, y].NextSpawnPoint != null)
                        {
                            gridObjects[x, y].NextSpawnPoint.SetActive(false);
                        }
                        
                        // 禁用时钟阴影
                        if (gridObjects[x, y].DigitalClockShandow != null)
                        {
                            gridObjects[x, y].DigitalClockShandow.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 在对象销毁时自动清理资源
    /// </summary>
    private void OnDestroy()
    {
        CleanupResources();
    }
}
