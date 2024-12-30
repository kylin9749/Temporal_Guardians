using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITipManager : MonoBehaviour
{
    public static UITipManager Instance { get; private set; }
    
    public Text tipText;        // UI提示文本组件
    public float showTime = 2f; // 提示显示时间
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowTip(string message)
    {
        if (tipText != null)
        {
            tipText.text = message;
            tipText.gameObject.SetActive(true);
            StartCoroutine(HideTipAfterDelay());
        }
    }
    
    private IEnumerator HideTipAfterDelay()
    {
        yield return new WaitForSeconds(showTime);
        tipText.gameObject.SetActive(false);
    }
}