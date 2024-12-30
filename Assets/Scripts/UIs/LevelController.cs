using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private int selectedChapter;
    // Start is called before the first frame update
    void Start()
    {
        selectedChapter = PlayerPrefs.GetInt("SelectedChapter", 1); // 默认值为1

        // 根据chapter值进行相应的关卡初始化
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 加载关卡
    private void LoadLevel(int level)
    {
        // 保存当前关卡编号
        PlayerPrefs.SetString("CurrentLevel", selectedChapter.ToString() + "_" + level.ToString());
        PlayerPrefs.Save();
        // 加载游戏场景
        SceneManager.LoadScene("BattleScene");
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("ChapterScene");
    }

    // 关卡按钮回调函数
    public void OnLevel1Click()
    {
        LoadLevel(1);
    }

    public void OnLevel2Click()
    {
        LoadLevel(2);
    }

    public void OnLevel3Click()
    {
        LoadLevel(3);
    }

    public void OnLevel4Click()
    {
        LoadLevel(4);
    }

    public void OnLevel5Click()
    {
        LoadLevel(5);
    }

    public void OnLevel6Click()
    {
        LoadLevel(6);
    }

    public void OnLevel7Click()
    {
        LoadLevel(7);
    }

    public void OnLevel8Click()
    {
        LoadLevel(8);
    }

    public void OnLevel9Click()
    {
        LoadLevel(9);
    }

    public void OnLevel10Click()
    {
        LoadLevel(10);
    }

    public void OnLevel11Click()
    {
        LoadLevel(11);
    }

    public void OnLevel12Click()
    {
        LoadLevel(12);
    }
}
