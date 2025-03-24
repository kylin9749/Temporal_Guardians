using System.Collections;
using UnityEngine;

/// <summary>
/// 防御塔时间增强组件
/// </summary>
[RequireComponent(typeof(towerCommon))]
public class TowerTimeEnhancer : MonoBehaviour
{
    // 引用
    private towerCommon _tower;
    private TimeManager _timeManager;
    
    // 增强状态
    private bool _isEnhanced = false;
    private bool _hasFrenzyTriggered = false;
    
    // 视觉效果
    [SerializeField] private GameObject _dayEnhanceEffect;
    [SerializeField] private GameObject _nightEnhanceEffect;
    [SerializeField] private GameObject _frenzyEffect;
    
    private void Awake()
    {
        _tower = GetComponent<towerCommon>();
    }
    
    private void Start()
    {
        // 获取时间管理器实例
        _timeManager = TimeManager.Instance;
        if (_timeManager == null)
        {
            DebugLevelControl.Log(
                "无法获取TimeManager实例，时间增强效果将不可用",
                DebugLevelControl.DebugModule.Tower,
                DebugLevelControl.LogLevel.Error);
            return;
        }
        
        // 初始禁用所有效果
        if (_dayEnhanceEffect != null) _dayEnhanceEffect.SetActive(false);
        if (_nightEnhanceEffect != null) _nightEnhanceEffect.SetActive(false);
        if (_frenzyEffect != null) _frenzyEffect.SetActive(false);
        
        // 订阅时间事件
        _timeManager.OnTimeStateChanged += HandleTimeStateChanged;
        _timeManager.OnFrenzyTimeReached += CheckFrenzyTime;
        
        // 注册狂暴时间
        RegisterFrenzyTime();
        
        // 立即检查当前状态
        if (_timeManager.CurrentTimeState != TimeState.Day && _timeManager.CurrentTimeState != TimeState.Night)
        {
            // 如果当前是过渡状态，稍后再检查
            StartCoroutine(DelayedTimeCheck());
        }
        else
        {
            // 直接检查当前状态
            HandleTimeStateChanged(_timeManager.CurrentTimeState);
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (_timeManager != null)
        {
            _timeManager.OnTimeStateChanged -= HandleTimeStateChanged;
            _timeManager.OnFrenzyTimeReached -= CheckFrenzyTime;
        }
    }
    
    /// <summary>
    /// 延迟检查时间状态
    /// </summary>
    private IEnumerator DelayedTimeCheck()
    {
        yield return new WaitForSeconds(1f);
        if (_timeManager != null)
        {
            HandleTimeStateChanged(_timeManager.CurrentTimeState);
        }
    }
    
    /// <summary>
    /// 处理时间状态变化
    /// </summary>
    private void HandleTimeStateChanged(TimeState newState)
    {
        if (_tower == null || _tower.TowerData == null || _tower.TowerData.timeEffect == null)
            return;
            
        TowerTimeEffect timeEffect = _tower.TowerData.timeEffect;
        bool shouldBeEnhanced = false;
        
        // 根据时间状态和防御塔配置判断是否应该增强
        if (timeEffect.isDayTimeEnhanced)
        {
            // 白天增强的防御塔
            shouldBeEnhanced = (newState == TimeState.Day);
        }
        else
        {
            // 夜晚增强的防御塔
            shouldBeEnhanced = (newState == TimeState.Night);
        }
        
        // 应用增强效果
        if (shouldBeEnhanced && !_isEnhanced)
        {
            ApplyEnhancement(true);
        }
        else if (!shouldBeEnhanced && _isEnhanced)
        {
            ApplyEnhancement(false);
        }
    }
    
    /// <summary>
    /// 应用/移除增强效果
    /// </summary>
    private void ApplyEnhancement(bool enhance)
    {
        if (_tower == null || _tower.TowerData == null || _tower.TowerData.timeEffect == null)
            return;
            
        TowerTimeEffect timeEffect = _tower.TowerData.timeEffect;
        
        if (enhance)
        {
            // 应用增强效果
            _tower.DmageIncreaseFactor = (_tower.TowerData.damage * timeEffect.enhancedDamageMultiplier) - _tower.TowerData.damage;
            _tower.AttackSpeedFactor = timeEffect.enhancedSpeedMultiplier;
            
            // 显示视觉效果
            if (timeEffect.isDayTimeEnhanced && _dayEnhanceEffect != null)
            {
                _dayEnhanceEffect.SetActive(true);
            }
            else if (!timeEffect.isDayTimeEnhanced && _nightEnhanceEffect != null)
            {
                _nightEnhanceEffect.SetActive(true);
            }
            
            DebugLevelControl.Log(
                $"{_tower.TowerData.towerName} 已激活{(timeEffect.isDayTimeEnhanced ? "白天" : "夜晚")}增强效果",
                DebugLevelControl.DebugModule.Tower,
                DebugLevelControl.LogLevel.Info);
        }
        else
        {
            // 移除增强效果
            _tower.DmageIncreaseFactor = 0;
            _tower.AttackSpeedFactor = 1.0f;
            
            // 移除视觉效果
            if (_dayEnhanceEffect != null) _dayEnhanceEffect.SetActive(false);
            if (_nightEnhanceEffect != null) _nightEnhanceEffect.SetActive(false);
            
            DebugLevelControl.Log(
                $"{_tower.TowerData.towerName} 已移除时间增强效果",
                DebugLevelControl.DebugModule.Tower,
                DebugLevelControl.LogLevel.Info);
        }
        
        _isEnhanced = enhance;
    }
    
    /// <summary>
    /// 注册狂暴时间
    /// </summary>
    private void RegisterFrenzyTime()
    {
        if (_tower == null || _tower.TowerData == null || _tower.TowerData.timeEffect == null || _timeManager == null)
            return;
            
        TowerTimeEffect timeEffect = _tower.TowerData.timeEffect;
        _timeManager.RegisterFrenzyTime(timeEffect.frenzyHour, timeEffect.frenzyMinute);
        
        DebugLevelControl.Log(
            $"{_tower.TowerData.towerName} 注册狂暴时间: {timeEffect.frenzyHour:D2}:{timeEffect.frenzyMinute:D2}",
            DebugLevelControl.DebugModule.Tower,
            DebugLevelControl.LogLevel.Info);
    }
    
    /// <summary>
    /// 检查是否到达狂暴时间
    /// </summary>
    private void CheckFrenzyTime(int hour, int minute)
    {
        if (_tower == null || _tower.TowerData == null || _tower.TowerData.timeEffect == null)
            return;
            
        TowerTimeEffect timeEffect = _tower.TowerData.timeEffect;
        
        // 检查是否是当前防御塔的狂暴时间
        if (timeEffect.frenzyHour == hour && timeEffect.frenzyMinute == minute && !_hasFrenzyTriggered)
        {
            TriggerFrenzySkill();
        }
    }
    
    /// <summary>
    /// 触发狂暴技能
    /// </summary>
    private void TriggerFrenzySkill()
    {
        if (_tower == null || _tower.TowerData == null)
            return;
            
        DebugLevelControl.Log(
            $"{_tower.TowerData.towerName} 触发狂暴技能!",
            DebugLevelControl.DebugModule.Tower,
            DebugLevelControl.LogLevel.Info);
            
        // 显示狂暴效果
        if (_frenzyEffect != null)
        {
            _frenzyEffect.SetActive(true);
            // 3秒后关闭效果
            StartCoroutine(HideFrenzyEffect(3f));
        }
        
        // 根据防御塔类型触发不同的狂暴技能
        ExecuteFrenzySkill();
        
        // 标记为已触发
        _hasFrenzyTriggered = true;
    }
    
    /// <summary>
    /// 根据防御塔类型执行相应的狂暴技能
    /// </summary>
    private void ExecuteFrenzySkill()
    {
        if (_tower == null || _tower.TowerData == null)
            return;
            
        // 获取防御塔类型
        TowerType towerType = _tower.TowerData.towerType;
        
        switch (towerType)
        {
            case TowerType.Sleep:
                ExecuteSleepFrenzySkill();
                break;
            case TowerType.Boom:
                ExecuteBoomFrenzySkill();
                break;
            case TowerType.AttackSpeedIncrease:
                ExecuteAttackSpeedFrenzySkill();
                break;
            default:
                DebugLevelControl.Log(
                    $"未找到 {towerType} 类型的狂暴技能实现",
                    DebugLevelControl.DebugModule.Tower,
                    DebugLevelControl.LogLevel.Warning);
                break;
        }
    }
    
    /// <summary>
    /// 执行睡眠塔的狂暴技能 - 全场的怪物睡眠3s
    /// </summary>
    private void ExecuteSleepFrenzySkill()
    {
        Debug.Log("执行睡眠塔的狂暴技能 - 全场的怪物睡眠3s");
        // // 获取场景中所有敌人
        // GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        // List<GameObject> enemyList = new List<GameObject>(allEnemies);
        
        // // 将所有敌人添加到临时列表
        // List<GameObject> affectedEnemies = new List<GameObject>();
        // foreach (GameObject enemy in enemyList)
        // {
        //     if (enemy != null)
        //     {
        //         affectedEnemies.Add(enemy);
                
        //         // 停止敌人移动
        //         EnemyCommon enemyScript = enemy.GetComponent<EnemyCommon>();
        //         if (enemyScript != null)
        //         {
        //             enemyScript.Stop();
        //         }
        //     }
        // }
        
        // // 3秒后恢复
        // StartCoroutine(WakeAllEnemies(affectedEnemies, 3f));
    }
    
    /// <summary>
    /// 执行炸弹塔的狂暴技能 - 随机向5只怪物投放定时炸弹
    /// </summary>
    private void ExecuteBoomFrenzySkill()
    {
        Debug.Log("执行炸弹塔的狂暴技能 - 随机向5只怪物投放定时炸弹");
        // // 获取场景中所有敌人
        // GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        // List<GameObject> enemyList = new List<GameObject>(allEnemies);
        
        // // 随机选择最多5个敌人
        // int bombCount = Mathf.Min(5, enemyList.Count);
        // for (int i = 0; i < bombCount; i++)
        // {
        //     if (enemyList.Count > 0)
        //     {
        //         // 随机选择一个敌人
        //         int randomIndex = UnityEngine.Random.Range(0, enemyList.Count);
        //         GameObject target = enemyList[randomIndex];
        //         enemyList.RemoveAt(randomIndex);
                
        //         // 创建炸弹
        //         GameObject bombPrefab = Resources.Load<GameObject>("Bullet/Boom");
        //         if (bombPrefab != null)
        //         {
        //             GameObject bomb = Instantiate(bombPrefab);
        //             bomb.transform.position = target.transform.position;
                    
        //             // 将炸弹设置为目标的子物体
        //             bomb.transform.SetParent(target.transform);
        //             bomb.transform.localPosition = Vector3.zero;
                    
        //             // 添加爆炸组件
        //             BombEffect bombEffect = bomb.AddComponent<BombEffect>();
        //             bombEffect.damage = _tower.TowerData.skillDamage;
        //             bombEffect.explosionRange = _tower.TowerData.skillRange;
        //             bombEffect.explosionDelay = _tower.TowerData.skillCastTime;
        //             bombEffect.tower = _tower;
        //         }
        //     }
        // }
    }
    
    /// <summary>
    /// 执行攻速塔的狂暴技能 - 场上所有防御塔攻速翻倍，持续3s
    /// </summary>
    private void ExecuteAttackSpeedFrenzySkill()
    {
        Debug.Log("执行攻速塔的狂暴技能 - 场上所有防御塔攻速翻倍，持续3s");
        // // 获取场景中所有防御塔
        // GameObject[] allTowers = GameObject.FindGameObjectsWithTag("Tower");
        
        // // 为所有防御塔添加攻速加成
        // foreach (GameObject tower in allTowers)
        // {
        //     towerCommon towerScript = tower.GetComponent<towerCommon>();
        //     if (towerScript != null)
        //     {
        //         towerScript.AttackSpeedFactor = 2.0f;
        //     }
        // }
        
        // // 3秒后恢复
        // StartCoroutine(ResetAllTowersAttackSpeed(allTowers, 3f));
    }
    
    /// <summary>
    /// 3秒后唤醒所有敌人
    /// </summary>
    // private IEnumerator WakeAllEnemies(List<GameObject> enemies, float delay)
    // {
    //     yield return new WaitForSeconds(delay);
        
    //     // 恢复所有敌人移动
    //     foreach (GameObject enemy in enemies)
    //     {
    //         if (enemy != null)
    //         {
    //             EnemyCommon enemyScript = enemy.GetComponent<EnemyCommon>();
    //             if (enemyScript != null)
    //             {
    //                 enemyScript.Resume();
    //             }
    //         }
    //     }
    // }
    
    /// <summary>
    /// 重置所有防御塔的攻速
    /// </summary>
    private IEnumerator ResetAllTowersAttackSpeed(GameObject[] towers, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 恢复所有防御塔的攻速
        foreach (GameObject tower in towers)
        {
            if (tower != null)
            {
                towerCommon towerScript = tower.GetComponent<towerCommon>();
                if (towerScript != null)
                {
                    towerScript.AttackSpeedFactor = 1.0f;
                }
            }
        }
    }
    
    /// <summary>
    /// 隐藏狂暴效果
    /// </summary>
    private IEnumerator HideFrenzyEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_frenzyEffect != null)
        {
            _frenzyEffect.SetActive(false);
        }
    }
}