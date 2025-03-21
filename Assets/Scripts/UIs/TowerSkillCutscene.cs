using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TowerSkillCutscene : MonoBehaviour
{
    [SerializeField]private Image towerImage;
    [SerializeField]private TextMeshProUGUI skillNameText;
    [SerializeField]private TextMeshProUGUI towerNameText;
    [SerializeField] private RectTransform container;

    // [SerializeField] private AudioSource sfxPlayer;
    // [SerializeField] private AudioClip introCutsceneSfx;
    
    private void Awake()
    {

    }
    
    void Start()
    {
        // 检查组件引用是否正确
        if (towerImage == null)
        {
            Debug.LogError("towerImage未设置!");
            towerImage = transform.Find("TowerImage")?.GetComponent<Image>();
        }
        
        if (skillNameText == null)
        {
            Debug.LogError("skillNameText未设置!");
            skillNameText = transform.Find("SkillNameText")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (towerNameText == null)
        {
            Debug.LogError("towerNameText未设置!");
            towerNameText = transform.Find("TowerNameText")?.GetComponent<TextMeshProUGUI>();
        }
        
        if (container == null)
        {
            Debug.LogError("container未设置!");
            container = GetComponent<RectTransform>();
        }
    }
    
    public void SetSkillData(TowerData towerData)
    {
        // 添加空值检查
        if (towerData == null)
        {
            Debug.LogError("TowerData 为空！");
            return;
        }
        
        if (towerImage == null)
        {
            Debug.LogError("towerImage 引用为空！");
            return;
        }
        
        if (skillNameText == null)
        {
            Debug.LogError("skillNameText 引用为空！");
            return;
        }
        
        if (towerNameText == null)
        {
            Debug.LogError("towerNameText 引用为空！");
            return;
        }
        
        // 检查 towerData 的必要属性
        if (towerData.towerSprite == null)
        {
            Debug.LogWarning("towerData.towerSprite 为空！");
            // 使用默认图像或跳过设置
        }
        else
        {
            towerImage.sprite = towerData.towerSprite;
        }
        
        skillNameText.text = towerData.skillName;
        towerNameText.text = string.IsNullOrEmpty(towerData.towerName) ? "Unknown Tower" : towerData.towerName;
    }
    
    public void PlayIntroAnimation()
    {
        // 播放音效
        // if (introCutsceneSfx != null && sfxPlayer != null)
        // {
        //     sfxPlayer.PlayOneShot(introCutsceneSfx);
        // }
        
        // DOTween实现的动画效果
        container.localScale = Vector3.zero;
        container.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        
        skillNameText.alpha = 0f;
        skillNameText.DOFade(1f, 0.3f).SetDelay(0.2f).SetUpdate(true);
        
        towerNameText.alpha = 0f;
        towerNameText.DOFade(1f, 0.3f).SetDelay(0.4f).SetUpdate(true);
        
        // towerImage.rectTransform.localPosition = new Vector3(-500, 0, 0);
        // towerImage.rectTransform.DOLocalMoveX(0, 0.7f).SetEase(Ease.OutQuint).SetUpdate(true);
    }
    
    public void PlayOutroAnimation()
    {
        // 创建一个序列，包含所有动画
        Sequence sequence = DOTween.Sequence();
        
        // 添加所有动画到序列
        sequence.Join(container.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        sequence.Join(skillNameText.DOFade(0f, 0.2f));
        sequence.Join(towerNameText.DOFade(0f, 0.2f));
        
        // 为整个序列设置忽略时间缩放，确保在游戏暂停时也能执行
        sequence.SetUpdate(true);
        
        // 设置序列在完成时自动销毁
        sequence.SetAutoKill(true);
    }

    public void KillAllTweens()
    {
        // 安全检查
        if (container != null) DOTween.Kill(container);
        if (skillNameText != null) DOTween.Kill(skillNameText);
        if (towerNameText != null) DOTween.Kill(towerNameText);
        if (towerImage != null && towerImage.rectTransform != null) DOTween.Kill(towerImage.rectTransform);
        
        // 额外确保所有与此对象相关的补间都停止
        DOTween.Kill(gameObject);
    }
}