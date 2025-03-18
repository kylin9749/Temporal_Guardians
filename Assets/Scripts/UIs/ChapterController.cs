using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {

    }

    public void ChapterSelect(int chapter)
    {
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
