using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class towerCommon : MonoBehaviour
{
    // 防御塔状态属性
    protected bool enable = false;          // 防御塔是否启用
    protected float currentMp = 0;          // 防御塔当前魔法值
    protected bool isSkilling = false;      // 是否正在释放技能
    public List<GameObject> enemyList;   // 防御塔攻击范围内的敌人列表
    private object enemyListLock = new object();
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
    private float angle = 0;                // 防御塔朝向
    private float dmageIncreaseFactor = 0;  //临时伤害增加因子

    public GameObject skillShadow;          //临时的技能特效
    public GameObject highLightEffect;      //高亮特效
    public GameObject zoneControl;
    [SerializeField] private GameObject towerCombineConfirmPanel; // 在Inspector中指定Panel预制体
    [SerializeField] private Transform comboTransform;

    // 防御塔技能组件
    protected TowerSkillCommon skillComponent;

    // 防御塔数据
    protected TowerData towerData;

    // 添加提示管理器的引用
    private UITipManager tipManager;

    // 添加UI面板引用
    [SerializeField] private GameObject towerPanel; // 在Inspector中指定Panel预制体
    private GameObject currentPanel; // 当前实例化的面板
    private GameObject currentCombineConfirmPanel;
    private Canvas mainCanvas; // 主Canvas的引用
    private TowerComboState comboState = TowerComboState.Normal;
    private TowerComboGroup currentGroup;
    private Dictionary<string, GameObject> connectionSprites;
    private bool isMouseOver = false;

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

        connectionSprites = new Dictionary<string, GameObject>
        {
            {"North", comboTransform.Find("North").gameObject},
            {"South", comboTransform.Find("South").gameObject},
            {"East", comboTransform.Find("East").gameObject},
            {"West", comboTransform.Find("West").gameObject},
            {"Northeast", comboTransform.Find("Northeast").gameObject},
            {"Northwest", comboTransform.Find("Northwest").gameObject},
            {"Southeast", comboTransform.Find("Southeast").gameObject},
            {"Southwest", comboTransform.Find("Southwest").gameObject}
        };
    }

    private System.Type GetSkillType(TowerType towerType)
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
	public void enemyUpdate(Collider2D[] newEnemies)
    {
        lock (enemyListLock)
        {
            enemyList.Clear();
            foreach (var newEnemy in newEnemies)
            {
                enemyList.Add(newEnemy.gameObject);
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

        zoneControl.GetComponent<zoneControl>().DetectEnemies();

        // 如果没有当前目标但敌人列表不为空，选择新目标
        if (CurrentTarget == null && enemyList != null && enemyList.Count > 0)
        {
            //按照离中心点的距离排序，优先攻击离中心点最近的敌人
            enemyList.Sort((a, b) =>
            {
                float distanceA = a.GetComponent<EnemyCommon>().DistanceToCenter;
                float distanceB = b.GetComponent<EnemyCommon>().DistanceToCenter;
                return distanceA.CompareTo(distanceB);
            });
            CurrentTarget = enemyList[0];
        }

        // 有有效目标时发动攻击
        if (CurrentTarget != null && towerData.bulletPrefab != null)
        {
            if (isSkilling && towerData.skillType == TowerSkillType.AttackBuff)
            {
                // 释放技能期间不增加mp值
            }
            else
            {
                // 增加MP值
                currentMp += 10;
            }

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
            newBullet.transform.SetParent(transform.parent);

            // 设置子弹追踪目标
            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript == null)
            {
                bulletScript = newBullet.AddComponent<Bullet>();
            }

            // 设置子弹属性
            bulletScript.SetTarget(CurrentTarget.transform);
            bulletScript.damage = towerData.damage + dmageIncreaseFactor;
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
        
        // 获取提示管理器的引用
        tipManager = UITipManager.Instance;
    }

    public void Update()
    {
        if (!isSettingTower)
        {
            HandleTowerPlacement();
            return;
        }

        // 检测防御塔信息面板的点击事件
        CheckTowerInfoPanelInput();

        if (!enable)
        {
            return;
        }
        
        // 检查是否可以释放技能
        if (currentMp >= towerData.totalMp)
        {
            this.isSkilling = true;
            this.CastSkill();
        }
        
        // 如果不是普攻加攻击buff类型的技能,则跳过后续攻击逻辑
        if (isSkilling && (towerData.skillType != TowerSkillType.AttackBuff))
        {
            return;
        }
        
        if (enemyList != null && enemyList.Count > 0 && Time.time >= nextAttackTime)
        {
            Attack();

            nextAttackTime = Time.time + (1f / (towerData.attackSpeed * attackSpeedFactor));
        }

        //如果有当前目标，让防御塔朝向目标
        if (CurrentTarget != null)
        {
            // 计算目标方向
            Vector3 direction = CurrentTarget.transform.position - transform.position;
            // 计算旋转角度
            angle = -(Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            // 应用旋转
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

#region 防御塔放置相关
    // 处理防御塔放置相关的逻辑
    private void HandleTowerPlacement()
    {
        UpdateTowerPreview();
        
        // 处理鼠标输入
        if (Input.GetMouseButtonDown(1))
        {
            CancelTowerPlacement();
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceTower();
        }
    }

    // 更新防御塔预览位置和显示
    private void UpdateTowerPreview()
    {
        nearestBase = BattleController.Instance.GetMapMaker().GetNearestBase(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (nearestBase != null && lastNearestBase != nearestBase)
        {
            transform.position = nearestBase.transform.position;
            attackRangeImage.gameObject.SetActive(true);
            lastNearestBase = nearestBase;
        }
    }

    // 取消防御塔放置
    private void CancelTowerPlacement()
    {
        Destroy(gameObject);
    }

    // 尝试放置防御塔
    private void TryPlaceTower()
    {
        if (!CanAffordTower())
        {
            ShowCannotAffordTip();
            return;
        }

        if (IsGridOccupied())
        {
            ShowGridOccupiedTip();
            return;
        }

        PlaceTower();
    }

    // 检查是否有足够的金钱
    private bool CanAffordTower()
    {
        return BattleController.Instance.GetMoney() >= towerData.cost;
    }

    // 检查格子是否被占用
    private bool IsGridOccupied()
    {
        return nearestBase.Tower != null;
    }

    // 显示金钱不足提示
    private void ShowCannotAffordTip()
    {
        if (tipManager != null)
        {
            tipManager.ShowTip("金币不足，无法建造");
        }
    }

    // 显示格子被占用提示
    private void ShowGridOccupiedTip()
    {
        if (tipManager != null)
        {
            tipManager.ShowTip("该位置已有防御塔，无法放置");
        }
    }

    // 执行防御塔放置
    private void PlaceTower()
    {
        transform.position = nearestBase.transform.position;
        attackRangeImage.gameObject.SetActive(false);
        
        isSettingTower = true;
        currentGrid = nearestBase;
        currentGrid.Tower = gameObject;
        
        BattleController.Instance.UpdateMoney(-towerData.cost);
        /* BattleController.Instance.GetTowerComboControl().CheckCombo(this); */

        // 获取主Canvas引用
        mainCanvas = GameObject.FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Cannot find main Canvas!");
        }
    }
#endregion

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
            // 检查是否有管理器
            if (SkillCutsceneManager.Instance != null)
            {
                // 播放特写，结束后再实际释放技能
                SkillCutsceneManager.Instance.PlaySkillCutscene(towerData, transform.position, () => {
                    skillComponent.CastSkill();
                });
            }
            else
            {
                // 没有管理器就直接释放技能
                skillComponent.CastSkill();
            }
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

#region 防御塔信息面板
    [SerializeField] private GameObject towerInfoPanelPrefab; // 在Inspector中指定面板预制体
    private GameObject currentInfoPanel; // 当前实例化的信息面板
    // 将这些变量改为GameObject类型

    // 处理防御塔点击事件
    private void HandleTowerClick()
    {
        // 如果不在设置塔状态，则返回
        if (!isSettingTower) return;
        
        // 如果鼠标左键被按下
        if (Input.GetMouseButtonDown(0))
        {
            // 如果鼠标指针在UI上，则返回
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 获取鼠标位置
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 检测点击
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            // 如果点击到了防御塔
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                ShowTowerInfoPanel();
            }
            else
            {
                HideTowerInfoPanel();
            }
        }
    }

    // 显示防御塔信息面板
    private void ShowTowerInfoPanel()
    {
        // 如果面板已经存在，直接返回
        if (currentInfoPanel != null) return;
        
        // 确保有Canvas引用
        if (mainCanvas == null)
        {
            mainCanvas = GameObject.FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("找不到主Canvas!");
                return;
            }
        }

        // 实例化信息面板
        currentInfoPanel = Instantiate(towerInfoPanelPrefab, mainCanvas.transform);
        
        // 填充防御塔信息
        FillTowerInfo(currentInfoPanel);
    }

    // 填充防御塔信息到面板
    private void FillTowerInfo(GameObject panel)
    {
        if (panel == null || towerData == null) return;

        // 通过Find查找并获取组件
        TextMeshProUGUI nameText = panel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        Image iconImage = panel.transform.Find("IconImage").GetComponent<Image>();
        TextMeshProUGUI damageText = panel.transform.Find("DamageText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI attackSpeedText = panel.transform.Find("AttackSpeedText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI mpText = panel.transform.Find("MpText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rangeText = panel.transform.Find("RangeText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = panel.transform.Find("CostText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI skillNameText = panel.transform.Find("SkillNameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI skillDescText = panel.transform.Find("SkillDescText").GetComponent<TextMeshProUGUI>();
        
        // 设置防御塔名称
        nameText.text = towerData.towerName;
        
        // 设置防御塔图标
        iconImage.sprite = towerData.towerSprite;
        
        // 设置攻击伤害
        damageText.text = "Damage: " + towerData.damage;
        
        // 设置攻击速度
        attackSpeedText.text = "Attack Speed: " + towerData.attackSpeed;
        
        // 设置总蓝量
        mpText.text = "Total MP: " + towerData.totalMp;
        
        // 设置攻击范围
        rangeText.text = "Attack Range: " + towerData.attackRange;
        
        // 设置建造费用
        costText.text = "Cost: " + towerData.cost;  
        
        // 设置技能名称
        skillNameText.text = "Skill: " + towerData.skillName;
        
        // 设置技能描述
        skillDescText.text = towerData.skillDescription;
    }

    // 隐藏防御塔信息面板
    private void HideTowerInfoPanel()
    {
        if (currentInfoPanel != null)
        {
            Destroy(currentInfoPanel);
            currentInfoPanel = null;
        }
    }

    // 在Update中调用处理点击事件的方法
    public void CheckTowerInfoPanelInput()
    {
        HandleTowerClick();
    }
#endregion

#region 防御塔面板
/*
    private bool CheckMouseHit()
    {
        // 获取鼠标在世界坐标中的位置
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // 使用OverlapPoint直接检测点击位置
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        
        // 或者使用小范围的圆形检测，提供更好的点击体验
        // Collider2D hitCollider = Physics2D.OverlapCircle(mousePosition, 0.1f);

        return hitCollider != null && hitCollider.gameObject == gameObject;
    }
    // 处理塔点击事件
    private void HandleTowerClick()
    {
        // 如果不在设置塔状态，则返回
        if (!isSettingTower) return;
        
        DebugLevelControl.Log("HandleTowerClick Input.GetMouseButtonDown(0) = " + Input.GetMouseButtonDown(0).ToString(),
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Info);
        DebugLevelControl.Log("HandleTowerClick comboState = " + comboState.ToString(),
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Info);
        // 如果鼠标左键被按下或者组合状态为正在选择
        if (Input.GetMouseButtonDown(0) || comboState == TowerComboState.BeingSelected)
        {
            // 如果鼠标指针在UI上，则返回
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 获取鼠标位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // 发射射线
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // 如果射线击中物体，并且物体是当前物体
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                DebugLevelControl.Log("HandleTowerClick HIT ",
                    DebugLevelControl.DebugModule.TowerCombo,
                    DebugLevelControl.LogLevel.Debug);

                // 如果组合状态为正在选择，则显示加入组确认面板
                if (comboState == TowerComboState.BeingSelected && Input.GetMouseButtonDown(0))
                {
                    // 显示加入组确认面板
                    ShowJoinGroupConfirmPanel(currentGroup);
                }
                // 否则，显示塔面板
                else if (Input.GetMouseButtonDown(0))
                {
                    ShowTowerPanel();
                }
                else if (comboState == TowerComboState.BeingSelected)
                {
                    if (!isMouseOver)  // 添加状态标记避免重复调用
                    {
                        isMouseOver = true;
                        BattleController.Instance.GetTowerComboControl().OnGroupHover(currentGroup);
                    }
                }
                
            }
            // 如果当前面板不为空，则隐藏塔面板
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HideTowerPanel();
                }
                else if (comboState == TowerComboState.BeingSelected)
                {
                    isMouseOver = false;
                    BattleController.Instance.GetTowerComboControl().OnGroupHoverExit();
                }
            }
        }
    }
    // 显示面板
    private void ShowTowerPanel()
    {
        // 如果面板已经存在，直接返回
        if (currentPanel != null) return;

        // 实例化面板
        currentPanel = Instantiate(towerPanel, mainCanvas.transform);
        
        // 设置面板位置（可以适当偏移，避免遮挡防御塔）
        currentPanel.transform.position = transform.position + new Vector3(0, 1f, 0);
        
        // 可以在这里设置面板中按钮的点击事件
        SetupPanelButtons();
    }

    // 隐藏面板
    private void HideTowerPanel()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
    }

    // 设置面板按钮的点击事件
    private void SetupPanelButtons()
    {
        if (currentPanel == null) return;

        // 获取按钮引用并添加监听器
        Button upgradeBtn = currentPanel.transform.Find("UpgradeButton").GetComponent<Button>();
        Button repairBtn = currentPanel.transform.Find("RepairButton").GetComponent<Button>();
        Button combineBtn = currentPanel.transform.Find("CombineButton").GetComponent<Button>();

        upgradeBtn.onClick.AddListener(() => OnUpgradeClick());
        repairBtn.onClick.AddListener(() => OnRepairClick());
        combineBtn.onClick.AddListener(() => OnCombineClick());
    }

    // 按钮点击回调函数
    private void OnUpgradeClick()
    {
        // 实现升级逻辑
        Debug.Log("Upgrade clicked");
    }

    private void OnRepairClick()
    {
        // 实现维修逻辑
        Debug.Log("Repair clicked");
    }

    private void OnCombineClick()
    {
        DebugLevelControl.Log("Enter OnCombineClick() currentGroup = " + currentGroup,
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Debug);
        HideTowerPanel();

        // 检查是否在组中
        if (currentGroup != null)
        {
            // 已在组中，显示退出组确认面板
            ShowLeaveGroupConfirmPanel();
        }
        else
        {
            // 进入组合模式
            EnterCombineMode();
        }
    }
*/
#endregion

#region 防御塔联动
/*
    public void SetHighlight(bool highlight)
    {
        if (highlight)
        {
            highLightEffect.SetActive(true);
        }
        else
        {
            highLightEffect.SetActive(false);
        }
    }
    
    public void setCurrentGroup(TowerComboGroup group)
    {
        currentGroup = group;
    }

    public void CreateConnectionVisuals()
    {
        if (currentGroup == null) return;

        foreach (var otherTower in currentGroup.GetTowers())
        {
            if (otherTower == this) continue;

            // 检查是否相邻
            if (IsAdjacent(this, otherTower))
            {
                // 获取相对方向
                Vector2Int direction = GetDirection(this, otherTower);
                
                // 激活对应的连接线
                ActivateConnection(this, otherTower, direction);
            }
        }
    }

    // 检查两个防御塔是否相邻
    public bool IsAdjacent(towerCommon tower1, towerCommon tower2)
    {
        MapGrid grid1 = tower1.CurrentGrid;
        MapGrid grid2 = tower2.CurrentGrid;

        DebugLevelControl.Log("grid1 = " + grid1 + ", grid2 = " + grid2,
            DebugLevelControl.DebugModule.TowerCombo,
            DebugLevelControl.LogLevel.Debug);

        if (grid1 == null || grid2 == null)
        {
            return false;
        }

        int dx = Mathf.Abs(grid1.X - grid2.X);
        int dy = Mathf.Abs(grid2.Y - grid1.Y);
        
        // 横、竖、斜方向均视为相邻
        return dx <= 2 && dy <= 2 && !(dx == 0 && dy == 0);
    }
    public void DisableConnectionVisuals()
    {
        // 禁用所有连接线
        foreach (var sprite in connectionSprites.Values)
        {
            sprite.SetActive(false);
        }

        // 如果在组中，需要处理相邻防御塔的连接线
        if (currentGroup != null)
        {
            foreach (var otherTower in currentGroup.GetTowers())
            {
                if (otherTower == this) continue;

                if (IsAdjacent(this, otherTower))
                {
                    // 获取相对方向
                    Vector2Int direction = GetDirection(this, otherTower);
                    
                    // 禁用对方对应的连接线
                    DisableConnection(otherTower, GetOppositeDirection(direction));
                }
            }
        }
    }
    private Vector2Int GetDirection(towerCommon from, towerCommon to)
    {
        int dx = to.CurrentGrid.X - from.CurrentGrid.X;
        int dy = to.CurrentGrid.Y - from.CurrentGrid.Y;
        return new Vector2Int(dx, dy);
    }

    private void ActivateConnection(towerCommon tower1, towerCommon tower2, Vector2Int direction)
    {
        // 激活当前防御塔的连接线
        string direction1 = GetDirectionString(direction);
        if (connectionSprites.ContainsKey(direction1))
        {
            connectionSprites[direction1].SetActive(true);
        }

        // 激活目标防御塔的连接线
        string direction2 = GetDirectionString(new Vector2Int(-direction.x, -direction.y));
        if (tower2.connectionSprites.ContainsKey(direction2))
        {
            tower2.connectionSprites[direction2].SetActive(true);
        }
    }

    private void DisableConnection(towerCommon tower, Vector2Int direction)
    {
        string directionString = GetDirectionString(direction);
        if (tower.connectionSprites.ContainsKey(directionString))
        {
            tower.connectionSprites[directionString].SetActive(false);
        }
    }

    private string GetDirectionString(Vector2Int direction)
    {
        if (direction.x == 0 && direction.y == 1) return "North";
        if (direction.x == 0 && direction.y == -1) return "South";
        if (direction.x == 1 && direction.y == 0) return "East";
        if (direction.x == -1 && direction.y == 0) return "West";
        if (direction.x == 1 && direction.y == 1) return "Northeast";
        if (direction.x == -1 && direction.y == 1) return "Northwest";
        if (direction.x == 1 && direction.y == -1) return "Southeast";
        if (direction.x == -1 && direction.y == -1) return "Southwest";
        return "";
    }

    private Vector2Int GetOppositeDirection(Vector2Int direction)
    {
        return new Vector2Int(-direction.x, -direction.y);
    }

    // 进入组合模式
    private void EnterCombineMode()
    {
        // 通知TowerComboControl进入组合模式
        BattleController.Instance.GetTowerComboControl().CheckCombo(this);
    }

    // 显示加入组确认面板
    private void ShowJoinGroupConfirmPanel(TowerComboGroup group)
    {
        if (currentCombineConfirmPanel != null) return;
        
        // 设置面板位置为鼠标位置
        Vector3 mousePosition = Input.mousePosition;
        currentCombineConfirmPanel = Instantiate(towerCombineConfirmPanel, mainCanvas.transform);
        currentCombineConfirmPanel.transform.position = mousePosition;

        // 设置确认和取消按钮的事件
        Button confirmBtn = currentPanel.transform.Find("ConfirmButton").GetComponent<Button>();
        Button cancelBtn = currentPanel.transform.Find("CancelButton").GetComponent<Button>();

        confirmBtn.onClick.AddListener(() => {
            BattleController.Instance.GetTowerComboControl().AddTowerIntoHoverGroup(this);
            HideTowerPanel();
        });

        cancelBtn.onClick.AddListener(() => {
            HideTowerPanel();
        });
    }

    // 显示退出组确认面板
    private void ShowLeaveGroupConfirmPanel()
    {
        if (currentPanel != null) return;

        currentPanel = Instantiate(towerCombineConfirmPanel, mainCanvas.transform);
        RectTransform rectTransform = currentPanel.GetComponent<RectTransform>();
        
        // 设置面板位置为鼠标位置
        Vector2 mousePosition = Input.mousePosition;
        rectTransform.position = mousePosition;

        // 设置确认和取消按钮的事件
        Button confirmBtn = currentPanel.transform.Find("ConfirmButton").GetComponent<Button>();
        Button cancelBtn = currentPanel.transform.Find("CancelButton").GetComponent<Button>();

        confirmBtn.onClick.AddListener(() => {
            BattleController.Instance.GetTowerComboControl().RemoveTowerFromHoverGroup(this);
            HideTowerPanel();
        });

        cancelBtn.onClick.AddListener(() => {
            HideTowerPanel();
        });
    }

    // 设置防御塔状态
    public void SetComboState(TowerComboState state)
    {
        comboState = state;
        switch (state)
        {
            case TowerComboState.Normal:
                SetHighlight(false);
                break;
            case TowerComboState.Selecting:
                SetHighlight(true);
                break;
            case TowerComboState.BeingSelected:
                SetHighlight(false);
                break;
            case TowerComboState.InGroup:
                // 可以设置特殊效果
                break;
        }
    }
*/
#endregion

    private void OnMouseOver()
    {

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

    public float Angle
    {
        get { return angle; }
        set { angle = value; }
    }

    public TowerData _TowerData
    {
        get { return towerData; }
        set { towerData = value; }
    }

    public float DmageIncreaseFactor
    {
        get { return dmageIncreaseFactor; }
        set { dmageIncreaseFactor = value; }
    }
}
