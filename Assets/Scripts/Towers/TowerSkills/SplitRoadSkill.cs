using UnityEngine;
using System.Collections;

public class SplitRoadSkill : TowerSkillCommon
{
    public override void CastSkill()
    {
        float angle = tower.Angle;
        Vector2Int targetTile = GetTargetTile(angle);
        SplitTile(targetTile);
        StartCoroutine(BlockTileForDuration(targetTile, 4f));
    }

    private Vector2Int GetTargetTile(float angle)
    {
        if (angle > -45 && angle <= 45)
            return new Vector2Int(0, 1);
        else if (angle > 45 && angle <= 135)
            return new Vector2Int(-1, 0);
        else if (angle > -135 && angle <= -45)
            return new Vector2Int(1, 0);
        else
            return new Vector2Int(0, -1);
    }

    private void SplitTile(Vector2Int tile)
    {
        // 实现劈开格子的逻辑
        //tower.GetGridFromVector2Int(tile).IsBlocked = true;
    }

    private IEnumerator BlockTileForDuration(Vector2Int tile, float duration)
    {
        // 实现阻止怪物通过的逻辑
        yield return new WaitForSeconds(duration);
        // 恢复格子的状态
    }
}
