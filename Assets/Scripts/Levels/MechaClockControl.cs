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
    private bool isClockActive = false;
    private float startMinuteAngle;
    private float startHourAngle;
    private float startHours;
    private float startMinutes;
    private BattleController battleController;
    // 添加新的变量来存储重叠的防御塔
    private List<towerCommon> overlappingTowers = new List<towerCommon>();
    
    public void Initialize(BattleController controller)
    {
        battleController = controller;
        // 调用方法更新时钟指针的大小
        UpdateClockHandsSize();

        // 记录当前时间作为时间偏移量
        timeOffset = Time.time;
        
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

        // 设置时钟为不活动状态
        isClockActive = false;

        // 启动协程
        StartCoroutine(UpdateClockHands());
    }

    private void UpdateClockHandsSize()
    {
        //从battleController中获取地图大小
        int x = battleController.GetMapMaker().XColumn;
        int y = battleController.GetMapMaker().YRow;

        // 先取较小的一个值的一半作为分针的半径
        float minuteHandLength = Mathf.Min(x, y) / 2f;
        
        // 时针的长度是分针的一半
        float hourHandLength = minuteHandLength / 2f;

        //设置时针对象中的gameobject的size
        GameObject minuteHandSpriteObject = minuteHand.transform.Find("MinuteHandSprite").gameObject;
        minuteHandSpriteObject.transform.localScale = new Vector3(1f, minuteHandLength, 0f);
        GameObject hourHandSpriteObject = hourHand.transform.Find("HourHandSprite").gameObject;
        hourHandSpriteObject.transform.localScale = new Vector3(1f, hourHandLength, 0f);

        // 设置时针和分针的位置
        minuteHandSpriteObject.transform.localPosition = new Vector3(0f, minuteHandLength / 2f, 0f);
        hourHandSpriteObject.transform.localPosition = new Vector3(0f, hourHandLength / 2f, 0f);

        // 设置时针和分针的碰撞体大小 和 offset
        BoxCollider2D minuteCollider = minuteHand.GetComponent<BoxCollider2D>();
        minuteCollider.size = new Vector2(1f, minuteHandLength);
        minuteCollider.offset = new Vector2(0f, minuteHandLength / 2f);
        BoxCollider2D hourCollider = hourHand.GetComponent<BoxCollider2D>();
        hourCollider.size = new Vector2(1f, hourHandLength);
        hourCollider.offset = new Vector2(0f, hourHandLength / 2f);
    }

    public void SetClockActive(bool active)
    {
        isClockActive = active;
    }

    public void SetClockTime(float timeInSeconds)
    {
        DebugLevelControl.Log("SetClockTime: " + timeInSeconds,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);

        timeOffset = timeInSeconds;

        DebugLevelControl.Log("timeOffset: " + timeOffset,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);

        // 立即更新指针位置
        float gameTime = timeInSeconds;
        DebugLevelControl.Log("gameTime: " + gameTime,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);

        float totalMinutes = gameTime / 60f;
        DebugLevelControl.Log("totalMinutes: " + totalMinutes,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);

        startHours = (totalMinutes / 60f) % 12f;
        startMinutes = totalMinutes % 60f;
        
        startMinuteAngle = startMinutes * 6f;
        startHourAngle = startHours * 30f + startMinutes * 0.5f;

        DebugLevelControl.Log("startMinuteAngle: " + startMinuteAngle,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);
        DebugLevelControl.Log("startHourAngle: " + startHourAngle,
            DebugLevelControl.DebugModule.MechaClock,
            DebugLevelControl.LogLevel.Debug);

        minuteHand.transform.localRotation = Quaternion.Euler(0, 0, -startMinuteAngle);
        hourHand.transform.localRotation = Quaternion.Euler(0, 0, -startHourAngle);
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

    // 使用协程更新指针位置
    private IEnumerator UpdateClockHands()
    {
        while (true)
        {
            if (isClockActive)
            {
                float totalMinutes = timeOffset / 60f;
                float hours = (totalMinutes / 60f) % 12f;
                float minutes = totalMinutes % 60f;

                float minuteAngle = minutes * 6f;
                float hourAngle = hours * 30f;
                DebugLevelControl.Log("minutes: " + minutes + " minuteAngle: " + minuteAngle,
                    DebugLevelControl.DebugModule.MechaClock,
                    DebugLevelControl.LogLevel.Debug);
                DebugLevelControl.Log("hours: " + hours + " hourAngle: " + hourAngle,
                    DebugLevelControl.DebugModule.MechaClock,
                    DebugLevelControl.LogLevel.Debug);

                // 使用动画平滑过渡
                StartCoroutine(AnimateHandRotation(minuteHand, -minuteAngle - startMinuteAngle));
                StartCoroutine(AnimateHandRotation(hourHand, -hourAngle - startHourAngle));

                timeOffset += 30f; // 每秒更新一次，相当于走过了30秒
            }

            yield return new WaitForSeconds(1f); // 每秒更新一次
        }
    }

    // 动画过渡方法
    private IEnumerator AnimateHandRotation(GameObject hand, float targetAngle)
    {
        Quaternion startRotation = hand.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);
        float duration = 1f; // 动画持续时间
        float elapsed = 0f;

        while (elapsed < duration)
        {
            hand.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        hand.transform.localRotation = endRotation;
    }

    public void CleanupResources()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 清理重叠的防御塔列表
        overlappingTowers.Clear();
    
        // 重置状态
        isClockActive = false;

        // 销毁时钟对象
        Destroy(minuteHand);
        Destroy(hourHand);
    }
    private void OnDestroy()
    {
        CleanupResources();
    }
}
