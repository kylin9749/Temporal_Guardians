using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏时间状态枚举
/// </summary>
public enum TimeState
{
    Day,        // 白天
    Night,      // 夜晚
    DayToNight, // 白天到夜晚的过渡
    NightToDay  // 夜晚到白天的过渡
}

/// <summary>
/// 游戏时间管理器
/// </summary>
public class TimeManager : MonoBehaviour
{
    // 单例实例
    private static TimeManager _instance;
    public static TimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("TimeManager实例不存在，请确保已正确初始化");
            }
            return _instance;
        }
    }

    // 时间定义常量
    private const int DAY_START_HOUR = 6;
    private const int NIGHT_START_HOUR = 18;
    private const int TRANSITION_MINUTES = 30; // 过渡期为30分钟

    // 当前游戏时间
    private DateTime _currentTime;
    public DateTime CurrentTime => _currentTime;

    // 当前时间状态
    private TimeState _currentTimeState;
    public TimeState CurrentTimeState => _currentTimeState;

    // 时间流速控制
    private float _timeScale = 30f; // 默认每秒游戏时间流逝30秒
    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Mathf.Max(0.1f, value); // 确保时间流速不会太小
    }

    // 时钟控制器引用
    private MechaClockControl _mechaClockControl;
    private DigitalClockControl _digitalClockControl;
    private bool _clockInitialized = false;
    
    // 事件定义
    public event Action<TimeState> OnTimeStateChanged;
    public event Action<DateTime> OnTimeChanged;
    public event Action<int, int> OnFrenzyTimeReached; // 参数：小时、分钟

    // 上一次检查的狂暴时间
    private Dictionary<string, bool> _frenzyTimeTriggered = new Dictionary<string, bool>();

    // 添加时间是否流逝的控制变量
    private bool _isTimeFlowing = false;
    public bool IsTimeFlowing
    {
        get => _isTimeFlowing;
        set => _isTimeFlowing = value;
    }

    /// <summary>
    /// 初始化时间管理器
    /// </summary>
    /// <param name="startTimeHour">初始小时</param>
    /// <param name="startTimeMinute">初始分钟</param>
    public void Initialize(int startTimeHour, int startTimeMinute = 0)
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 初始化时间
        _currentTime = new DateTime(2024, 1, 1, startTimeHour, startTimeMinute, 0);
        
        // 确定初始时间状态
        UpdateTimeState();
        
        // 初始状态为暂停
        _isTimeFlowing = false;
        
        DebugLevelControl.Log(
            $"时间管理器初始化完成。初始时间：{_currentTime.ToString("HH:mm")}，状态：{_currentTimeState}，时间流逝：暂停",
            DebugLevelControl.DebugModule.TimeManager,
            DebugLevelControl.LogLevel.Info);
            
        // 启动时间更新协程
        StartCoroutine(TimeUpdateRoutine());
    }

    /// <summary>
    /// 设置时钟控制器引用
    /// </summary>
    public void SetClockControls(MechaClockControl mechaClockControl, DigitalClockControl digitalClockControl)
    {
        _mechaClockControl = mechaClockControl;
        _digitalClockControl = digitalClockControl;
        _clockInitialized = true;
        
        // 同步初始时间到时钟
        SyncTimeToClock();
    }

    /// <summary>
    /// 时间更新协程
    /// </summary>
    private IEnumerator TimeUpdateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            // 仅在时间流逝状态下更新时间
            if (_isTimeFlowing)
            {
                // 更新游戏时间
                UpdateGameTime();
                
                // 同步时间到时钟控制器
                if (_clockInitialized)
                {
                    SyncTimeToClock();
                }
                
                // 触发时间变化事件
                OnTimeChanged?.Invoke(_currentTime);
                
                // 检查是否需要更新时间状态
                UpdateTimeState();
                
                // 检查是否到达狂暴时间
                CheckFrenzyTimes();
            }
        }
    }

    /// <summary>
    /// 更新游戏时间
    /// </summary>
    private void UpdateGameTime()
    {
        _currentTime = _currentTime.AddSeconds(_timeScale);
    }

    /// <summary>
    /// 同步时间到时钟控制器
    /// </summary>
    private void SyncTimeToClock()
    {
        float totalSeconds = (_currentTime.Hour * 3600) + (_currentTime.Minute * 60) + _currentTime.Second;
        
        if (_mechaClockControl != null)
        {
            _mechaClockControl.SetClockTime(totalSeconds);
        }
        
        if (_digitalClockControl != null)
        {
            _digitalClockControl.SetClockTime(totalSeconds);
        }
    }

    /// <summary>
    /// 更新时间状态
    /// </summary>
    private void UpdateTimeState()
    {
        TimeState previousState = _currentTimeState;
        
        int hour = _currentTime.Hour;
        int minute = _currentTime.Minute;
        
        // 判断当前时间状态
        if (hour == DAY_START_HOUR - 1 && minute >= 60 - TRANSITION_MINUTES || 
            hour == DAY_START_HOUR && minute < TRANSITION_MINUTES)
        {
            // 夜晚到白天的过渡期
            _currentTimeState = TimeState.NightToDay;
        }
        else if (hour >= DAY_START_HOUR && hour < NIGHT_START_HOUR)
        {
            // 白天
            _currentTimeState = TimeState.Day;
        }
        else if (hour == NIGHT_START_HOUR - 1 && minute >= 60 - TRANSITION_MINUTES || 
                 hour == NIGHT_START_HOUR && minute < TRANSITION_MINUTES)
        {
            // 白天到夜晚的过渡期
            _currentTimeState = TimeState.DayToNight;
        }
        else
        {
            // 夜晚
            _currentTimeState = TimeState.Night;
        }
        
        // 如果状态发生变化，触发事件
        if (previousState != _currentTimeState)
        {
            DebugLevelControl.Log(
                $"时间状态变化：{previousState} -> {_currentTimeState}，当前时间：{_currentTime.ToString("HH:mm")}",
                DebugLevelControl.DebugModule.TimeManager,
                DebugLevelControl.LogLevel.Info);
                
            OnTimeStateChanged?.Invoke(_currentTimeState);
        }
    }

    /// <summary>
    /// 检查是否到达狂暴时间
    /// </summary>
    private void CheckFrenzyTimes()
    {
        // 生成当前时间的键
        string timeKey = $"{_currentTime.Hour}:{_currentTime.Minute}";
        
        // 检查是否已经触发过这个时间点
        if (!_frenzyTimeTriggered.ContainsKey(timeKey))
        {
            _frenzyTimeTriggered[timeKey] = true;
            
            // 触发狂暴时间事件
            DebugLevelControl.Log(
                $"检查狂暴时间点：{timeKey}",
                DebugLevelControl.DebugModule.TimeManager,
                DebugLevelControl.LogLevel.Debug);
                
            OnFrenzyTimeReached?.Invoke(_currentTime.Hour, _currentTime.Minute);
            
            // 清理旧的时间键以防内存泄漏
            CleanupOldTimeKeys();
        }
    }

    /// <summary>
    /// 清理旧的时间键
    /// </summary>
    private void CleanupOldTimeKeys()
    {
        if (_frenzyTimeTriggered.Count > 300) // 保留最近几小时的记录
        {
            _frenzyTimeTriggered.Clear();
            DebugLevelControl.Log(
                "清理狂暴时间触发记录",
                DebugLevelControl.DebugModule.TimeManager,
                DebugLevelControl.LogLevel.Debug);
        }
    }

    /// <summary>
    /// 设置游戏时间
    /// </summary>
    public void SetGameTime(int hour, int minute = 0)
    {
        _currentTime = new DateTime(2024, 1, 1, hour, minute, 0);
        UpdateTimeState();
        
        if (_clockInitialized)
        {
            SyncTimeToClock();
        }
        
        DebugLevelControl.Log(
            $"手动设置游戏时间：{_currentTime.ToString("HH:mm")}",
            DebugLevelControl.DebugModule.TimeManager,
            DebugLevelControl.LogLevel.Info);
            
        OnTimeChanged?.Invoke(_currentTime);
    }

    /// <summary>
    /// 判断当前是否为白天
    /// </summary>
    public bool IsDaytime()
    {
        return _currentTimeState == TimeState.Day || _currentTimeState == TimeState.NightToDay;
    }

    /// <summary>
    /// 判断当前是否为夜晚
    /// </summary>
    public bool IsNighttime()
    {
        return _currentTimeState == TimeState.Night || _currentTimeState == TimeState.DayToNight;
    }

    /// <summary>
    /// 判断当前是否为指定时间(小时和分钟)
    /// </summary>
    public bool IsSpecificTime(int hour, int minute)
    {
        return _currentTime.Hour == hour && _currentTime.Minute == minute;
    }

    /// <summary>
    /// 获取当前时间的格式化字符串
    /// </summary>
    public string GetFormattedTime()
    {
        return _currentTime.ToString("HH:mm");
    }

    /// <summary>
    /// 获取时间状态的过渡百分比(用于视觉过渡效果)
    /// </summary>
    public float GetTransitionPercentage()
    {
        if (_currentTimeState == TimeState.NightToDay)
        {
            // 计算从夜晚到白天的过渡百分比
            if (_currentTime.Hour == DAY_START_HOUR - 1)
            {
                return (_currentTime.Minute - (60 - TRANSITION_MINUTES)) / (float)TRANSITION_MINUTES;
            }
            else // DAY_START_HOUR
            {
                return (TRANSITION_MINUTES + _currentTime.Minute) / (float)(TRANSITION_MINUTES * 2);
            }
        }
        else if (_currentTimeState == TimeState.DayToNight)
        {
            // 计算从白天到夜晚的过渡百分比
            if (_currentTime.Hour == NIGHT_START_HOUR - 1)
            {
                return (_currentTime.Minute - (60 - TRANSITION_MINUTES)) / (float)TRANSITION_MINUTES;
            }
            else // NIGHT_START_HOUR
            {
                return (TRANSITION_MINUTES + _currentTime.Minute) / (float)(TRANSITION_MINUTES * 2);
            }
        }
        
        return 1.0f; // 非过渡状态返回1
    }

    /// <summary>
    /// 注册狂暴时间
    /// </summary>
    public void RegisterFrenzyTime(int hour, int minute)
    {
        DebugLevelControl.Log(
            $"注册狂暴时间点：{hour}:{minute}",
            DebugLevelControl.DebugModule.TimeManager,
            DebugLevelControl.LogLevel.Info);
    }

    /// <summary>
    /// 开始时间流逝
    /// </summary>
    public void StartTimeFlow()
    {
        if (!_isTimeFlowing)
        {
            _isTimeFlowing = true;
            DebugLevelControl.Log(
                "时间开始流逝",
                DebugLevelControl.DebugModule.TimeManager,
                DebugLevelControl.LogLevel.Info);
        }
    }
    
    /// <summary>
    /// 暂停时间流逝
    /// </summary>
    public void PauseTimeFlow()
    {
        if (_isTimeFlowing)
        {
            _isTimeFlowing = false;
            DebugLevelControl.Log(
                "时间流逝已暂停",
                DebugLevelControl.DebugModule.TimeManager,
                DebugLevelControl.LogLevel.Info);
        }
    }
}