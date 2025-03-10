using System.Collections.Generic;

public class TowerComboGroup
{
    private List<towerCommon> towers = new List<towerCommon>();
    private TowerComboType comboType = TowerComboType.None;
    public void AddTower(towerCommon tower)
    {
        if (!towers.Contains(tower))
        {
            towers.Add(tower);
            UpdateComboEffect();
        }
    }
    
    public void RemoveTower(towerCommon tower)
    {
        if (towers.Contains(tower))
        {
            towers.Remove(tower);
            UpdateComboEffect();
        }
    }
    
    private void UpdateComboEffect()
    {
        // 根据组内防御塔数量更新增益效果
        float bonus = towers.Count * 0.05f; // 5%加成/塔
        foreach (towerCommon tower in towers)
        {
            tower.DmageIncreaseFactor = bonus;
        }
    }
    
    public bool IsEmpty()
    {
        return towers.Count == 0;
    }
    
    public TowerColor GetTowerColor()
    {
        if (towers.Count > 0)
            return towers[0].TowerData.towerColor;
        
        //默认返回white
        return TowerColor.White;
    }
    
    public List<towerCommon> GetTowers()
    {
        return towers;
    }
    
    // 重置组合对象状态
    public void Reset()
    {
        // 清除所有防御塔的增益效果
        foreach (towerCommon tower in towers)
        {
            tower.DmageIncreaseFactor = 0;
        }
        towers.Clear();
        comboType = TowerComboType.None;
    }

    public bool ContainsTower(towerCommon tower)
    {
        return towers.Contains(tower);
    }
}