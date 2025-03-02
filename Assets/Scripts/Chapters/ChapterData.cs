using UnityEngine;
using System.Collections.Generic;

public enum ChapterType
{
    MechaClock,
    DigitalClock
}

[CreateAssetMenu(fileName = "NewChapterData", menuName = "Tower Defense/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public string chapterName;              // 关卡名称
    public ChapterType chapterType;           // 关卡类型
    public int xColumn;
    public int yRow;
} 