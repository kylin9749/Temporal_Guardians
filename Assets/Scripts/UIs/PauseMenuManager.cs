using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private GameObject settingsPanel;  // 设置面板

    private void Start()
    {
        // 配置暂停菜单项
        ConfigurePauseMenu();
    }

    private void ConfigurePauseMenu()
    {
        var menuItems = pauseMenuUI.GetComponent<PauseMenuUI>().menuItems;
        
        // 继续游戏
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "继续游戏",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[0].onClick.AddListener(ResumeGame);

        // 设置
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "设置",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[1].onClick.AddListener(OpenSettings);

        // 返回主菜单
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "返回主菜单",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[2].onClick.AddListener(ReturnToMainMenu);
    }

    public void ResumeGame()
    {
        BattleController.Instance.TogglePause();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;  // 恢复正常时间流速
        SceneManager.LoadScene("MainMenu");  // 替换为你的主菜单场景名
    }
}