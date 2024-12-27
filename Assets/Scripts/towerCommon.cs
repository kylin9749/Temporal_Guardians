using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class towerCommon : MonoBehaviour
{
    // 防御塔状态属性
    protected bool enable = false;          // 防御塔是否启用
    protected float currentMp = 0;          // 防御塔当前魔法值
    protected bool isSkilling = false;      // 是否正在释放技能
    protected List<GameObject> enemyList;   // 防御塔攻击范围内的敌人列表
    protected GameObject currentTarget;     // 当前攻击目标
	protected float nextAttackTime = 0f;    // 下一次攻击的时间
    private bool isSettingTower = false;    // 是否设置塔
    private MapGrid nearestBase;            // 离防御塔最近的base类型的格子
    private MapGrid lastNearestBase;        // 当前防御塔所在的格子
    public Transform mpBar = null;          // MP条的Transform组件引用
    private MapGrid currentGrid;            // 当前防御塔所在的格子
    private Transform attackRangeImage;     // 攻击范围图片的Transform组件引用
    private CircleCollider2D attackRangeCollider; // 攻击范围碰撞器的引用
    private float attackSpeedFactor = 1;    // 攻击速度因子
    public GameObject shadow; //阴影

    // 防御塔技能组件
    protected TowerSkillCommon skillComponent;

    // 防御塔数据
    protected TowerData towerData;

    public void InitializeTower(TowerData data)
    {
        // 初始化状态
        currentMp = 0;
        isSkilling = false;
        enable = true;
        
        // 保存数据引用
        towerData = data;

        // 移除现有的技能组件（如果有）
        var oldSkill = GetComponent<TowerSkillCommon>();
        if (oldSkill != null)
            Destroy(oldSkill);

        // 添加新的技能组件
        System.Type skillType = GetSkillType(data.towerType);
        if (skillType != null)
        {
            skillComponent = gameObject.AddComponent(skillType) as TowerSkillCommon;
            if (skillComponent == null)
            {
                Debug.LogError($"Failed to add skill component of type {skillType}");
            }
        }
        else
        {
            Debug.LogError($"Invalid tower type: {data.towerType}");
        }

        // 设置塔的图片
        GetComponent<SpriteRenderer>().sprite = data.towerSprite;

        // 设置塔的攻击范围
        attackRangeCollider = this.transform.Find("AttackRangeController").GetComponent<CircleCollider2D>();
        attackRangeCollider.radius = data.attackRange;
        
        // 设置塔的攻击范围预览图大小(这里是直径，所以*2)
        attackRangeImage = this.transform.Find("AttackRangeImage");
        attackRangeImage.localScale = new Vector3(data.attackRange * 2, data.attackRange * 2, 0);
    }

    public System.Type GetSkillType(TowerType towerType)
    {
        switch (towerType)
        {
            case TowerType.Sleep:
                return typeof(SleepSkill);
            case TowerType.Boom:
                return typeof(BoomSkill);
            case TowerType.AttackSpeedIncrease:
                return typeof(AttackSpeedIncreaseSkill);
            // ... 其他情况
            default:
                return null;
        }
    }

    /// <summary>
    /// Add enemies to the list
    /// </summary>
    /// <param name="newEnemy"></param>
	public void enemyAdd(GameObject newEnemy)
    {
        enemyList.Add(newEnemy);
    }
    /// <summary>
    /// Remove enemies to the list
    /// </summary>
    /// <param name="other"></param>
	public void enemyRemove(string other)
    {
		for(int i = 0; i < enemyList.Count; i++){
			if (enemyList[i] != null)
            {
				if(enemyList[i].name == other)
                {
                    enemyList.RemoveAt(i);
                }
			}
		}
	}

    private void Attack()
    {
        // 检查当前目标是否有效
        if (CurrentTarget != null)
        {
            // 检查当前目标是否还在敌人列表中
            if (!enemyList.Contains(CurrentTarget))
            {
                CurrentTarget = null; // 清空当前目标
            }
        }

        // 如果没有当前目标但敌人列表不为空，选择新目标
        if (CurrentTarget == null && enemyList != null && enemyList.Count > 0)
        {
            CurrentTarget = enemyList[0];
        }

        // 检查是否可以释放技能
        if (currentMp >= towerData.totalMp)
        {
            this.isSkilling = true;
            this.CastSkill();
        }

        // 如果不是普攻加强类型的技能,则跳过后续攻击逻辑
        if (isSkilling && (towerData.skillType != TowerSkillType.AttackBuff))
        {
            return;
        }

        // 有有效目标时发动攻击
        if (CurrentTarget != null && towerData.bulletPrefab != null)
        {
            // 增加MP值
            currentMp += 10;

            // 计算MP百分比
            float mpPercentage = (currentMp + 10) / towerData.totalMp;
            
            // 限制百分比在0-1之间
            mpPercentage = Mathf.Clamp01(mpPercentage);
            
            // 更新MP条的缩放
            if (mpBar != null)
            {
                Vector3 scale = mpBar.localScale;
                scale.x = mpPercentage;
                mpBar.localScale = scale;
            }
            
            // 在防御塔位置生成子弹
            GameObject newBullet = Instantiate(towerData.bulletPrefab, transform.position, Quaternion.identity);
            newBullet.transform.SetParent(transform);

            // 设置子弹追踪目标
            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript == null)
            {
                bulletScript = newBullet.AddComponent<Bullet>();
            }

            // 设置子弹属性
            bulletScript.SetTarget(CurrentTarget.transform);
            bulletScript.damage = towerData.damage;
            bulletScript.speed = towerData.bulletSpeed; // 子弹速度由防御塔定义
            bulletScript.SetMovementType(BulletMovementType.Straight);
        }
    }

    public void Start()
    {
        enemyList = new List<GameObject>();
        currentMp = 0;
        if (mpBar != null)
        {
            Vector3 scale = mpBar.localScale;
            scale.x = 0;
            mpBar.localScale = scale;
        }
    }

    public void Update()
    {
        if (!isSettingTower)
        {
            //离鼠标最近的base类型的格子上生成预览图
            nearestBase = MapMaker.Instance.GetNearestBase(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (nearestBase != null)
            {
                // 如果当前防御塔所在的格子与离防御塔最近的格子不一致，则更新预览图 
                if (lastNearestBase != nearestBase)
                {
                    // 将防御塔移动到最近的格子中心位置
                    transform.position = nearestBase.GridObject.transform.position;

                    // 显示当前的攻击范围图片
                    attackRangeImage.gameObject.SetActive(true);

                    lastNearestBase = nearestBase;
                }
            }

            // 如果鼠标左键点击，则设置塔
            if (Input.GetMouseButtonDown(0))
            {
                //将防御塔放置在离防御塔最近的格子上
                transform.position = nearestBase.GridObject.transform.position;

                //隐藏攻击范围图片
                attackRangeImage.gameObject.SetActive(false);

                // 设置塔为已放置
                isSettingTower = true;
                currentGrid = nearestBase;
                currentGrid.Tower = gameObject;
            }

            return;
        }

        if (!enable)
        {
            return;
        }

        if (enemyList != null && enemyList.Count > 0 && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + (1f / (towerData.attackSpeed * attackSpeedFactor));
        }

        // 如果有当前目标，让防御塔朝向目标
        // if (CurrentTarget != null)
        // {
        //     // 计算目标方向
        //     Vector3 direction = CurrentTarget.transform.position - transform.position;
        //     // 计算旋转角度(忽略z轴)
        //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //     // 应用旋转
        //     transform.rotation = Quaternion.Euler(0, 0, angle);
        // }
    }

    public void DisableTower()
    {
        enable = false;
        this.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void EnableTower()
    {
        enable = true;
        this.GetComponent<SpriteRenderer>().enabled = true;
    }

    protected void CastSkill()
    {
        if (skillComponent != null)
        {
            skillComponent.CastSkill();
            currentMp = 0;  // 重置MP
            isSkilling = false;
        }
    }

    /// <summary>
    /// 获取防御塔攻击范围内的敌人列表
    /// </summary>
    /// <returns>敌人列表</returns>
    public List<GameObject> GetEnemyList()
    {
        return enemyList;
    }

    /// <summary>
    /// 设置防御塔攻击范围内的敌人列表
    /// </summary>
    /// <param name="enemies">新的敌人列表</param>
    public void SetEnemyList(List<GameObject> enemies)
    {
        enemyList = enemies;
    }

    // 为protected属性添加get和set方法
    public float CurrentMp
    {
        get { return currentMp; }
        set { currentMp = value; }
    }

    public bool IsSkilling
    {
        get { return isSkilling; }
        set { isSkilling = value; }
    }

    public GameObject CurrentTarget
    {
        get { return currentTarget; }
        set { currentTarget = value; }
    }

    public float NextAttackTime
    {
        get { return nextAttackTime; }
        set { nextAttackTime = value; }
    }

    public TowerData TowerData
    {
        get { return towerData; }
        set { towerData = value; }
    }

    public MapGrid CurrentGrid
    {
        get { return currentGrid; }
        set { currentGrid = value; }
    }

    public float AttackSpeedFactor
    {
        get { return attackSpeedFactor; }
        set { attackSpeedFactor = value; }
    }
}
