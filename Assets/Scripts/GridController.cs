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
        return MapMaker.Instance.gridObjects[X + tile.x, Y + tile.y];
    }
}
