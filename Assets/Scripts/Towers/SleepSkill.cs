using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepSkill : TowerSkillCommon
{
    public override void CastSkill()
    {
        // 对列表中的所有敌人施加睡眠效果
        if (tower.GetEnemyList() != null)
        {
            StartCoroutine(SleepEnemies());
        }
    }
    private void BeginSkill(List<GameObject> affectedEnemies)
    {
        // 对列表中的每个敌人启动睡眠协程
        foreach (GameObject enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                EnemyCommon enemyScript = enemy.GetComponent<EnemyCommon>();
                if (enemyScript != null)
                {
                    // 停止敌人移动
                    enemyScript.Stop();
                }
            }
        }
    }

    private void EndSkill(List<GameObject> affectedEnemies)
    {
        // 恢复所有敌人移动
        foreach (GameObject enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                EnemyCommon enemyScript = enemy.GetComponent<EnemyCommon>();
                if (enemyScript != null)
                {
                    enemyScript.Resume();
                }
            }
        }

        // 技能释放完毕
        tower.IsSkilling = false;
        tower.CurrentMp = 0;
    }

    private IEnumerator SleepEnemies()
    {
        // 创建临时列表记录当前被控制的敌人
        List<GameObject> affectedEnemies = new List<GameObject>();
        
        // 将当前enemyList中的敌人添加到临时列表
        foreach (GameObject enemy in tower.GetEnemyList())
        {
            if (enemy != null)
            {
                affectedEnemies.Add(enemy);
            }
        }

        BeginSkill(affectedEnemies);

        // 等待3秒
        yield return new WaitForSeconds(tower.TowerData.skillCastTime);

        EndSkill(affectedEnemies);
    }
}
