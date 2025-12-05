using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    [Header("背包设置")]
    public Transform inventoryPanel;  // 背包UI的父对象
    public GameObject itemSlotPrefab; // 物品槽预制体（必须包含背景框、图标、文字）
    public float slotSpacing = 70f;   // 每个物品槽的垂直间距
    
    [Header("最大容量")]
    public int maxCapacity = 10;      // 背包最大容量
    
    private List<ItemData> inventoryItems = new List<ItemData>();
    private List<GameObject> itemSlots = new List<GameObject>();
    
    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 添加这一行
    }
    else
    {
        Destroy(gameObject);
    }
}
    
    void Start()
    {
        // 清空背包UI（确保开始时是空的）
        ClearInventoryUI();
        Debug.Log("背包系统初始化完成");
    }
    
    // 清空背包UI（不删除物品数据）
    void ClearInventoryUI()
    {
        if (inventoryPanel == null) return;
        
        foreach (Transform child in inventoryPanel)
        {
            Destroy(child.gameObject);
        }
        itemSlots.Clear();
    }
    
    // 添加物品到背包
    public void AddItem(ItemData itemData, Sprite itemSprite)
    {
        // 检查背包是否已满
        if (inventoryItems.Count >= maxCapacity)
        {
            Debug.LogWarning($"背包已满！最多只能携带{maxCapacity}个物品。");
            return;
        }
        
        // 1. 添加到物品列表
        inventoryItems.Add(itemData);
        
        // 2. 创建UI物品槽
        CreateItemSlot(itemData, itemSprite);
        
        Debug.Log($"拾取物品: {itemData.itemName} (ID: {itemData.itemID})，当前物品数: {inventoryItems.Count}");
    }
    
    // 创建物品槽UI
    void CreateItemSlot(ItemData itemData, Sprite itemSprite)
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryPanel未指定！");
            return;
        }
        
        if (itemSlotPrefab == null)
        {
            Debug.LogError("ItemSlotPrefab未指定！");
            return;
        }
        
        // 创建新的物品槽
        GameObject newSlot = Instantiate(itemSlotPrefab, inventoryPanel);
        
        // 命名以便识别
        newSlot.name = $"ItemSlot_{inventoryItems.Count:00}_{itemData.itemName}";
        
        // 添加到列表
        itemSlots.Add(newSlot);
        
        // 设置位置
        RectTransform slotRect = newSlot.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            float yPosition = -slotSpacing * (inventoryItems.Count - 1);
            slotRect.anchoredPosition = new Vector2(0, yPosition);
        }
        
        // 查找并设置图标
        Image iconImage = FindChildComponent<Image>(newSlot, "Icon", true);
        if (iconImage != null && itemSprite != null)
        {
            iconImage.sprite = itemSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }
        
        // 设置物品名称 - 针对TextMeshProUGUI组件
        Transform nameTextTransform = newSlot.transform.Find("ItemName");
        if (nameTextTransform != null)
        {
            // 关键修改：获取 TextMeshProUGUI 组件，而不是 Text
            TextMeshProUGUI nameText = nameTextTransform.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = itemData.itemName; // 赋值方式不变
                Debug.Log($"成功设置TextMeshPro文本为: {itemData.itemName}");
            }
            else
            {
                Debug.LogError($"在 ItemName 物体上未找到 TextMeshProUGUI 组件！请确认预制体使用的组件类型。");
            }
        }
        else
        {
            Debug.LogError($"在物品槽 {newSlot.name} 中未找到名为 'ItemName' 的子物体！");
        }
        
        // 确保物品槽激活
        newSlot.SetActive(true);
    }
    
    // 通用方法：在子物体中查找指定组件
    T FindChildComponent<T>(GameObject parent, string childName, bool excludeParent = false) where T : Component
    {
        // 方法1：按名称查找
        Transform child = parent.transform.Find(childName);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null) return component;
        }
        
        // 方法2：查找所有子组件的第一个
        T[] allComponents = parent.GetComponentsInChildren<T>(true);
        foreach (T component in allComponents)
        {
            if (excludeParent && component.transform == parent.transform)
                continue;
                
            return component;
        }
        
        Debug.LogWarning($"在{parent.name}中未找到{typeof(T).Name}组件 (名称: {childName})");
        return null;
    }
    
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
    
    // 清空背包（调试用）
    public void ClearInventory()
    {
        inventoryItems.Clear();
        ClearInventoryUI();
        Debug.Log("背包已清空");
    }
}