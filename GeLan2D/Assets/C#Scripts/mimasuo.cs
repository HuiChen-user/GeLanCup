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
    
    [Header("2D提示设置")]
    public GameObject interactionHintUI;
    public SpriteRenderer interactionHintSprite;
    public Vector2 hintOffset = new Vector2(0.5f, 1f); // 右上角偏移
    public float hintPixelOffsetX = 20f; // 像素级水平偏移
    public float hintPixelOffsetY = 20f; // 像素级垂直偏移
    
    [Header("测试模式")]
    public bool useTestMode = true;
    public bool showDebugLogs = true;
    
    // 临时物品存储（用于测试）
    private static Dictionary<string, int> testInventory = new Dictionary<string, int>();
    
    private bool isUnlocked = false;
    private Transform playerTransform;
    private RectTransform panelRect;
    private RectTransform popupRect;
    private Camera mainCamera;
    
    // 新增：用于2D提示
    private bool playerInRange = false;
    private RectTransform hintRectTransform; // 如果是UI类型提示
    
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
        
        // 初始化2D提示
        if (interactionHintUI != null)
        {
            interactionHintUI.SetActive(false);
            // 如果是UI类型，获取RectTransform
            hintRectTransform = interactionHintUI.GetComponent<RectTransform>();
        }
        
        if (interactionHintSprite != null)
        {
            interactionHintSprite.enabled = false;
            // 初始位置设置在密码锁右上角
            UpdateHintPosition();
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
        // 检测玩家是否在交互范围内
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool newInRange = distance <= interactionRange;
            
            if (newInRange != playerInRange)
            {
                playerInRange = newInRange;
                UpdateInteractionHint();
            }
        }
        else
        {
            playerInRange = false;
            UpdateInteractionHint();
        }
        
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
        
        // 实时更新提示位置（如果可见）
        if (playerInRange && !isUnlocked &&
            unlockPanel != null && !unlockPanel.activeSelf)
        {
            if ((interactionHintSprite != null && interactionHintSprite.enabled) ||
                (interactionHintUI != null && interactionHintUI.activeSelf))
            {
                UpdateHintPosition();
            }
        }
        
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.P) && showDebugLogs)
        {
            ShowTestInventory();
        }
    }
    
    // 更新提示位置（使用与对话框相同的位置计算逻辑）
    void UpdateHintPosition()
    {
        if (mainCamera == null || (!interactionHintSprite && !interactionHintUI)) return;
        
        // 如果不跟随玩家，固定在物体上方
        if (!followPlayer && playerTransform != null)
        {
            // 计算物体的"头顶"位置
            Renderer objRenderer = GetComponent<Renderer>();
            Collider objCollider = GetComponent<Collider>();
            float objHeight = 1f; // 默认高度
            
            if (objRenderer != null)
            {
                objHeight = objRenderer.bounds.extents.y;
            }
            else if (objCollider != null)
            {
                objHeight = objCollider.bounds.extents.y;
            }
            
            Vector3 objTopPos = transform.position + Vector3.up * objHeight;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(objTopPos);
            
            if (screenPos.z < 0) screenPos *= -1;
            
            // 设置Sprite提示位置
            if (interactionHintSprite != null && interactionHintSprite.enabled)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
                worldPos.z = transform.position.z;
                interactionHintSprite.transform.position = worldPos + 
                    new Vector3(hintPixelOffsetX * 0.01f, hintPixelOffsetY * 0.01f, 0);
            }
            // 设置UI提示位置
            else if (hintRectTransform != null && interactionHintUI != null && interactionHintUI.activeSelf)
            {
                Vector2 canvasPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    hintRectTransform.parent as RectTransform,
                    screenPos,
                    mainCamera,
                    out canvasPos))
                {
                    // 添加像素偏移
                    canvasPos.x += hintPixelOffsetX;
                    canvasPos.y += hintPixelOffsetY;
                    hintRectTransform.anchoredPosition = canvasPos;
                }
            }
        }
        // 跟随玩家时（与对话框逻辑保持一致）
        else if (playerTransform != null)
        {
            Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);
            
            if (screenPos.z < 0) screenPos *= -1;
            
            // 设置Sprite提示位置
            if (interactionHintSprite != null && interactionHintSprite.enabled)
            {
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
                worldPos.z = transform.position.z;
                interactionHintSprite.transform.position = worldPos + 
                    new Vector3(hintPixelOffsetX * 0.01f, hintPixelOffsetY * 0.01f, 0);
            }
            // 设置UI提示位置
            else if (hintRectTransform != null && interactionHintUI != null && interactionHintUI.activeSelf)
            {
                Vector2 canvasPos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    hintRectTransform.parent as RectTransform,
                    screenPos,
                    mainCamera,
                    out canvasPos))
                {
                    // 添加像素偏移
                    canvasPos.x += hintPixelOffsetX;
                    canvasPos.y += hintPixelOffsetY;
                    hintRectTransform.anchoredPosition = canvasPos;
                }
            }
        }
    }
    
    // 显示/隐藏提示
    void ShowInteractionHint(bool show)
    {
        if (interactionHintUI != null)
            interactionHintUI.SetActive(show);
        
        if (interactionHintSprite != null)
            interactionHintSprite.enabled = show;
        
        // 显示时更新位置
        if (show)
        {
            UpdateHintPosition();
        }
    }
    
    // 更新交互提示状态
    void UpdateInteractionHint()
    {
        if (isUnlocked)
        {
            ShowInteractionHint(false);
            return;
        }
        
        if (unlockPanel != null && unlockPanel.activeSelf)
        {
            ShowInteractionHint(false);
        }
        else
        {
            ShowInteractionHint(playerInRange);
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
        if (playerTransform == null || mainCamera == null || uiRect == null) return;
        
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
        
        // 隐藏交互提示
        ShowInteractionHint(false);
        
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
            messageText.text = $"输入密码（最多{password.Length}位）：";
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
        
        // 如果玩家还在范围内且未解锁，显示提示
        if (playerInRange && !isUnlocked)
        {
            ShowInteractionHint(true);
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
        
        // 隐藏交互提示
        ShowInteractionHint(false);
        
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
    
    // 显示成功弹窗
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
            GUI.Label(new Rect(10, 70, 400, 100), $"交互键: {interactKey}, 提交键: {submitKey}", style);
        }
    }
}
