using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSelectionScreen : MonoBehaviour
{
    [SerializeField] private GameObject towerSelectionItemPrefab;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    
    private List<TowerType> selectedTowers = new List<TowerType>();
    private Dictionary<TowerType, Toggle> towerToggles = new Dictionary<TowerType, Toggle>();
    
    private void Start()
    {
        // 初始化界面
        InitializeSelectionScreen();
        
        // 设置开始按钮点击事件
        startButton.onClick.AddListener(OnStartButtonClicked);
        
        // 更新已选数量显示
        UpdateSelectedCountText();
    }
    
    private void InitializeSelectionScreen()
    {
        // 清空容器
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        towerToggles.Clear();
        selectedTowers.Clear();
        
        // 获取所有已解锁的防御塔数据
        List<TowerType> unlockedTowers = PlayerManager.Instance.playerData.unlockedTowers;
        TowerData[] allTowerData = TowerFactory.Instance.towerConfigs;
        
        // 创建选择项
        foreach (TowerType towerType in unlockedTowers)
        {
            TowerData towerData = System.Array.Find(allTowerData, t => t.towerType == towerType);
            if (towerData == null) continue;
            
            // 实例化选择项
            GameObject item = Instantiate(towerSelectionItemPrefab, contentContainer);
            
            // 设置塔信息
            Image towerIcon = item.transform.Find("TowerIcon").GetComponent<Image>();
            TextMeshProUGUI towerName = item.transform.Find("TowerName").GetComponent<TextMeshProUGUI>();
            Toggle toggle = item.GetComponent<Toggle>();
            
            towerIcon.sprite = towerData.towerSprite;
            towerName.text = towerData.towerName;
            
            // 记录Toggle引用
            towerToggles[towerType] = toggle;
            
            // 添加Toggle事件
            int index = selectedTowers.Count; // 捕获当前索引
            toggle.onValueChanged.AddListener((isOn) => {
                OnTowerToggleChanged(towerType, isOn);
            });
        }
    }
    
    private void OnTowerToggleChanged(TowerType towerType, bool isOn)
    {
        if (isOn)
        {
            // 如果已经选择了最大数量的防御塔，禁止选择更多
            if (selectedTowers.Count >= PlayerManager.Instance.maxTowersPerLevel)
            {
                towerToggles[towerType].isOn = false;
                return;
            }
            
            // 添加到已选列表
            if (!selectedTowers.Contains(towerType))
            {
                selectedTowers.Add(towerType);
            }
        }
        else
        {
            // 从已选列表移除
            selectedTowers.Remove(towerType);
        }
        
        // 更新已选数量显示
        UpdateSelectedCountText();
        
        // 更新开始按钮状态
        startButton.interactable = selectedTowers.Count > 0;
    }
    
    private void UpdateSelectedCountText()
    {
        selectedCountText.text = $"已选择: {selectedTowers.Count}/{PlayerManager.Instance.maxTowersPerLevel}";
    }
    
    private void OnStartButtonClicked()
    {
        // 保存选择的防御塔
        PlayerManager.Instance.SetSelectedTowers(selectedTowers);
        
        // 开始游戏
        StartLevel();
    }
    
    private void StartLevel()
    {
        // 隐藏选择界面
        gameObject.SetActive(false);
        
        // 通知游戏开始
        FindObjectOfType<BattleController>().StartLevelAfterTowerSelection();
    }
}