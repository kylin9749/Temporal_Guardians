using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zoneControl : MonoBehaviour
{
    public LayerMask enemyLayer;
    private towerCommon tower;
    public float detectionInterval = 0.5f; // 检测间隔时间
    public GameObject circleVisual; // 用于显示圆形的游戏对象

    void Start()
    {
        tower = transform.parent.GetComponent<towerCommon>();
        StartCoroutine(DetectEnemiesRoutine());

        // // 设置圆形的初始位置和大小
        // UpdateCircleVisual();
    }

    // void UpdateCircleVisual()
    // {
    //     float radius = this.GetComponent<CircleCollider2D>().radius;
    //     circleVisual.transform.position = transform.position;
    //     circleVisual.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
    // }

    IEnumerator DetectEnemiesRoutine()
    {
        while (true)
        {
            DetectEnemies();
            yield return new WaitForSeconds(detectionInterval);
        }
    }

    public void DetectEnemies()
    {
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position,
            this.GetComponent<CircleCollider2D>().radius * this.transform.parent.localScale.x,
             enemyLayer);
        
        DebugLevelControl.Log("DetectEnemies enemiesInRange: " + enemiesInRange.Length,
            DebugLevelControl.DebugModule.Tower,
            DebugLevelControl.LogLevel.Debug);

        tower.enemyUpdate(enemiesInRange);
    }
}
