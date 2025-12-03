using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PasswordLockInteractable : Interactable
{
    [Header("密码设置")]
    public string password = "1234";
    public KeyCode submitKey = KeyCode.Return;   // 提交密码键
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
    
    [Header("成功提示")]
    public GameObject successPopup;              // 成功提示弹窗
    public Text successMessageText;              // 成功消息文本
    public float popupDisplayTime = 2f;          // 弹窗显示时间
    public float popupVerticalOffset = 150f;     // 弹窗垂直偏移
    
    [Header("位置设置")]
    public float panelVerticalOffset = 100f;     // 输入面板垂直偏移
    public bool followPlayer = true;
    
    [Header("门解锁")]
    public GameObject doorToUnlock;
    
    [Header("测试模式")]
    public bool useTestMode = true;
    public bool showDebugLogs = false;
    
    // 临时物品存储（用于测试）
    private static Dictionary<string, int> testInventory = new Dictionary<string, int>();
    
    private bool isUnlocked = false;
    private Transform playerTransform;
    private RectTransform panelRect;
    private RectTransform popupRect;
    private Camera mainCamera;
    private bool isShowingSuccessPopup = false;
    private Coroutine panelInputCoroutine;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;
        
        // 设置输入框字符限制
        if (passwordInput != null)
        {
            passwordInput.characterLimit = password.Length;
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
        
        // 自动查找背包管理器
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
    }
    
    // 重写Interactable的OnPlayerEnter：根据解锁状态显示不同提示
    protected override void OnPlayerEnter()
    {
        if (isUnlocked)
        {
            return;
        }
        
        base.OnPlayerEnter();
    }
    
    // 重写Interactable的OnInteract：按E键打开密码面板
    protected override void OnInteract()
    {
        if (isUnlocked) return;
        
        ShowUnlockPanel();
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
            messageText.text = $"输入密码（最多{password.Length}位）：";
            messageText.color = Color.white;
        }
        
        // 开始监听面板输入
        if (panelInputCoroutine != null)
        {
            StopCoroutine(panelInputCoroutine);
        }
        panelInputCoroutine = StartCoroutine(HandlePanelInput());
    }
    
    // 处理面板输入协程
    IEnumerator HandlePanelInput()
    {
        while (unlockPanel != null && unlockPanel.activeSelf && !isUnlocked)
        {
            // 按提交键提交密码
            if (Input.GetKeyDown(submitKey))
            {
                CheckPassword();
            }
            
            // ESC键关闭面板
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseUnlockPanel();
            }
            
            // 更新UI位置
            if (followPlayer && panelRect != null && mainCamera != null)
            {
                UpdateUIPosition(panelRect, panelVerticalOffset);
            }
            
            if (successPopup != null && successPopup.activeSelf && followPlayer && popupRect != null && mainCamera != null)
            {
                UpdateUIPosition(popupRect, popupVerticalOffset);
            }
            
            yield return null;
        }
    }
    
    // 关闭解锁面板
    void CloseUnlockPanel()
    {
        if (unlockPanel != null)
        {
            unlockPanel.SetActive(false);
        }
        
        if (panelInputCoroutine != null)
        {
            StopCoroutine(panelInputCoroutine);
            panelInputCoroutine = null;
        }
    }
    
    // 输入框结束编辑监听（处理Enter键提交）
    void OnPasswordInputEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckPassword();
        }
    }
    
    // 检查密码
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
    }
    
    // 解锁成功处理
    void OnUnlockSuccess()
    {
        isUnlocked = true;
        
        CloseUnlockPanel();
        ShowSuccessPopup($"解开了！获得 {rewardItemName} ×{rewardAmount}");
        
        if (useTestMode)
        {
            GiveRewardToPlayer_Test();
        }
        else
        {
            GiveRewardToPlayer_Real();
        }
        
        if (doorToUnlock != null)
        {
            doorToUnlock.SetActive(false);
        }
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
        
        if (InteractionHintManager.Instance != null)
        {
            InteractionHintManager.Instance.HideHint();
        }
        
        if (playerInRange)
        {
            InteractionHintManager.Instance?.HideHint();
        }
    }
    
    // 显示成功弹窗
    void ShowSuccessPopup(string message)
    {
        if (isShowingSuccessPopup) return;
        
        if (successPopup != null)
        {
            if (successMessageText != null)
            {
                successMessageText.text = message;
                successMessageText.color = Color.green;
            }
            
            if (popupRect != null && mainCamera != null && playerTransform != null)
            {
                UpdateUIPosition(popupRect, popupVerticalOffset);
            }
            
            successPopup.SetActive(true);
            isShowingSuccessPopup = true;
            
            StartCoroutine(HidePopupAfterDelay(popupDisplayTime));
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = Color.green;
                StartCoroutine(ClearMessageAfterDelay(2f));
            }
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
        isShowingSuccessPopup = false;
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
    
    // 重写离开范围方法
    protected override void OnPlayerExit()
    {
        if (!isUnlocked)
        {
            base.OnPlayerExit();
        }
    }
    
    // 在编辑器中可视化交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}