using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;
    [SerializeField] private TextMeshProUGUI towerCategoryText;
    
    // 添加最小拖拽距离阈值
    [SerializeField] private float minDragDistance = 10f;
    
    private TowerType towerType;
    private BattleController battleController;
    private TowerData towerData;
    private GameObject towerPreview;
    
    // 添加拖拽开始位置
    private Vector2 dragStartPosition;
    // 添加是否正在进行有效拖拽的标志
    private bool isValidDrag = false;
    
    // 初始化按钮数据
    public void Initialize(TowerData data, BattleController controller)
    {
        towerData = data;
        towerType = data.towerType;
        battleController = controller;
        
        // 设置UI元素
        towerImage.sprite = data.towerSprite;
        towerNameText.text = data.towerName;
        towerPriceText.text = data.cost.ToString();
        
        // 设置防御塔分类文本
        switch(data.towerCategory)
        {
            case TowerCategory.Single:
                towerCategoryText.text = "单体";
                break;
            case TowerCategory.Area:
                towerCategoryText.text = "群攻";
                break;
            case TowerCategory.Support:
                towerCategoryText.text = "辅助";
                break;
            case TowerCategory.Control:
                towerCategoryText.text = "控制";
                break;
        }
    }
    
    // 处理点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果是有效拖拽结束，不触发点击
        if (isValidDrag) return;
        
        // 显示防御塔详细信息
        ShowTowerDetailPanel();
    }
    
    // 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 确保任何之前的拖拽引用被清理
        CleanupPreviousDrag();
        
        // 记录拖拽开始位置
        dragStartPosition = eventData.position;
        // 重置拖拽状态
        isValidDrag = false;
    }
    
    // 拖拽中
    public void OnDrag(PointerEventData eventData)
    {
        // 计算拖拽距离
        float dragDistance = Vector2.Distance(dragStartPosition, eventData.position);
        
        // 如果拖拽距离超过阈值，且预览塔尚未创建
        if (!isValidDrag && dragDistance > minDragDistance)
        {
            isValidDrag = true;
            
            // 检查是否有足够金钱
            if (battleController.GetMoney() < towerData.cost)
            {
                if (UITipManager.Instance != null)
                {
                    UITipManager.Instance.ShowTip("金币不足，无法建造");
                }
                // 重要：标记拖拽无效并中止拖拽流程
                isValidDrag = false;
                return;
            }
            
            // 创建预览塔
            towerPreview = TowerFactory.Instance.CreateTower(towerType, Vector3.zero, battleController);
        }
        
        // 如果是有效拖拽，更新预览塔位置
        if (isValidDrag && towerPreview != null)
        {
            // 更新预览塔位置
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            towerPreview.GetComponent<towerCommon>().UpdateTowerPreviewPosition(worldPos);
        }
    }
    
    // 结束拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        // 只有在有效拖拽的情况下，才处理塔放置
        if (isValidDrag && towerPreview != null)
        {
            // 结束拖拽时尝试放置防御塔
            towerPreview.GetComponent<towerCommon>().TryPlaceTower();
            // 放置后清空引用和状态
            towerPreview = null;
        }
        
        // 重置拖拽状态
        isValidDrag = false;
    }
    
    // 新增：清理之前的拖拽状态
    private void CleanupPreviousDrag()
    {
        // 如果存在预览塔引用但未成功放置，则销毁它
        if (towerPreview != null)
        {
            Destroy(towerPreview);
            towerPreview = null;
        }
        isValidDrag = false;
    }
    
    // 显示详细信息面板
    private void ShowTowerDetailPanel()
    {
        // 实现显示塔的详细信息面板
        battleController.ShowTowerInfoPanel(towerData);
    }

    // 新增：确保在脚本被禁用或销毁时清理资源
    private void OnDisable()
    {
        CleanupPreviousDrag();
    }
    
    private void OnDestroy()
    {
        CleanupPreviousDrag();
    }
}