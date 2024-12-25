using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Base,   // 防御塔基台
    Road,   // 道路
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


public class MapGrid
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
    // 当前格子对应的场景物体
    public GameObject GridObject { get; set; }

    // 构造函数
    public MapGrid(int x, int y, GridType type = GridType.Base)
    {
        X = x;
        Y = y;
        Type = type;
        
        // 根据格子类型初始化属性
        switch(type)
        {
            case GridType.Base:
                IsWalkable = false;
                SpawnType = SpawnPointType.None;
                Tower = null;
                break;
            case GridType.Road:
                IsWalkable = true; 
                SpawnType = SpawnPointType.None;
                Tower = null;
                break;
            case GridType.Center:
                IsWalkable = false;
                SpawnType = SpawnPointType.None;
                Tower = null;
                break;
        }
    }
}
