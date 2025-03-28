using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    private int selectedChapter;
    public Button[] levelButtons;
    // Start is called before the first frame update
    void Start()
    {
        selectedChapter = PlayerPrefs.GetInt("SelectedChapter", 1);
        
        // 初始化关卡按钮
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "Level" + (i + 1).ToString();
            UpdateLevelButtonState(i);
        }
    }

    private void UpdateLevelButtonState(int levelIndex)
    {
        string levelKey = $"{selectedChapter}_{levelIndex + 1}";
        bool isUnlocked = IsLevelUnlocked(levelIndex + 1);
        
        levelButtons[levelIndex].interactable = isUnlocked;
        
        // 可选：为未解锁的关卡添加锁定图标
        Transform lockIcon = levelButtons[levelIndex].transform.Find("LockIcon");
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(!isUnlocked);
        }
    }
    
    private bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true; 
        
        // 检查前一关是否通关
        string previousLevelKey = $"{selectedChapter}_{levelNumber - 1}";
        return PlayerManager.Instance != null && 
               PlayerManager.Instance.playerData.completedLevels.Contains(previousLevelKey);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 加载关卡
    public void LoadLevel(int level)
    {
        if (!IsLevelUnlocked(level))
        {
            // 可以显示提示信息
            Debug.Log("需要先通关前一关卡");
            return;
        }
        
        PlayerPrefs.SetString("CurrentLevel", selectedChapter.ToString() + "_" + level.ToString());
        PlayerPrefs.SetInt("SelectedChapter", selectedChapter);
        PlayerPrefs.Save();
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
