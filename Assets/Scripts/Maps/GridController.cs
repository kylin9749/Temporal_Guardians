using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Base,   // 防御塔基台
    RoadVertical,   // 纵向道路
    RoadHorizontal,  // 横向道路
    Cross,  // 十字路口
    Center  // 中心点
}

public enum SpawnPointType
{
    Top,    // 上方出怪
    Bottom, // 下方出怪
    Left,   // 左方出怪
    Right,  // 右方出怪
    None    // 无出怪点
}


public class MapGrid : MonoBehaviour
{
    // 格子类型
    public GridType Type { get; set; }

    // 格子坐标
    public int X { get; set; }
    public int Y { get; set; }

    // 是否可通过(用于寻路)
    public bool IsWalkable { get; set; }

    // 是否是出怪点
    public SpawnPointType SpawnType { get; set; }

    // 是否已放置防御塔
    public GameObject Tower { get; set; }

    // 是否被阻挡
    public bool IsBlocked { get; set; }

    public GameObject BaseImage;
    public GameObject CenterImage;
    public GameObject VerticalRoadImage;
    public GameObject HorizontalRoadImage;
    public GameObject CrossRoadImage;
    public GameObject DigitalClockShandow;
    public GameObject NextSpawnPoint;

    public MapGrid GetGridFromVector2Int(Vector2Int tile)
    {
        return BattleController.Instance.GetMapMaker().gridObjects[X + tile.x, Y + tile.y];
    }

    // 获取相邻格子
    public List<MapGrid> GetNeighborGrids()
    {
        List<MapGrid> neighbors = new List<MapGrid>();
        // 上下左右四个方向
        int[] dx = {0, 0, -1, 1};
        int[] dy = {-1, 1, 0, 0};

        for (int i = 0; i < 4; i++)
        {
            int newX = X + dx[i];
            int newY = Y + dy[i];
            
            // 检查边界
            if (newX >= 0 && newX < BattleController.Instance.GetMapMaker().XColumn &&
                newY >= 0 && newY < BattleController.Instance.GetMapMaker().YRow)
            {
                // Debug.Log("newX = " + newX + ", newY = " + newY);
                neighbors.Add(BattleController.Instance.GetMapMaker().GridObjects[newX, newY]);
            }
        }
        return neighbors;
    }

    public List<MapGrid> GetNeighborBaseGrids()
    {
        // 获取防御塔基台的相邻格子
        if (Type != GridType.Base)
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
            int newX = X + dx[i];
            int newY = Y + dy[i];
            
            // 检查边界
            if (newX >= 0 && newX < BattleController.Instance.GetMapMaker().XColumn &&
                newY >= 0 && newY < BattleController.Instance.GetMapMaker().YRow)
            {
                // Debug.Log("newX = " + newX + ", newY = " + newY);
                neighbors.Add(BattleController.Instance.GetMapMaker().GridObjects[newX, newY]);
            }
        }
        return neighbors;
    }
}
