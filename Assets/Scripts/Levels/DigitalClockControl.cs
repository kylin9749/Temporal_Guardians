using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigitalClockControl : MonoBehaviour
{
    private System.DateTime startTime = new System.DateTime(2024, 1, 1, 0, 0, 0);
    // 数字显示区域的定义
    private readonly int[,] digitPositions = new int[4,2] {
        {2, 2},   // 小时十位
        {8, 2},   // 小时个位
        {16, 2},  // 分钟十位
        {22, 2}   // 分钟个位
    };
    
    // 数字0-9的显示模板 (5x9的布尔数组)
    private static readonly bool[,,] digitPatterns = new bool[10,5,9] {
        // 0的模式
        {
            {true, true,  true,  true,  true,  true,  true,  true,  true},
            {true, false, false, false, false, false, false, false, true},
            {true, false, false, false, false, false, false, false, true},
            {true, false, false, false, false, false, false, false, true},
            {true, true,  true,  true,  true,  true,  true,  true,  true}
        },
        // 1的模式 (其他数字模式类似)
        {
            {false, false, false, false, false, false, false, false, false},
            {false, false, false, false, false, false, false, false, false},
            {false, false, false, false, false, false, false, false, false},
            {false, false, false, false, false, false, false, false, false},
            {true,  true,  true,  true,  true,  true,  true,  true,  true}
        },
        // 2的模式
        {
            {true, true,  true,  true,  true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, true,  true,  true,  true}
        },
        // 3的模式
        {
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, true,  true,  true,  true, true,  true,  true,  true}
        },
        // 4的模式
        {
            {false, false, false, false, true, true,  true,  true,  true},
            {false, false, false, false, true, false, false, false, false},
            {false, false, false, false, true, false, false, false, false},
            {false, false, false, false, true, false, false, false, false},
            {true,  true,  true,  true,  true, true,  true,  true,  true}
        },
        // 5的模式
        {
            {true, false, false, false, true, true,  true,  true,  true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, true,  true,  true,  true, false, false, false, true}
        },
        // 6的模式
        {
            {true, true,  true,  true,  true, true,  true,  true,  true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, true,  true,  true,  true, false, false, false, true}
        },
        // 7的模式
        {
            {false, false, false, false, false, false, false, false, true},
            {false, false, false, false, false, false, false, false, true},
            {false, false, false, false, false, false, false, false, true},
            {false, false, false, false, false, false, false, false, true},
            {true,  true,  true,  true,  true,  true,  true,  true,  true}
        },
        // 8的模式
        {
            {true, true,  true,  true,  true, true,  true,  true,  true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, true,  true,  true,  true, true,  true,  true,  true}
        },
        // 9的模式
        {
            {true, false, false, false, true, true,  true,  true,  true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, false, false, false, true, false, false, false, true},
            {true, true,  true,  true,  true, true,  true,  true,  true}
        },
    };

    private bool isClockActive = false;

    private int[] currentDigits = new int[4]; // 当前显示的数字
    private Dictionary<Vector2Int, List<GameObject>> digitTowers; // 记录每个位置的防御塔

    void Start()
    {
        digitTowers = new Dictionary<Vector2Int, List<GameObject>>();
        UpdateDisplay(startTime.Hour, startTime.Minute);
        StartCoroutine(UpdateClock());

        //始终显示电子表中间的两个点
        MapMaker.Instance.GridObjects[14, 4].transform.Find("DigitalClockShandow").gameObject.SetActive(true);
        MapMaker.Instance.GridObjects[14, 8].transform.Find("DigitalClockShandow").gameObject.SetActive(true);
    }

    public void SetClockTime(float timeInSeconds)
    {
        // 将秒数转换为TimeSpan
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeInSeconds);
        
        // 更新startTime为基准时间加上传入的时间
        startTime = new System.DateTime(2024, 1, 1, timeSpan.Hours, timeSpan.Minutes, 0);
        
        // 立即更新显示
        UpdateDisplay(startTime.Hour, startTime.Minute);
    }

    public void SetClockActive(bool active)
    {
        isClockActive = active;
    }

    IEnumerator UpdateClock()
    {
        while (true)
        {
            UpdateDisplay(startTime.Hour, startTime.Minute);
            yield return new WaitForSeconds(2f);   // 每2秒更新一次,相当于走过了1分钟
            startTime = startTime.AddMinutes(1);   // 每次更新增加1分钟
        }
    }

    void UpdateDisplay(int hour, int minute)
    {
        if (!isClockActive) return;

        currentDigits[0] = hour / 10;
        currentDigits[1] = hour % 10;
        currentDigits[2] = minute / 10;
        currentDigits[3] = minute % 10;

        for (int i = 0; i < 4; i++)
        {
            UpdateDigit(i, currentDigits[i]);
        }
    }

    void UpdateDigit(int position, int number)
    {
        int startX = digitPositions[position, 0];
        int startY = digitPositions[position, 1];
        
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                Vector2Int gridPos = new Vector2Int(startX + x, startY + y);
                bool isActive = digitPatterns[number, x, y];
                // 更新防御塔状态
                UpdateTowersAtPosition(position, gridPos, isActive);
                // 更新数字显示
                UpdateGridForDigitalClock(position, gridPos, isActive);
            }
        }
    }

    void UpdateTowersAtPosition(int position, Vector2Int gridPos, bool active)
    {
        int startX = digitPositions[position, 0];
        int startY = digitPositions[position, 1];

        MapGrid grid = MapMaker.Instance.GridObjects[gridPos.x, gridPos.y];
        if (grid == null) return;
        if (grid.Tower == null) return;

        if (gridPos.x >= startX && gridPos.x < startX + 5 &&
            gridPos.y >= startY && gridPos.y < startY + 9)
        {
            // 如果这个位置是数字的边缘和中间的横线（数字8）
            if (gridPos.x == startX || gridPos.x == startX + 4 ||
                gridPos.y == startY || gridPos.y == startY + 4 ||gridPos.y == startY + 8)
            {
                if (active)
                {
                    grid.Tower.GetComponent<towerCommon>().EnableTower();
                }
                else
                {
                    grid.Tower.GetComponent<towerCommon>().DisableTower();
                }
            }
        }
    }

    void UpdateGridForDigitalClock(int position, Vector2Int gridPos, bool active)
    {
        MapMaker.Instance.GridObjects[gridPos.x, gridPos.y]
            .DigitalClockShandow.SetActive(active);
    }
}
