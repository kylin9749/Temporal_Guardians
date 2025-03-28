using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 初始化代码
        Debug.Log("MainMenuController Start");
    }

    // Update is called once per frame
    void Update()
    {
        // 更新代码
    }

    // 新的开始按钮点击事件
    public void OnNewGameButtonClicked()
    {
        // 加载新游戏场景
        Debug.Log("OnNewGameButtonClicked");
        SceneManager.LoadScene("ChapterScene");
    }

    // 读取存档按钮点击事件
    public void OnLoadGameButtonClicked()
    {
        // 加载存档场景或功能
        SceneManager.LoadScene("LoadGameScene");
    }

    // 设置按钮点击事件
    public void OnSettingsButtonClicked()
    {
        // 加载设置场景或功能
        SceneManager.LoadScene("SettingsScene");
    }

    // 退出游戏按钮点击事件
    public void OnExitButtonClicked()
    {
        // 保存并清理数据
        PlayerManager.Cleanup();
        
        // 退出游戏
        Application.Quit();
    }

    public void OnIllustratedButtonClicked()
    {
        // 加载图鉴场景
        SceneManager.LoadScene("IllustratedHandbookScene");
    }

    public void OnAchievementButtonClicked()
    {
        // 加载成就场景
        SceneManager.LoadScene("AchievementScene");
    }
}
