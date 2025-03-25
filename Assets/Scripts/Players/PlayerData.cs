using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // 已解锁的防御塔类型列表
    public List<TowerType> unlockedTowers = new List<TowerType>();
    
    // 已通关的关卡
    public List<string> completedLevels = new List<string>();
    
    // 玩家金币
    public int coins = 0;
    
    // 玩家经验值
    public int experience = 0;
    
    // 成就进度等其他数据
    // ...
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public PlayerData playerData = new PlayerData();
    
    // 当前选择的防御塔
    public List<TowerType> selectedTowers = new List<TowerType>();
    
    // 每关可选的最大防御塔数量
    public int maxTowersPerLevel = 8;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 保存玩家数据
    public void SavePlayerData()
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString("PlayerData", json);
        PlayerPrefs.Save();
    }
    
    // 加载玩家数据
    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            string json = PlayerPrefs.GetString("PlayerData");
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            // 初始默认解锁的防御塔
            playerData.unlockedTowers.Add(TowerType.Sleep);
            playerData.unlockedTowers.Add(TowerType.Boom);
            playerData.unlockedTowers.Add(TowerType.AttackSpeedIncrease);
            SavePlayerData();
        }
    }
    
    // 解锁新防御塔
    public void UnlockTower(TowerType towerType)
    {
        if (!playerData.unlockedTowers.Contains(towerType))
        {
            playerData.unlockedTowers.Add(towerType);
            SavePlayerData();
        }
    }
    
    // 设置当前选择的防御塔
    public void SetSelectedTowers(List<TowerType> towers)
    {
        selectedTowers = new List<TowerType>(towers);
    }
    
    // 判断是否需要选择防御塔界面
    public bool NeedTowerSelectionScreen()
    {
        return playerData.unlockedTowers.Count > maxTowersPerLevel;
    }
}