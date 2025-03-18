using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SkillCutsceneManager : MonoBehaviour
{
    public static SkillCutsceneManager Instance { get; private set; }
    
    [SerializeField] private GameObject cutscenePrefab;
    [SerializeField] private float minTimeBetweenCutscenes = 5f;
    [SerializeField] private bool enableCutscenes = true;
    
    private GameObject currentCutscene;
    private float lastCutsceneTime;
    private bool isPlaying = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlaySkillCutscene(TowerData towerData, Vector3 position, System.Action onComplete = null)
    {
        if (!enableCutscenes) 
        {
            onComplete?.Invoke();
            return;
        }
        
        if (Time.time - lastCutsceneTime < minTimeBetweenCutscenes)
        {
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(PlayCutsceneSequence(towerData, position, onComplete));
    }
    
    private IEnumerator PlayCutsceneSequence(TowerData towerData, Vector3 position, System.Action onComplete)
    {
        isPlaying = true;
        lastCutsceneTime = Time.time;
        
        // 找到主Canvas（或创建一个）
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("场景中没有找到Canvas!");
        }
        
        // 在Canvas下实例化UI
        currentCutscene = Instantiate(cutscenePrefab, mainCanvas.transform);
        currentCutscene.transform.position = position + new Vector3(0, 2, 0);

        // 设置技能数据
        var cutsceneController = currentCutscene.GetComponent<TowerSkillCutscene>();
        if (cutsceneController == null)
        {
            Debug.LogError("预制体上没有找到 TowerSkillCutscene 组件！");
            Destroy(currentCutscene);
            isPlaying = false;
            onComplete?.Invoke();
            yield break;
        }
        
        cutsceneController.SetSkillData(towerData);
        
        // 播放入场动画
        cutsceneController.PlayIntroAnimation();
        
        // 等待入场动画完成（使用Time.unscaledDeltaTime因为游戏暂停了）
        float duration = 2.0f; // 动画持续时间
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 播放退场动画
        cutsceneController.PlayOutroAnimation();
        
        // 等待退场动画完成
        float outroDuration = 1.0f;
        float outroTimer = 0f;
        
        while (outroTimer < outroDuration)
        {
            outroTimer += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // 在清理部分也销毁canvas
        if (currentCutscene != null)
        {
            // 先存储引用
            GameObject tempCutscene = currentCutscene;
            currentCutscene = null;
            
            // 确保动画停止
            var towerSkillComponent = tempCutscene.GetComponent<TowerSkillCutscene>();
            if (towerSkillComponent != null)
            {
                towerSkillComponent.KillAllTweens();
            }
            
            // 销毁对象
            Destroy(tempCutscene);
        }
        
        // 恢复游戏
        isPlaying = false;
        
        // 回调
        onComplete?.Invoke();
    }
    
    public bool IsPlayingCutscene()
    {
        return isPlaying;
    }
    
    public void EnableCutscenes(bool enable)
    {
        enableCutscenes = enable;
    }
}