using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class UITipManager : MonoBehaviour
{
    public GameObject tipPanel;        // UI提示文本panel
    public TextMeshProUGUI tipText;        // UI提示文本组件
    public float showTime = 2f;         // 提示显示时间
    
    public void ShowTip(string message)
    {
        if (tipPanel != null && tipText != null)
        {
            tipText.text = message;
            tipPanel.SetActive(true);
            StartCoroutine(HideTipAfterDelay());
        }
    }
    
    private IEnumerator HideTipAfterDelay()
    {
        yield return new WaitForSeconds(showTime);
        tipPanel.SetActive(false);
    }
}