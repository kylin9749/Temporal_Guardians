using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour 
{
    [System.Serializable]
    public class PauseMenuItem
    {
        public string name;           // 按钮名称
        public bool isActive = true;  // 是否启用该按钮
        public UnityEngine.Events.UnityEvent onClick;  // 点击事件
    }

    [SerializeField] private GameObject buttonPrefab;  // 按钮预制体
    [SerializeField] private Transform buttonContainer;  // 按钮容器
    [SerializeField] public List<PauseMenuItem> menuItems = new List<PauseMenuItem>(); // 菜单项列表

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        // 清除现有按钮
        foreach (Transform child in buttonContainer) 
        {
            Destroy(child.gameObject);
        }

        // 创建新按钮
        foreach (var item in menuItems)
        {
            if (!item.isActive) continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // 设置按钮文本
            buttonText.text = item.name;

            // 添加点击事件
            button.onClick.AddListener(() => item.onClick?.Invoke());
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        // 调用BattleController的暂停切换方法
        BattleController.Instance.TogglePause();
    }

    public void ReturnToMainMenu()
    {
        // 在切换场景前恢复正常时间流速
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        SceneManager.LoadScene("LevelScene"); // 这里使用你的关卡选择场景名称
    }
}