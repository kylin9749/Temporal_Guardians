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
        if (chapterNumber <= 1) return true;
        
        // 检查前一章节是否有通关的关卡
        string previousChapter = (chapterNumber - 1).ToString();
        return PlayerManager.Instance.playerData.completedLevels.Any(level => 
            level.StartsWith(previousChapter + "_"));
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
