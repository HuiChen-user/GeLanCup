using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    [Header("背包设置")]
    public Transform inventoryPanel;  // 背包UI的父对象
    public GameObject itemSlotPrefab; // 物品槽预制体
    public float slotSpacing = 70f;   // 每个物品槽的垂直间距
    
    private List<ItemData> inventoryItems = new List<ItemData>();
    private List<GameObject> itemSlots = new List<GameObject>();
    
    void Awake()
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
    
    // 添加物品到背包
    public void AddItem(ItemData itemData, Sprite itemSprite)
    {
        // 1. 添加到物品列表
        inventoryItems.Add(itemData);
        
        // 2. 创建UI物品槽
        CreateItemSlot(itemData, itemSprite);
        
        Debug.Log($"拾取物品: {itemData.itemName} (ID: {itemData.itemID})，当前物品数: {inventoryItems.Count}");
    }
    
    // 创建物品槽UI
    void CreateItemSlot(ItemData itemData, Sprite itemSprite)
    {
        if (inventoryPanel == null || itemSlotPrefab == null)
        {
            Debug.LogError("背包UI未设置完整！");
            return;
        }
        
        // 创建新的物品槽
        GameObject newSlot = Instantiate(itemSlotPrefab, inventoryPanel);
        itemSlots.Add(newSlot);
        
        // 计算位置：从顶部开始，每个物品槽向下排列
        RectTransform slotRect = newSlot.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            float yPosition = -slotSpacing * (inventoryItems.Count - 1);
            slotRect.anchoredPosition = new Vector2(0, yPosition);
        }
        
        // 设置物品图标
        Image iconImage = newSlot.GetComponentInChildren<Image>();
        if (iconImage != null && itemSprite != null)
        {
            iconImage.sprite = itemSprite;
        }
        
        // 设置物品名称（可选）
        UnityEngine.UI.Text textComponent = newSlot.GetComponentInChildren<UnityEngine.UI.Text>();
        if (textComponent != null)
        {
            textComponent.text = itemData.itemName;
        }
    }
    
    //第二步：新增的代码从这里开始
    
    // 根据物品ID移除物品
    public void RemoveItem(int itemID)
    {
        // 查找物品
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemID == itemID)
            {
                // 从列表中移除
                inventoryItems.RemoveAt(i);
                
                // 销毁对应的UI物体
                if (i < itemSlots.Count)
                {
                    Destroy(itemSlots[i]);
                    itemSlots.RemoveAt(i);
                }
                
                // 重新排列剩余物品
                RearrangeItems();
                
                Debug.Log($"移除了物品: {itemID}");
                return;
            }
        }
        
        Debug.LogWarning($"没有找到物品ID: {itemID}");
    }
    
    // 重新排列物品槽位置
    void RearrangeItems()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            RectTransform slotRect = itemSlots[i].GetComponent<RectTransform>();
            if (slotRect != null)
            {
                float yPosition = -slotSpacing * i;
                slotRect.anchoredPosition = new Vector2(0, yPosition);
            }
        }
    }
    
    //新增的代码到这里结束
    
    // 检查是否有特定物品
    public bool HasItem(int itemID)
    {
        foreach (ItemData item in inventoryItems)
        {
            if (item.itemID == itemID)
                return true;
        }
        return false;
    }
    
    // 获取物品数量
    public int GetItemCount()
    {
        return inventoryItems.Count;
    }
    
    // 可选：按键使用物品功能（你可以稍后添加）
    void Update()
    {
        // 这里可以添加按数字键使用物品的逻辑
    }
}