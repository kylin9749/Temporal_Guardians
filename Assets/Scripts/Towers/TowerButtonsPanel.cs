using System.Collections.Generic;
using UnityEngine;

public class TowerButtonsPanel : MonoBehaviour
{
    [SerializeField] private GameObject towerButtonPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private float buttonSpacing = 10f;
    
    private BattleController battleController;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    
    public void Initialize(BattleController controller, TowerData[] towerConfigs)
    {
        battleController = controller;
        ClearButtons();
        
        // 获取父容器的宽度
        float containerWidth = buttonsContainer.GetComponent<RectTransform>().rect.width;
        
        // 计算起始X位置 - 从父容器的左侧开始
        float leftMargin = 20f; // 左侧边距
        float currentX = -containerWidth/2 + leftMargin; // 容器左边缘 + 边距
        
        // 生成按钮
        foreach (var config in towerConfigs)
        {
            if (config == null) continue;
            
            GameObject buttonObj = Instantiate(towerButtonPrefab, buttonsContainer);
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            
            // 设置按钮位置，从左向右布局
            rectTransform.anchoredPosition = new Vector2(currentX + rectTransform.rect.width/2, 0);
            
            // 初始化按钮
            TowerButton button = buttonObj.GetComponent<TowerButton>();
            button.Initialize(config, battleController);
            
            // 更新下一个按钮的位置
            currentX += rectTransform.rect.width + buttonSpacing;
            
            spawnedButtons.Add(buttonObj);
        }
    }
    
    private void ClearButtons()
    {
        foreach (var button in spawnedButtons)
        {
            Destroy(button);
        }
        spawnedButtons.Clear();
    }
}