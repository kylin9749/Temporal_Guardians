using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class ChapterController : MonoBehaviour
{
    public Button[] chapterButtons; // 在Inspector中设置章节按钮
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateChapterButtonsState();
    }

    private void UpdateChapterButtonsState()
    {
        if (PlayerManager.Instance == null) return;
        
        // 第一章默认解锁
        chapterButtons[0].interactable = true;
        
        // 检查其他章节是否解锁
        for (int i = 1; i < chapterButtons.Length; i++)
        {
            bool isUnlocked = IsChapterUnlocked(i + 1);
            chapterButtons[i].interactable = isUnlocked;
            
            // 可选：为未解锁的章节添加锁定图标
            Transform lockIcon = chapterButtons[i].transform.Find("LockIcon");
            if (lockIcon != null)
            {
                lockIcon.gameObject.SetActive(!isUnlocked);
            }
        }
    }
    
    private bool IsChapterUnlocked(int chapterNumber)
    {
        // 第一章默认解锁
        if (chapterNumber <= 1) return true;
        
        // 方案1：检查前一章节的最后一关是否通关（更简单，推荐）
        string previousChapter = (chapterNumber - 1).ToString();
        string lastLevelKey = $"{previousChapter}_{GetLastLevelNumberInChapter(chapterNumber - 1)}";
        return PlayerManager.Instance.playerData.completedLevels.Contains(lastLevelKey);
        
        // 方案2：检查前一章节的所有关卡是否通关（更严格）
        /* 
        string previousChapter = (chapterNumber - 1).ToString();
        int lastLevelNumber = GetLastLevelNumberInChapter(chapterNumber - 1);
        
        // 检查前一章节的每一关是否都已通关
        for (int i = 1; i <= lastLevelNumber; i++)
        {
            string levelKey = $"{previousChapter}_{i}";
            if (!PlayerManager.Instance.playerData.completedLevels.Contains(levelKey))
            {
                return false; // 只要有一关未通关，就返回false
            }
        }
        return true; // 所有关卡都已通关
        */
    }

    // 获取指定章节的最后一关编号
    private int GetLastLevelNumberInChapter(int chapterNumber)
    {
        // 这里可以根据实际情况返回每个章节的最后一关编号
        // 例如，可以硬编码，或者从配置文件读取，或者通过其他方式获取
        switch (chapterNumber)
        {
            case 1: return 12; // 第一章有12关
            case 2: return 12; // 第二章有12关
            case 3: return 12; // 第三章有12关
            case 4: return 12; // 第四章有12关
            default: return 12; // 默认12关
        }
    }

    void Update()
    {

    }

    public void ChapterSelect(int chapter)
    {
        if (!IsChapterUnlocked(chapter))
        {
            // 可以显示提示信息
            Debug.Log("需要先通关前一章节");
            return;
        }
        
        PlayerPrefs.SetInt("SelectedChapter", chapter);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelScene");
    }

    public void OnChapter1ButtonClick()
    {
        ChapterSelect(1);
    }

    public void OnChapter2ButtonClick()
    {
        ChapterSelect(2);
    }

    public void OnChapter3ButtonClick()
    {
        ChapterSelect(3);
    }

    public void OnChapter4ButtonClick()
    {
        ChapterSelect(4);
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("MainScene");
    }
}
