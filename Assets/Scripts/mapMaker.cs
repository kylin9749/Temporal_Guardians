
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
    public GameObject towerBase;
    //道路
    public GameObject roadBase;
    //中心点
    public GameObject centerBase;
    //格子对象数组
    public MapGrid[,] gridObjects;
    //中心格子
    public MapGrid centerGrid;

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

        //初始化格子对象数组
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                gridObjects[x,y] = new MapGrid(x, y);
            }
        }

        //初始化所有格子为Base类型
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                gridObjects[x,y].Type = GridType.Base;
            }
        }

        //设置6条横向道路
        for (int x = 0; x < xColumn; x++)
        {
            gridObjects[x,1].Type = GridType.Road;
            gridObjects[x,3].Type = GridType.Road;
            gridObjects[x,5].Type = GridType.Road;
            gridObjects[x,7].Type = GridType.Road;
            gridObjects[x,9].Type = GridType.Road;
            gridObjects[x,11].Type = GridType.Road;
        }

        //设置14条纵向道路
        for (int y = 0; y < yRow; y++)
        {
            gridObjects[1,y].Type = GridType.Road;  
            gridObjects[3,y].Type = GridType.Road;
            gridObjects[5,y].Type = GridType.Road;
            gridObjects[7,y].Type = GridType.Road;
            gridObjects[9,y].Type = GridType.Road;
            gridObjects[11,y].Type = GridType.Road;
            gridObjects[13,y].Type = GridType.Road;
            gridObjects[15,y].Type = GridType.Road;
            gridObjects[17,y].Type = GridType.Road;
            gridObjects[19,y].Type = GridType.Road;
            gridObjects[21,y].Type = GridType.Road;
            gridObjects[23,y].Type = GridType.Road;
            gridObjects[25,y].Type = GridType.Road;
            gridObjects[27,y].Type = GridType.Road;
        }

        //设置中心点
        gridObjects[14,6].Type = GridType.Center;
        CenterGrid = gridObjects[14,6];

        //设置道路的边界是出怪格，对于纵向道路，怪物从上下方出来，对于横向道路，怪物从左右方出来
        //横向道路的左右边界
        for(int i = 1; i < yRow; i+=2)
        {  
            if(i == 1 || i == 3 || i == 5 || i == 7 || i == 9 || i == 11)
            {
                gridObjects[0,i].SpawnType = SpawnPointType.Left;  //左边界
                gridObjects[xColumn-1,i].SpawnType = SpawnPointType.Right;  //右边界
            }
        }
        //纵向道路的上下边界
        for(int i = 1; i < xColumn; i+=2)
        {
            if(i == 1 || i == 3 || i == 5 || i == 7 || i == 9 || i == 11 || i == 13 || i == 15 || i == 17 || i == 19 || i == 21 || i == 23 || i == 25 || i == 27)
            {
                gridObjects[i,0].SpawnType = SpawnPointType.Bottom;  //下边界
                gridObjects[i,yRow-1].SpawnType = SpawnPointType.Top;  //上边界
            }
        }

        //生成格子
        for (int x = 0; x < xColumn; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                //根据格子类型生成不同的预制件
                GameObject itemGo;
                if(gridObjects[x,y].Type == GridType.Base)
                {
                    itemGo = Instantiate(towerBase, transform.position, transform.rotation);
                }
                else if(gridObjects[x,y].Type == GridType.Center)
                {
                    itemGo = Instantiate(centerBase, transform.position, transform.rotation);
                }
                else
                {
                    itemGo = Instantiate(roadBase, transform.position, transform.rotation);
                }
                //设置属性
                gridObjects[x,y].GridObject = itemGo;
                itemGo.transform.localScale = new Vector3(gridWidth, gridHeight, 1);
                itemGo.transform.position = CorretPositon(x * gridWidth, y * gridHeight);
                itemGo.transform.SetParent(transform);
            }
        }

        //设置地图原点
        mapOrigin = new Vector3(gridObjects[0,0].GridObject.transform.position.x,
                                gridObjects[0,0].GridObject.transform.position.y, 0);
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
                    Vector3 gridPosition = gridObjects[x, y].GridObject.transform.position;
                    
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
