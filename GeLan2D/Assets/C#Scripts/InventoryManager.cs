using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 1. 新增命名空间

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    [Header("背包设置")]
    public Transform inventoryPanel; // 背包UI的父对象
    public GameObject itemSlotPrefab; // 物品槽预制体（必须包含背景框、图标、文字）
    public float slotSpacing = 70f; // 每个物品槽的垂直间距
    
    [Header("最大容量")]
    public int maxCapacity = 10; // 背包最大容量

    // 添加这个字典来保存物品ID到图标的映射
    private Dictionary<int, Sprite> itemIconDictionary = new Dictionary<int, Sprite>();
    private List<ItemData> inventoryItems = new List<ItemData>();
    private List<GameObject> itemSlots = new List<GameObject>();
    
    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // 添加这一行
        // +++ 在 DontDestroyOnLoad 之后，添加下面这两行新代码 +++
        // 订阅场景加载完成的事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        // +++ 新增代码结束 +++
    }
    else
    {
        Destroy(gameObject);
    }
}
    
    void Start()
    {
         // 初始时查找面板并刷新UI
        FindInventoryPanelInScene();
        RefreshInventoryUI(); // <--- 修改这一行，原来是 ClearInventoryUI();
        Debug.Log("背包系统初始化完成");
    }

    // ========== 以下是需要添加的三个新方法 ==========

    // 1. 新方法：当新场景加载完成时，Unity会自动调用此函数
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"检测到新场景加载: {scene.name}, 正在更新背包UI引用...");
        FindInventoryPanelInScene();
        RefreshInventoryUI(); // 刷新UI，显示当前背包物品
    }

    // 2. 新方法：在当前场景中查找背包面板
    void FindInventoryPanelInScene()
    {
        // 方法：手动检查所有Canvas下是否有我们需要的面板
        // 这里假设你的背包面板是挂在Canvas下的一个叫 "InventoryPanel" 的GameObject
        // 如果你的面板名字不同，请将下面代码里的 "InventoryPanel" 替换成你实际的名字！
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true); // true表示也会查找隐藏的
        foreach (Canvas canvas in allCanvases)
        {
            Transform panel = canvas.transform.Find("InventoryPanel"); // <-- 注意名字！
            if (panel != null)
            {
                inventoryPanel = panel;
                Debug.Log($"成功更新背包面板引用: {panel.name}, 位于 {canvas.name}");
                return; // 找到后直接返回
            }
        }
    
        // 如果没找到，给出警告
        if (inventoryPanel == null)
        {
            Debug.LogWarning($"在当前场景 {SceneManager.GetActiveScene().name} 中未找到背包面板(InventoryPanel)。物品将无法显示。");
        }
    }

    // 3. 新方法：刷新整个背包UI（根据数据列表重新绘制）
    void RefreshInventoryUI()
    {
        // 先清空当前所有UI格子
        ClearInventoryUI();
    
        // 如果面板不存在，无法刷新
        if (inventoryPanel == null || inventoryItems == null) return;
    
        // 遍历所有物品数据，重新创建UI格子
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            ItemData item = inventoryItems[i];
            Sprite itemSprite = null;
        
            // 尝试从字典获取图标
            if (itemIconDictionary.ContainsKey(item.itemID))
            {
                itemSprite = itemIconDictionary[item.itemID];
            }
        
            CreateItemSlot(item, itemSprite);
        }
    
        Debug.Log($"背包UI已刷新，显示 {inventoryItems.Count} 个物品。");
    }
    // ========== 新增方法结束 ==========
    
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
    
        // 2. 将图标保存到字典（关键！）
        if (itemSprite != null && !itemIconDictionary.ContainsKey(itemData.itemID))
        {
            itemIconDictionary[itemData.itemID] = itemSprite;
            Debug.Log($"已缓存物品图标: {itemData.itemName} (ID: {itemData.itemID})");
        }
    
        // 3. 创建UI物品槽
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
    newSlot.name = $"ItemSlot_{inventoryItems.Count:00}_{itemData.itemName}";
    itemSlots.Add(newSlot);
    
    // 设置位置
    RectTransform slotRect = newSlot.GetComponent<RectTransform>();
    if (slotRect != null)
    {
        float yPosition = -slotSpacing * (inventoryItems.Count - 1);
        slotRect.anchoredPosition = new Vector2(0, yPosition);
    }
    
    // 查找并设置图标 - 优先使用传入的sprite，如果为null则从字典获取
    Sprite iconToUse = itemSprite;
    if (iconToUse == null && itemIconDictionary.ContainsKey(itemData.itemID))
    {
        iconToUse = itemIconDictionary[itemData.itemID];
        Debug.Log($"从字典恢复图标: {itemData.itemName}");
    }
    
    Image iconImage = FindChildComponent<Image>(newSlot, "Icon", true);
    if (iconImage != null)
    {
        iconImage.sprite = iconToUse;
        iconImage.color = (iconToUse != null) ? Color.white : new Color(1, 1, 1, 0.5f); // 没图标就半透明白色
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
    // ========== 在脚本末尾添加这个方法 ==========
    // 重要：在脚本被销毁时取消事件订阅，避免内存泄漏
    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    // ========== 新增结束 ==========
}   // 注意：这行是你的类本身的右大括号，应该已经存在，不要重复添加。