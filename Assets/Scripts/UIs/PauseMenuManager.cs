using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private GameObject settingsPanel;  // 设置面板
    private bool isPaused = false;
    private void Start()
    {
        Debug.Log("PauseMenuManager Start");
        // 配置暂停菜单项
        ConfigurePauseMenu();
    }
    private void ConfigurePauseMenu()
    {
        var menuItems = pauseMenuUI.GetComponent<PauseMenuUI>().menuItems;
        
        // 继续游戏
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "Continue",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[0].onClick.AddListener(ResumeGame);

        // 设置
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "Settings",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[1].onClick.AddListener(OpenSettings);

        // 返回主菜单
        menuItems.Add(new PauseMenuUI.PauseMenuItem
        {
            name = "Back to Main Menu",
            isActive = true,
            onClick = new UnityEngine.Events.UnityEvent()
        });
        menuItems[2].onClick.AddListener(ReturnToMainMenu);
    }
    public void ResumeGame()
    {
        TogglePause();
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
        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        // 先清理资源
        BattleController battleController = FindObjectOfType<BattleController>();
        if (battleController != null)
        {
            battleController.CleanupBeforeSceneChange();
        }
        
        Time.timeScale = 1f;
        
        // 异步加载主菜单场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");
        
        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void OnClickPause()
    {
        TogglePause();
    }
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        // 可以在这里触发UI显示
        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }
    }
    private void ShowPauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.Show();
        }
    }
    
    private void HidePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.Hide();
        }
    }
}