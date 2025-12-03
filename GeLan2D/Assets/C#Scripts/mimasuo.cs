using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerCenteredUnlockSystem_Testable : MonoBehaviour
{
    [Header("基础设置")]
    public string password = "1234";
    public KeyCode interactKey = KeyCode.R;  // 打开面板键
    public KeyCode submitKey = KeyCode.Return; // ✅ 新增：自定义提交键
    public float interactionRange = 2f;
    
    [Header("奖励物品")]
    public string rewardItemID = "101";
    public string rewardItemName = "钥匙";
    public int rewardAmount = 1;
    public Sprite rewardItemIcon;
    
    [Header("背包系统")]
    public InventoryManager inventoryManager;
    
    [Header("UI组件")]
    public GameObject unlockPanel;
    public Text messageText;
    public InputField passwordInput;
    public Button submitButton; // ✅ 新增：提交按钮引用
    
    [Header("提示设置")]
    public GameObject successPopup; // ✅ 新增：成功提示弹窗
    public Text successMessageText; // ✅ 新增：成功消息文本
    public float popupDisplayTime = 2f; // ✅ 新增：弹窗显示时间
    
    [Header("位置设置")]
    public float verticalOffset = 100f;
    public bool followPlayer = true;
    
    [Header("门解锁")]
    public GameObject doorToUnlock;
    
    [Header("测试模式")]
    public bool useTestMode = true;
    public bool showDebugLogs = true;
    
    // 临时物品存储（用于测试）
    private static Dictionary<string, int> testInventory = new Dictionary<string, int>();
    
    private bool isUnlocked = false; // ✅ 重要：已解锁状态
    private Transform playerTransform;
    private RectTransform panelRect;
    private Camera mainCamera;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
        
        if (passwordInput != null)
        {
            passwordInput.characterLimit = password.Length;
        }
        
        // ✅ 初始化UI状态
        if (unlockPanel != null)
        {
            unlockPanel.SetActive(false);
            panelRect = unlockPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        
        // ✅ 初始化成功弹窗
        if (successPopup != null)
        {
            successPopup.SetActive(false);
        }
        
        // ✅ 设置提交按钮事件
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners(); // 清除旧事件
            submitButton.onClick.AddListener(CheckPassword);
        }
        
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("密码系统初始化完成");
            Debug.Log($"正确密码: {password}");
            Debug.Log($"提交按键: {submitKey}");
        }
    }
    
    void Update()
    {
        // ✅ 重要：已解锁的物体跳过所有交互
        if (isUnlocked) return;
        
        // 检测玩家是否在交互范围内
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) <= interactionRange)
        {
            // 按R键打开面板
            if (Input.GetKeyDown(interactKey))
            {
                ShowUnlockPanel();
            }
        }
        
        // 面板打开时的键盘控制
        if (unlockPanel != null && unlockPanel.activeSelf)
        {
            // ✅ 按自定义提交键提交密码
            if (Input.GetKeyDown(submitKey))
            {
                CheckPassword();
            }
            
            // ESC键关闭面板
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseUnlockPanel();
            }
        }
        
        // 面板位置跟随
        if (unlockPanel != null && unlockPanel.activeSelf && followPlayer && panelRect != null && mainCamera != null)
        {
            UpdatePanelPosition();
        }
        
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.P) && showDebugLogs)
        {
            ShowTestInventory();
        }
    }
    
    void UpdatePanelPosition()
    {
        if (playerTransform == null || mainCamera == null) return;
        
        Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);
        if (screenPos.z < 0) screenPos *= -1;
        
        Vector2 canvasPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect.parent as RectTransform,
            screenPos,
            mainCamera,
            out canvasPos))
        {
            panelRect.anchoredPosition = new Vector2(0, canvasPos.y + verticalOffset);
        }
    }
    
    // ✅ 打开解锁面板
    void ShowUnlockPanel()
    {
        if (isUnlocked) // ✅ 已解锁的物体不能打开面板
        {
            if (messageText != null)
            {
                messageText.text = "已解锁";
                messageText.color = Color.gray;
            }
            return;
        }
        
        if (unlockPanel == null) return;
        
        unlockPanel.SetActive(true);
        
        if (panelRect != null && mainCamera != null)
        {
            UpdatePanelPosition();
        }
        
        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        
        if (messageText != null)
        {
            messageText.text = $"输入密码（最多{password.Length}位）：";
            messageText.color = Color.white;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("显示密码输入面板");
        }
    }
    
    // ✅ 关闭解锁面板
    void CloseUnlockPanel()
    {
        if (unlockPanel != null)
        {
            unlockPanel.SetActive(false);
        }
    }
    
    // ✅ 检查密码（按钮和键盘都会调用）
    public void CheckPassword()
    {
        if (passwordInput == null) return;
        
        string inputText = passwordInput.text;
        
        if (inputText == password)
        {
            OnUnlockSuccess();
        }
        else
        {
            OnPasswordError();
        }
    }
    
    // ✅ 密码错误处理
    void OnPasswordError()
    {
        if (messageText != null)
        {
            messageText.text = "密码错误，请重试";
            messageText.color = Color.red;
        }
        
        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("密码错误");
        }
        
        // 可选：添加错误音效或震动
    }
    
    // ✅ 解锁成功处理
    void OnUnlockSuccess()
    {
        // 标记为已解锁
        isUnlocked = true;
        
        // 关闭输入面板
        CloseUnlockPanel();
        
        // ✅ 显示成功弹窗
        ShowSuccessPopup($"解开了！获得 {rewardItemName} ×{rewardAmount}");
        
        // 给予玩家物品
        if (useTestMode)
        {
            GiveRewardToPlayer_Test();
        }
        else
        {
            GiveRewardToPlayer_Real();
        }
        
        // 解锁门
        if (doorToUnlock != null)
        {
            doorToUnlock.SetActive(false);
            if (showDebugLogs)
            {
                Debug.Log($"门已解锁: {doorToUnlock.name}");
            }
        }
        
        // 改变自身颜色表示已解锁
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"<color=green>解锁成功！获得 {rewardItemName} ({rewardItemID}) ×{rewardAmount}</color>");
        }
    }
    
    // ✅ 新增：显示成功弹窗
    void ShowSuccessPopup(string message)
    {
        if (successPopup != null)
        {
            // 设置成功消息
            if (successMessageText != null)
            {
                successMessageText.text = message;
                successMessageText.color = Color.green;
            }
            
            // 显示弹窗
            successPopup.SetActive(true);
            
            // ✅ 弹窗跟随玩家（如果需要）
            if (followPlayer)
            {
                StartCoroutine(UpdatePopupPosition());
            }
            
            // ✅ 自动关闭弹窗
            StartCoroutine(HidePopupAfterDelay(popupDisplayTime));
        }
        else
        {
            // 如果没有弹窗，在消息文本显示
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = Color.green;
                StartCoroutine(ClearMessageAfterDelay(2f));
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"显示成功弹窗: {message}");
        }
    }
    
    // ✅ 弹窗位置更新协程
    IEnumerator UpdatePopupPosition()
    {
        if (successPopup == null || !followPlayer) yield break;
        
        RectTransform popupRect = successPopup.GetComponent<RectTransform>();
        if (popupRect == null || mainCamera == null) yield break;
        
        while (successPopup.activeSelf)
        {
            if (playerTransform != null)
            {
                Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);
                if (screenPos.z > 0)
                {
                    Vector2 canvasPos;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        popupRect.parent as RectTransform,
                        screenPos,
                        mainCamera,
                        out canvasPos))
                    {
                        popupRect.anchoredPosition = new Vector2(0, canvasPos.y + verticalOffset + 150f);
                    }
                }
            }
            yield return null;
        }
    }
    
    // ✅ 自动隐藏弹窗
    IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (successPopup != null)
        {
            successPopup.SetActive(false);
        }
    }
    
    // ✅ 清除消息文本
    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (messageText != null)
        {
            messageText.text = "";
        }
    }
    
    // 测试模式：给予物品
    void GiveRewardToPlayer_Test()
    {
        if (testInventory.ContainsKey(rewardItemID))
        {
            testInventory[rewardItemID] += rewardAmount;
        }
        else
        {
            testInventory[rewardItemID] = rewardAmount;
        }
        Debug.Log($"<color=yellow>[测试背包] 获得: {rewardItemName} ×{rewardAmount}</color>");
    }
    
    // 真实模式：添加到背包系统
    void GiveRewardToPlayer_Real()
    {
        if (inventoryManager != null)
        {
            try
            {
                ItemData newItem = new ItemData();
                newItem.itemName = rewardItemName;
                newItem.description = $"通过密码锁获得的{rewardItemName}";
                
                if (int.TryParse(rewardItemID, out int itemId))
                {
                    newItem.itemID = itemId;
                }
                else
                {
                    newItem.itemID = rewardItemID.GetHashCode();
                }
                
                inventoryManager.AddItem(newItem, rewardItemIcon);
                
                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[背包系统] 获得: {rewardItemName} (ID: {newItem.itemID}) ×{rewardAmount}</color>");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"添加到背包失败: {e.Message}");
                GiveRewardToPlayer_Test();
            }
        }
        else
        {
            GiveRewardToPlayer_Test();
        }
    }
    
    // 显示测试背包内容
    void ShowTestInventory()
    {
        if (testInventory.Count == 0)
        {
            Debug.Log("[测试背包] 空空如也");
            return;
        }
        
        string inventoryText = "[测试背包] 内容:\n";
        foreach (var item in testInventory)
        {
            inventoryText += $"- {item.Key}: ×{item.Value}\n";
        }
        Debug.Log(inventoryText);
    }
    
    // ========== 调试工具 ==========
    [ContextMenu("测试解锁")]
    void TestUnlock()
    {
        if (Application.isPlaying && !isUnlocked)
        {
            OnUnlockSuccess();
        }
    }
    
    [ContextMenu("重置锁定状态")]
    void ResetLockState()
    {
        isUnlocked = false;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.blue;
        }
        
        Debug.Log("密码锁状态已重置");
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
            Gizmos.DrawSphere(playerHeadPos, 0.1f);
            Gizmos.DrawLine(transform.position, playerHeadPos);
        }
    }
    
    void OnGUI()
    {
        if (showDebugLogs)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.yellow;
            style.fontSize = 14;
            
            GUI.Label(new Rect(10, 30, 400, 100), $"状态: {(isUnlocked ? "已解锁" : "未解锁")}", style);
            GUI.Label(new Rect(10, 50, 400, 100), "按P键查看背包详情", style);
            GUI.Label(new Rect(10, 70, 400, 100), $"提交键: {submitKey}", style);
        }
    }
}
