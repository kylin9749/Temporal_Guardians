using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletMovementType
{
    Straight,    // 直线移动
    Parabolic,   // 抛物线移动
    // 后续可以添加更多移动方式
    // Spiral,   // 螺旋移动
    // Homing,   // 追踪导弹
    // Wave      // 波浪形
}

public class Bullet : MonoBehaviour
{
    public float speed = 0.5f;        // 子弹速度
    public float damage;              // 子弹伤害
    public Transform target;          // 追踪目标
    
    private BulletMovementType movementType;
    private float parabolicHeight = 0.3f; // 抛物线最大高度
    private Vector3 startPos;          // 起始位置
    private float journeyTime = 0f;    // 飞行时间

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Start()
    {

    }
    
    void Update()
    {
        if (target != null)
        {
            // 移动子弹
            MoveBullet();

            //RotateBullet(transform.position, target.position);

            // 检查是否击中目标
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget < 0.2f) // 可以根据需要调整判定距离
            {
                // 获取敌人脚本并调用扣血函数
                Debug.Log($"击中目标");
                EnemyCommon enemy = target.GetComponent<EnemyCommon>();
                Debug.Log("target.name = " + target.name);
                Debug.Log("target.tag = " + target.tag);
                Debug.Log("target.getComponents = " + target.GetComponents<Component>().Length);
                Debug.Log($"敌人脚本是否为空: {enemy == null}");
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                
                // 销毁子弹
                Destroy(gameObject);
            }
        }
        else
        {
            // 如果目标不存在则销毁子弹
            Destroy(gameObject);

            //表现手法可以是落在地上。
        }
    }

    public void SetMovementType(BulletMovementType type)
    {
        movementType = type;
        if(movementType == BulletMovementType.Parabolic)
        {
            startPos = transform.position;
        }
    }

    // 直线移动
    private void MoveStraight()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    // 计算子弹旋转角度
    private void RotateBullet(Vector3 currentPos, Vector3 nextPos)
    {
        if (target != null)
        {
            // 计算移动方向
            Vector3 direction = (nextPos - currentPos).normalized;
            
            // 计算旋转角度
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // 应用旋转
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // 抛物线移动
    private void MoveParabolic()
    {
        if (target != null)
        {
            journeyTime += Time.deltaTime;
            
            // 计算当前位置到目标的水平距离
            float totalDistance = Vector3.Distance(startPos, target.position);
            float journeyDuration = totalDistance / speed;
            
            // 计算完成度(0-1)
            float progress = journeyTime / journeyDuration;
            
            // 计算当前位置
            Vector3 currentPos = Vector3.Lerp(startPos, target.position, progress);
            
            // 添加抛物线高度
            float height = parabolicHeight * Mathf.Sin(progress * Mathf.PI);
            currentPos.y += height;
            
            // 更新子弹位置
            transform.position = currentPos;
        }
    }

    private void MoveBullet()
    {
        switch (movementType)
        {
            case BulletMovementType.Straight:
                MoveStraight();
                break;
            case BulletMovementType.Parabolic:
                MoveParabolic();
                break;
            // 后续可以添加更多移动方式的case
        }
    }
}
