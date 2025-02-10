using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLevelControl : MonoBehaviour
{
    // 使用位运算来控制不同模块的日志
    [System.Flags]
    public enum DebugModule
    {
        None = 0,
        Tower = 1 << 0,
        Monster = 1 << 1,
        BattleController = 1 << 2,
        Economy = 1 << 3,
        MechaClock = 1 << 4,
        DigitalClock = 1 << 5,
        // 可以继续添加更多模块...
    }

    // 当前启用的调试模块
    private static DebugModule activeModules = DebugModule.None;
    
    // 日志等级
    public enum LogLevel
    {
        None,
        Error,
        Warning,
        Debug,
        Info,
    }

    // 默认日志等级
    private static LogLevel currentLogLevel = LogLevel.Warning;

    // 单例实例
    private static DebugLevelControl instance;
    public static DebugLevelControl Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 设置日志等级
    public void SetLogLevel(LogLevel level)
    {
        currentLogLevel = level;
    }

    // 启用指定模块的调试
    public void EnableModule(DebugModule module)
    {
        activeModules |= module;
    }

    // 禁用指定模块的调试
    public void DisableModule(DebugModule module)
    {
        activeModules &= ~module;
    }

    // 检查模块是否启用
    public static bool IsModuleEnabled(DebugModule module)
    {
        return (activeModules & module) != 0;
    }

    // 日志打印方法
    public static void Log(string message, DebugModule module, LogLevel level = LogLevel.Info)
    {
        // Debug.Log("DebugLevelControl.Module: " + module);
        // Debug.Log("DebugLevelControl.Level: " + level);
        // Debug.Log("DebugLevelControl.IsModuleEnabled: " + IsModuleEnabled(module));
        // Debug.Log("DebugLevelControl.IsLevelEnabled: " + (level >= currentLogLevel));
        if (!IsModuleEnabled(module) || level > currentLogLevel)
        {
            return;
        }

        string modulePrefix = $"[{module}]";
        
        switch (level)
        {
            case LogLevel.Error:
                Debug.LogError($"{modulePrefix} {message}");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"{modulePrefix} {message}");
                break;
            default:
                Debug.Log($"{modulePrefix} {message}");
                break;
        }
    }
}
