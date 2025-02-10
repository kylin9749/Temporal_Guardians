using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedIncreaseSkill : TowerSkillCommon
{
    MapGrid[] adjacentBases;
    public override void CastSkill()
    {
        MapGrid baseGrid = tower.CurrentGrid;
        adjacentBases = MapMaker.Instance.GetAdjacentBases(baseGrid);

        foreach (MapGrid grid in adjacentBases)
        {
            if (grid.Tower != null)
            {
                //防御塔的攻速增加
                grid.Tower.GetComponent<towerCommon>().AttackSpeedFactor = 2;
            }
        }

        // 给自己加buff
        tower.AttackSpeedFactor = 2;
        tower.CurrentMp = 0;
        tower.IsSkilling = false;
        tower.skillShadow.SetActive(true);

        // 3秒后恢复原始攻速
        StartCoroutine(ResetAttackSpeedAfterDelay(tower.TowerData.skillCastTime));
    }
    private IEnumerator ResetAttackSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tower.AttackSpeedFactor = 1;

        foreach (MapGrid grid in adjacentBases)
        {
            if (grid.Tower != null)
            {
                grid.Tower.GetComponent<towerCommon>().AttackSpeedFactor = 1;
            }
        }
        tower.skillShadow.SetActive(false);
    }
}
