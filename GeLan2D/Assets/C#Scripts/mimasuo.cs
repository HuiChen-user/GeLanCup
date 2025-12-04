using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerCenteredUnlockSystem_Testable : MonoBehaviour
{
    [Header("基础设置")]
    public string password = "1234";
    public KeyCode interactKey = KeyCode.R; // 打开面板键
    public KeyCode submitKey = KeyCode.Return; // 提交密码键
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
    public Button submitButton; // 可选：如果需要鼠标点击提交
    
    [Header("提示设置")]
    public GameObject successPopup; // 成功提示弹窗
    public Text successMessageText; // 成功消息文本
    public float popupDisplayTime = 2f; // 弹窗显示时间
    public float popupVerticalOffset = 150f; // 弹窗垂直偏移
    
    [Header("位置设置")]
    public float panelVerticalOffset = 100f; // 输入面板垂直偏移
    public bool followPlayer = true;
    
    [Header("门解锁")]
    public GameObject doorToUnlock;
    
    [Header("测试模式")]
    public bool useTestMode = true;
    public bool showDebugLogs = true;
    
    // 打字机效果设置
    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f; // 每个字符显示的时间间隔
    private Coroutine typingCoroutine; // 用于控制打字机效果的协程
    
    // 临时物品存储（用于测试）
    private static Dictionary<string, int> testInventory = new Dictionary<string, int>();
    private bool isUnlocked = false;
    private Transform playerTransform;
    private RectTransform panelRect;
    private RectTransform popupRect;
    private Camera mainCamera;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
        
        // 设置输入框字符限制
        if (passwordInput != null)
        {
            passwordInput.characterLimit = password.Length;
            // 监听Enter键提交
            passwordInput.onEndEdit.AddListener(OnPasswordInputEndEdit);
        }
        
        // 初始化输入面板
        if (unlockPanel != null)
        {
            unlockPanel.SetActive(false);
            panelRect = unlockPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                SetupRectTransform(panelRect);
            }
        }
        
        // 初始化成功弹窗
        if (successPopup != null)
        {
            successPopup.SetActive(false);
            popupRect = successPopup.GetComponent<RectTransform>();
            if (popupRect != null)
            {
                SetupRectTransform(popupRect);
            }
        }
        
        // 设置提交按钮事件（如果提供了按钮）
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CheckPassword);
        }
        
        // 自动查找背包管理器
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
        
        // 验证ID格式
        if (!IsNumeric(rewardItemID) && showDebugLogs)
        {
            Debug.LogWarning($"rewardItemID '{rewardItemID}' 不是纯数字，真实模式下可能出错！");
        }
        
        if (showDebugLogs)
        {
            Debug.Log("密码系统初始化完成");
            Debug.Log($"正确密码: {password}");
            Debug.Log($"交互键: {interactKey}, 提交键: {submitKey}");
            Debug.Log($"模式: {(useTestMode ? "测试模式" : "真实模式")}");
        }
    }
    
    void Update()
    {
        if (isUnlocked) return;
        
        // 检测玩家是否在交互范围内
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) <= interactionRange)
        {
            if (Input.GetKeyDown(interactKey))
            {
                ShowUnlockPanel();
            }
        }
        
        // 面板打开时的键盘控制
        if (unlockPanel != null && unlockPanel.activeSelf)
        {
            // 按自定义提交键提交密码
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
        
        // 更新输入面板位置
        if (unlockPanel != null && unlockPanel.activeSelf && followPlayer && panelRect != null && mainCamera != null)
        {
            UpdateUIPosition(panelRect, panelVerticalOffset);
        }
        
        // 更新成功弹窗位置
        if (successPopup != null && successPopup.activeSelf && followPlayer && popupRect != null && mainCamera != null)
        {
            UpdateUIPosition(popupRect, popupVerticalOffset);
        }
        
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.P) && showDebugLogs)
        {
            ShowTestInventory();
        }
    }
    
    // 设置RectTransform基础属性
    void SetupRectTransform(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
    
    // 更新UI位置（通用方法）
    void UpdateUIPosition(RectTransform uiRect, float verticalOffset)
    {
        if (playerTransform == null || mainCamera == null) return;
        
        Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);
        
        // 如果玩家在屏幕后方，取反坐标
        if (screenPos.z < 0) screenPos *= -1;
        
        Vector2 canvasPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiRect.parent as RectTransform,
            screenPos,
            mainCamera,
            out canvasPos))
        {
            uiRect.anchoredPosition = new Vector2(0, canvasPos.y + verticalOffset);
        }
    }
    
    // 打字机效果显示文本
    IEnumerator TypeTextEffect(Text targetText, string fullText)
    {
        targetText.text = "";
        
        // 如果有正在进行的打字效果，先停止它
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 逐个字符显示
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    // 打开解锁面板
    void ShowUnlockPanel()
    {
        if (isUnlocked)
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
            UpdateUIPosition(panelRect, panelVerticalOffset);
        }
        
        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.Select();
            passwordInput.ActivateInputField();
        }
        
        if (messageText != null)
        {
            string fullText = $"输入密码（最多{password.Length}位）：";
            typingCoroutine = StartCoroutine(TypeTextEffect(messageText, fullText));
            messageText.color = Color.white;
        }
        
        if (showDebugLogs)
        {
            Debug.Log("显示密码输入面板");
        }
    }
    
    // 关闭解锁面板
    void CloseUnlockPanel()
    {
        if (unlockPanel != null)
        {
            unlockPanel.SetActive(false);
        }
    }
    
    // 输入框结束编辑监听（处理Enter键提交）
    void OnPasswordInputEndEdit(string text)
    {
        // 只有当用户按Enter提交时才处理，点击其他地方不处理
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckPassword();
        }
    }
    
    // 检查密码（按钮和键盘都会调用）
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
    
    // 密码错误处理
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
    }
    
    // 解锁成功处理
    void OnUnlockSuccess()
    {
        isUnlocked = true;
        
        // 关闭输入面板
        CloseUnlockPanel();
        
        // 显示成功弹窗
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
        
        // 注意：这里移除了改变自身颜色的代码
        
        if (showDebugLogs)
        {
            Debug.Log($"<color=green>解锁成功！获得 {rewardItemName} ({rewardItemID}) ×{rewardAmount}</color>");
        }
    }
    
    // 显示成功弹窗
    void ShowSuccessPopup(string message)
    {
        if (successPopup != null)
        {
            // 设置成功消息
            if (successMessageText != null)
            {
                // 使用打字机效果显示成功消息
                typingCoroutine = StartCoroutine(TypeTextEffect(successMessageText, message));
                successMessageText.color = Color.green;
            }
            
            // 确保弹窗位置正确
            if (popupRect != null && mainCamera != null && playerTransform != null)
            {
                UpdateUIPosition(popupRect, popupVerticalOffset);
            }
            
            // 显示弹窗
            successPopup.SetActive(true);
            
            // 自动关闭弹窗
            StartCoroutine(HidePopupAfterDelay(popupDisplayTime));
        }
        else
        {
            // 如果没有弹窗，在消息文本显示
            if (messageText != null)
            {
                // 使用打字机效果显示消息
                typingCoroutine = StartCoroutine(TypeTextEffect(messageText, message));
                messageText.color = Color.green;
                StartCoroutine(ClearMessageAfterDelay(2f));
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"显示成功弹窗: {message}");
        }
    }
    
    // 自动隐藏弹窗
    IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (successPopup != null)
        {
            successPopup.SetActive(false);
        }
    }
    
    // 清除消息文本
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
            Debug.LogWarning("背包系统未找到，使用测试模式");
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
    
    // 检查字符串是否为纯数字
    bool IsNumeric(string str)
    {
        foreach (char c in str)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
    
    // ========== 调试工具 ==========
    [ContextMenu("测试解锁")]
    void TestUnlock()
    {
        if (Application.isPlaying && !isUnlocked)
        {
            if (passwordInput != null)
            {
                passwordInput.text = password;
                CheckPassword();
            }
        }
    }
    
    [ContextMenu("重置锁定状态")]
    void ResetLockState()
    {
        isUnlocked = false;
        
        // 注意：这里移除了重置颜色的代码，因为没有颜色变化了
        
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
            GUI.Label(new Rect(10, 70, 400, 100), $"交互键: {interactKey}, 提交键: {submitKey}", style);
        }
    }
}
