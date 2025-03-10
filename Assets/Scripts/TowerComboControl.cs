using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TowerComboState
{
    Normal,         // 普通状态
    Selecting,      // 正在选择组
    BeingSelected,  // 可被选择状态
    InGroup         // 已在组中
}

public enum TowerComboType
{
    None,
    Simple,
    LetterA
}

// 组合对象池类
public class TowerComboGroupPool
{
    private Stack<TowerComboGroup> pool;
    
    public TowerComboGroupPool(int initialSize)
    {
        pool = new Stack<TowerComboGroup>(initialSize);
        // 预先创建对象
        for (int i = 0; i < initialSize; i++)
        {
            pool.Push(new TowerComboGroup());
        }
    }
    
    public TowerComboGroup Get()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }
        else
        {
            // 如果池为空，创建新对象
            return new TowerComboGroup();
        }
    }
    
    public void Return(TowerComboGroup group)
    {
        // 重置组合对象状态
        group.Reset();
        pool.Push(group);
    }
}
public class TowerComboControl
{
    // 组合对象池
    private TowerComboGroupPool groupPool;
    // 初始池大小
    private const int INITIAL_POOL_SIZE = 20;
    // 当前选中的防御塔
    private towerCommon selectedTower;
    // 存储所有的组合
    private List<TowerComboGroup> comboGroups = new List<TowerComboGroup>();
    // 添加新的字段
    private List<TowerComboGroup> highlightedGroups = new List<TowerComboGroup>();
    private TowerComboGroup currentHoveredGroup;
    private towerCommon selectingTower;
    private List<TowerComboGroup> availableGroups = new List<TowerComboGroup>();
    private bool isInComboMode = false;

    public TowerComboControl()
    {
        // 初始化对象池
        groupPool = new TowerComboGroupPool(INITIAL_POOL_SIZE);
    }

    // 添加公共方法来检查当前是否在组合模式
    public bool IsInComboMode()
    {
        return isInComboMode;
    }
    // 检查新放置的防御塔是否可以加入现有组合
    public void CheckCombo(towerCommon newTower)
    {
        availableGroups.Clear();

        // 检查是否可以加入现有组
        foreach (TowerComboGroup group in comboGroups)
        {
            if (CanJoinGroup(newTower, group))
            {
                availableGroups.Add(group);
            }
        }
        DebugLevelControl.Log("availableGroups.count = " + availableGroups.Count,
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Debug);

        // 如果有可加入的组, 则进入组合模式
        if (availableGroups.Count > 0)
        {
            DebugLevelControl.Log("Come to EnterComboMode " + newTower,
                DebugLevelControl.DebugModule.TowerCombo,
                DebugLevelControl.LogLevel.Debug);
            EnterComboMode(newTower);
        }
        else
        {
            // 显示提示：不具备组队条件
            if (UITipManager.Instance != null)
            {
                UITipManager.Instance.ShowTip("当前没有可加入的组");
            }
            DebugLevelControl.Log("Come to CreateNewGroup " + newTower,
                DebugLevelControl.DebugModule.TowerCombo,
                DebugLevelControl.LogLevel.Debug);
            //当前防御塔单独成组
            CreateNewGroup(newTower);
        }
    }
    
    // 检查防御塔是否可以加入组
    private bool CanJoinGroup(towerCommon tower, TowerComboGroup group)
    {
        DebugLevelControl.Log("tower.TowerData.towerColor = " + tower.TowerData.towerColor.ToString(),
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Debug);
        DebugLevelControl.Log("group.GetTowerColor() = " + group.GetTowerColor().ToString(),
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Debug);
        
        // 检查颜色是否相同
        if (tower.TowerData.towerColor != group.GetTowerColor())
        {
            return false;
        };
            
        // 检查是否与组内任意一个防御塔相邻
        foreach (towerCommon groupTower in group.GetTowers())
        {
            // 检查颜色是否相同，如果已经是相同颜色，则不需要再检查
            if (tower.IsAdjacent(tower, groupTower))
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 创建新的组合
    private void CreateNewGroup(towerCommon tower)
    {
        // 从对象池获取组合对象
        TowerComboGroup newGroup = groupPool.Get();
        newGroup.AddTower(tower);
        comboGroups.Add(newGroup);
        tower.setCurrentGroup(newGroup);

        // 检查是否形成特殊图案
        CheckSpecialPattern(newGroup);
    }
    
    // 往组中添加防御塔
    public void AddTowerIntoGroup(towerCommon tower, TowerComboGroup group)
    {
        group.AddTower(tower);
        tower.setCurrentGroup(group);
    }
    // 将防御塔从组中移除
    public void RemoveTowerFromGroup(towerCommon tower, TowerComboGroup group)
    {
        if (group.ContainsTower(tower))
        {
            group.RemoveTower(tower);
            if (group.IsEmpty())
            {
                comboGroups.Remove(group);
                // 将空组返回对象池
                groupPool.Return(group);
            }

            tower.setCurrentGroup(null);
        }
    }

    // 修改移除防御塔的方法
    public void RemoveTowerFromHoverGroup(towerCommon tower)
    {
        if (currentHoveredGroup != null && currentHoveredGroup.ContainsTower(tower))
        {
            RemoveTowerFromGroup(tower, currentHoveredGroup);
            currentHoveredGroup = null; // 清除引用
        }
    }

    public void AddTowerIntoHoverGroup(towerCommon tower)
    {
        if (currentHoveredGroup != null)
        {
            AddTowerIntoGroup(tower, currentHoveredGroup);
            currentHoveredGroup = null; // 清除引用
        }
    }

    // 检查是否形成特殊图案（如字母）
    private void CheckSpecialPattern(TowerComboGroup group)
    {
        // 获取组内所有防御塔的位置
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (towerCommon tower in group.GetTowers())
        {
            positions.Add(new Vector2Int(tower.CurrentGrid.X, tower.CurrentGrid.Y));
        }
        
        // // 检查是否匹配预定义的图案
        // foreach (SpecialPattern pattern in PredefinedPatterns.Patterns)
        // {
        //     if (pattern.Matches(positions))
        //     {
        //         ApplySpecialEffect(group, pattern.Type);
        //         break;
        //     }
        // }
    }
    
    // 添加新方法
    public void OnGroupHover(TowerComboGroup group)
    {
        if (!isInComboMode) return;

        currentHoveredGroup = group; // 记录当前悬停的组
        foreach (var tower in group.GetTowers())
        {
            tower.SetHighlight(true);
        }
    }

    public void OnGroupHoverExit()
    {
        if (!isInComboMode) return;

        if (currentHoveredGroup != null)
        {
            foreach (var tower in currentHoveredGroup.GetTowers())
            {
                tower.SetHighlight(false);
            }
        }
        currentHoveredGroup = null; // 清除当前悬停的组
    }

    private void EnterComboMode(towerCommon tower)
    {
        isInComboMode = true;
        selectingTower = tower;
        
        // 设置选择状态
        tower.SetComboState(TowerComboState.Selecting);
        
        // 设置可选组的状态
        foreach (var group in availableGroups)
        {
            foreach (var groupTower in group.GetTowers())
            {
                groupTower.SetComboState(TowerComboState.BeingSelected);
            }
        }
    }

    public void ExitComboMode()
    {
        if (!isInComboMode) return;

        // 清除所有状态
        if (selectingTower != null)
        {
            selectingTower.SetComboState(TowerComboState.Normal);
        }

        foreach (var group in availableGroups)
        {
            foreach (var tower in group.GetTowers())
            {
                tower.SetComboState(TowerComboState.Normal);
            }
        }

        isInComboMode = false;
        selectingTower = null;
        availableGroups.Clear();
    }
}
