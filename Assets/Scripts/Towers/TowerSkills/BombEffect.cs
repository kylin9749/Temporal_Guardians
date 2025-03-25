using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombEffect : MonoBehaviour
{
    public float damage;
    public float explosionRange;
    public float explosionDelay;
    public towerCommon tower;
    private GameObject boomTarget;
    private bool isBoom = false;
    // Start is called before the first frame update
    void Start()
    {
        boomTarget = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //如果防御塔被拆掉，则销毁炸弹
        if (tower == null)
        {
            Destroy(gameObject);
            return;
        }
            
        //如果怪物死亡，则销毁炸弹
        if (boomTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        explosionDelay -= Time.deltaTime;
        if (explosionDelay <= 0)
        {
            if (!isBoom)
            {
                Boom();
            }
        }
    }

    private void Boom()
    {
        // 获取fire子对象
        Transform fireTransform = transform.Find("fire");
        if (fireTransform != null)
        {
            // 激活fire对象使其显示
            fireTransform.gameObject.SetActive(true);
        }
        
        // 获取炸弹的SpriteRenderer组件并隐藏
        SpriteRenderer bombSprite = GetComponent<SpriteRenderer>();
        if (bombSprite != null)
        {
            bombSprite.enabled = false;
        }

        // 添加 LayerMask 来只检测敌人层
        int enemyLayer = LayerMask.GetMask("Enemy");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRange, enemyLayer);
        
        Debug.Log("爆炸范围内的怪物数量:" + colliders.Length);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag("enemies"))
            {
                EnemyCommon enemy = collider.gameObject.GetComponent<EnemyCommon>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        //当前怪物也受到伤害
        if (boomTarget.CompareTag("enemies"))
        {
            EnemyCommon enemy = boomTarget.GetComponent<EnemyCommon>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        isBoom = true;
        // 销毁炸弹
        Destroy(gameObject, 0.5f);
    }
}
