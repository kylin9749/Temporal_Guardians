
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapMaker : MonoBehaviour
{
    //开关属性、画线开关
    public bool drawLine;
    //地图宽
    private float mapWidth;
    //高
    private float mapHeight;
    //格子宽
    private float gridWidth;
    //格子高
    private float gridHeight;
    //行数
    public int yRow = 13; 
    //列数
    public int xColumn = 29;
    //防御塔基台
    public GameObject gridBase;
    //格子对象数组
    public MapGrid[,] gridObjects;
    //中心格子
    private MapGrid centerGrid;
    //出怪点
    public List<MapGrid> spawnPoints;
    public static MapMaker Instance;
    //地图原点
    private Vector3 mapOrigin;

    void Start()
    {

    }

    private void Awake()
    {
        Instance = this;
        InitMap();
    }

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

    public float GridWidth
    {
        get { return gridWidth; }
    }

    public float GridHeight
    {
        get { return gridHeight; }
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
    public void InitMap()
    {
        CalculateSize();
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

                itemGo.transform.localScale = new Vector3(gridWidth, gridHeight, 1);
                itemGo.transform.position = CorretPositon(x * gridWidth, y * gridHeight);
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

    //获取格子类型
    public GridType GetGridType(int x, int y)
    {
        if (y == 1 || y == 3 || y == 5 || y == 7 || y == 9 || y == 11)
        {
            // 直接检查x是否为垂直道路的条件
            if (x == 1 || x == 3 || x == 5 || x == 7 || x == 9 || x == 11 || x == 13 || x == 15
                    || x == 17 || x == 19 || x == 21 || x == 23 || x == 25 || x == 27)
            {
                return GridType.Cross;
            }
        }

        if (x == 1 || x == 3 || x == 5 || x == 7 || x == 9 || x == 11 || x == 13 || x == 15
                || x == 17 || x == 19 || x == 21 || x == 23 || x == 25 || x == 27)
        {
            return GridType.RoadVertical;
        }
        else if (y == 1 || y == 3 || y == 5 || y == 7 || y == 9 || y == 11)
        {
            return GridType.RoadHorizontal;
        }
        else if (x == 14 && y == 6)
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
        return new Vector3(x - mapWidth / 2 + gridWidth / 2, y- mapHeight / 2 + gridHeight / 2);
    }

    //计算地图格子的宽高
    private void CalculateSize()
    {
        //左下角
        Vector3 leftDown = new Vector3(0.1f, 0.1f);
        //右上角
        Vector3 rightUp = new Vector3(0.9f, 0.9f);
        //视口坐标转世界坐标 、 左下角的世界坐标
        Vector3 posOne = Camera.main.ViewportToWorldPoint(leftDown);
        //右上角
        Vector3 posTwo = Camera.main.ViewportToWorldPoint(rightUp);

        //地图高
        mapHeight = posTwo.y - posOne.y;
        //格子的宽
        gridWidth = mapHeight / yRow;
        //格子高
        gridHeight = mapHeight / yRow;
        //地图宽
        mapWidth = gridWidth * xColumn;
    }


    // 画格子
    private void OnDrawGizmos()
    {
        //画线
        if (drawLine)
        {
            //计算格子的大小
            CalculateSize();
            //格子的颜色
            Gizmos.color = Color.green;
            //画出行数   这里的值应该是要等于行数的
            for (int y = 0; y <= yRow; y++)
            {
                //起始位置
                Vector3 startPos = new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                //终点坐标
                Vector3 endPos = new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                //画线
                Gizmos.DrawLine(startPos, endPos);
            }
            //画列
            for (int x = 0; x <= xColumn; x++)
            {
                Vector3 startPos = new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
                Vector3 endPos = new Vector3(-mapWidth / 2 + x * gridWidth, -mapHeight / 2);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
        else
        {
            return;
        }
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

    public MapGrid[] GetAdjacentBases(MapGrid baseGrid)
    {
        List<MapGrid> adjacentBases = new List<MapGrid>();
        Vector2Int pos = new Vector2Int(baseGrid.X, baseGrid.Y);

        // 检查上下左右相邻的格子
        Vector2Int[] directions = new Vector2Int[] {
            new Vector2Int(0, 2),  // 上
            new Vector2Int(0, -2), // 下 
            new Vector2Int(-2, 0), // 左
            new Vector2Int(2, 0)   // 右
        };

        foreach (Vector2Int dir in directions)
        {
            int newX = pos.x + dir.x;
            int newY = pos.y + dir.y;

            // 检查新坐标是否在地图范围内
            if (newX >= 0 && newX < xColumn && newY >= 0 && newY < yRow)
            {
                // 如果相邻格子是base类型,添加到列表
                if (gridObjects[newX, newY].Type == GridType.Base)
                {
                    adjacentBases.Add(gridObjects[newX, newY]);
                }
            }
        }

        return adjacentBases.ToArray();
    }
}
