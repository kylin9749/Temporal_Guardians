using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechaClockControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject minuteHand;
    public GameObject hourHand;
    public float timeScale = 60f;
    private float timeOffset = 0f;
    
    // 添加表盘中心点引用
    public Transform clockCenter;
    
    // 添加更新间隔控制
    private float updateInterval = 0.1f; // 每0.1秒更新一次
    private float nextUpdateTime = 0f;

    // 添加新的变量来存储重叠的防御塔
    private List<towerCommon> overlappingTowers = new List<towerCommon>();
    
    void Start()
    {
        timeOffset = Time.time;
        Debug.Log($"timeOffset: {timeOffset}");
        
        // 如果没有指定表盘中心，就使用当前物体的位置作为中心
        if (clockCenter == null)
        {
            clockCenter = this.transform;
        }
        
        // 将指针设置为表盘中心的子对象
        minuteHand.transform.SetParent(clockCenter);
        hourHand.transform.SetParent(clockCenter);
        
        // 确保时针和分针都有触发器类型的碰撞体
        SetupCollider(minuteHand);
        SetupCollider(hourHand);
    }

    // 添加设置碰撞体的辅助方法
    private void SetupCollider(GameObject hand)
    {
        BoxCollider2D collider = hand.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    // 添加碰撞检测方法
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D tag=" + other.gameObject.tag);
        if (other.gameObject.CompareTag("towers"))
        {
            towerCommon tower = other.GetComponent<towerCommon>();
            if (tower != null && !overlappingTowers.Contains(tower))
            {
                overlappingTowers.Add(tower);
                tower.DisableTower();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D tag=" + other.gameObject.tag);
        if (other.gameObject.CompareTag("towers"))
        {
            towerCommon tower = other.GetComponent<towerCommon>();
            if (tower != null)
            {
                // 只有当时针和分针都不与防御塔重叠时才启用防御塔
                if (!IsOverlappingWithHands(tower))
                {
                    overlappingTowers.Remove(tower);
                    tower.EnableTower();
                }
            }
        }
    }
    
    // 检查防御塔是否与任一指针重叠
    private bool IsOverlappingWithHands(towerCommon tower)
    {
        Collider2D towerCollider = tower.GetComponent<Collider2D>();
        if (towerCollider == null) return false;
        
        Collider2D hourCollider = hourHand.GetComponent<Collider2D>();
        Collider2D minuteCollider = minuteHand.GetComponent<Collider2D>();
        
        return Physics2D.IsTouching(towerCollider, hourCollider) || 
               Physics2D.IsTouching(towerCollider, minuteCollider);
    }

    // Update is called once per frame
    void Update()
    {
        // 检查是否需要更新
        if (Time.time >= nextUpdateTime)
        {
            float currentTime = Time.time;
            float gameTime = (currentTime - timeOffset) * timeScale;
            
            float totalMinutes = gameTime / 60f;
            float hours = (totalMinutes / 60f) % 12f;
            float minutes = totalMinutes % 60f;
            
            float minuteAngle = minutes * 6f;
            float hourAngle = hours * 30f + minutes * 0.5f;

            minuteHand.transform.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
            hourHand.transform.localRotation = Quaternion.Euler(0, 0, -hourAngle);

            nextUpdateTime = Time.time + updateInterval;
        }
    }

    
}
