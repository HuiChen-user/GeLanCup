using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class NPCDialogue2D : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject dialoguePanel;
    public Text dialogueText;

    [Header("2D位置设置")]
    public float verticalOffset = 100f;
    public bool followPlayer = true;
    public float interactionRange = 3f;

    [Header("交互模式设置")]
    public bool autoTrigger = false;
    public bool destroyAfterAutoDialogue = true;
    public KeyCode startKey = KeyCode.E;
    public KeyCode nextKey = KeyCode.R;

    [Header("ESC退出设置")]
    public bool allowESCToExit = true;
    public bool resetOnESCExit = true;

    [Header("玩家控制设置")]
    public bool freezePlayerDuringDialogue = true;

    [Header("2D碰撞检测")]
    public CircleCollider2D interactionArea;
    public bool useTriggerDetection = true;

    [Header("对话文件")]
    public TextAsset dialogueFile;

    [Header("2D提示设置")]
    public GameObject interactionHintUI;
    public SpriteRenderer interactionHintSprite;
    public float hintPixelOffsetX = 20f; // 水平偏移
    public float hintPixelOffsetY = 20f; // 垂直偏移

    [Header("自动对话设置")]
    public float autoTriggerDelay = 0.5f;
    private bool isAutoTriggering = false;
    private bool hasAutoDialogueCompleted = false;

    [Header("打字机效果设置")]
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;
    public AudioClip typeSound;

    // ========== 新增：电视和茶几的交互功能 ==========
    [Header("电视/茶几交互设置")]
    public bool isTV = false;           // 是否为电视
    public bool isCoffeeTable = false;  // 是否为茶几
    
    // 场景对象切换相关
    [Header("场景对象切换")]
    public GameObject beforeObject;     // Before SofaAndTable
    public GameObject afterObject;      // After SofaAndTable
    
    // 交互状态
    private static bool hasInteractedWithTV = false;
    private static bool hasInteractedWithTable = false;
    
    // 单例实例
    private static NPCDialogue2D tvInstance;
    private static NPCDialogue2D tableInstance;

    // 私有变量
    private List<string> textList = new List<string>();
    private int currentIndex = 0;
    private bool isDialogueActive = false;
    private RectTransform panelRect;
    private Camera mainCamera;
    private Transform playerTransform;
    private bool playerInRange = false;
    private Component playerControlComponent;
    private bool wasPlayerMovable = true;
    private AudioSource typeAudioSource;
    private Coroutine typingCoroutine;

    // 新增：用于Canvas坐标转换
    private Canvas uiCanvas;
    private RectTransform canvasRect;

    // 新增：用于UI提示
    private RectTransform hintRectTransform; // 如果是UI类型提示

    void Start()
    {
        InitializeComponents();
        SetupInteractionArea();
        LoadDialogue();
        FindPlayerControlComponent();
        
        // 新增：设置单例
        if (isTV)
        {
            tvInstance = this;
            Debug.Log("✅ 已设置为电视交互器");
        }
        else if (isCoffeeTable)
        {
            tableInstance = this;
            Debug.Log("✅ 已设置为茶几交互器");
        }
        
        // 确保Before和After状态正确
        EnsureSceneObjectsState();
    }

    // 新增：确保场景对象初始状态
    void EnsureSceneObjectsState()
    {
        if (beforeObject != null)
            beforeObject.SetActive(true);
        if (afterObject != null)
            afterObject.SetActive(false);
    }

    void InitializeComponents()
    {
        mainCamera = Camera.main;
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 设置UI
        if (dialoguePanel != null)
        {
            panelRect = dialoguePanel.GetComponent<RectTransform>();
            // 关键：设置正确的锚点和轴心（像原版那样）
            SetupPanelAnchor();
            dialoguePanel.SetActive(false);

            // 查找Canvas
            uiCanvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (uiCanvas != null)
            {
                canvasRect = uiCanvas.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("未找到Canvas，尝试查找场景中的Canvas");
                uiCanvas = FindObjectOfType<Canvas>();
                if (uiCanvas != null)
                {
                    canvasRect = uiCanvas.GetComponent<RectTransform>();
                    // 将panel移到Canvas下
                    dialoguePanel.transform.SetParent(uiCanvas.transform);
                    SetupPanelAnchor(); // 重新设置锚点
                }
            }
        }

        // 设置提示
        if (interactionHintUI != null)
        {
            interactionHintUI.SetActive(false);
            // 如果是UI类型，获取RectTransform
            hintRectTransform = interactionHintUI.GetComponent<RectTransform>();
        }
        if (interactionHintSprite != null)
        {
            interactionHintSprite.enabled = false;
        }

        // 初始化打字机音效组件
        if (typeSound != null)
        {
            typeAudioSource = gameObject.AddComponent<AudioSource>();
            typeAudioSource.clip = typeSound;
            typeAudioSource.volume = 0.5f;
            typeAudioSource.playOnAwake = false;
        }
    }

    // 新增：专门更新提示位置的方法（直接定位到玩家头顶）
    void UpdateHintPosition()
    {
        if (mainCamera == null || playerTransform == null ||
            (!interactionHintSprite && !interactionHintUI)) return;

        // 计算玩家头顶位置
        Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);
        if (screenPos.z < 0) screenPos *= -1;

        // 设置Sprite提示位置
        if (interactionHintSprite != null && interactionHintSprite.enabled)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = transform.position.z;
            // 添加偏移量
            worldPos.x += hintPixelOffsetX * 0.01f;
            worldPos.y += hintPixelOffsetY * 0.01f;
            interactionHintSprite.transform.position = worldPos;
        }

        // 设置UI提示位置
        else if (hintRectTransform != null && interactionHintUI != null &&
                 interactionHintUI.activeSelf)
        {
            Vector2 canvasPos;
            Camera uiCamera = uiCanvas != null ? uiCanvas.worldCamera : mainCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hintRectTransform.parent as RectTransform,
                screenPos,
                uiCamera,
                out canvasPos))
            {
                // 直接设置到玩家头顶位置，添加像素偏移
                canvasPos.x += hintPixelOffsetX;
                canvasPos.y += hintPixelOffsetY;
                hintRectTransform.anchoredPosition = canvasPos;
            }
        }
    }

    // 新增：设置面板锚点（像原版那样）
    void SetupPanelAnchor()
    {
        if (panelRect == null) return;

        // 设置为屏幕中心锚点
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        // 初始位置
        panelRect.anchoredPosition = new Vector2(0, verticalOffset);
    }

    void FindPlayerControlComponent()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 尝试常见的玩家控制脚本名称
        string[] commonControlNames = {
            "PlayerController", "PlayerMovement", "CharacterController",
            "PlayerControl", "MovementController", "Player"
        };

        foreach (string name in commonControlNames)
        {
            Component comp = player.GetComponent(name);
            if (comp != null)
            {
                playerControlComponent = comp;
                return;
            }
        }

        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        if (scripts.Length > 0)
        {
            playerControlComponent = scripts[0];
        }
    }

    void Update()
    {
        if (!useTriggerDetection)
        {
            CheckPlayerDistance();
        }

        // 根据交互模式处理输入
        if (playerInRange && !hasAutoDialogueCompleted)
        {
            if (!autoTrigger && !isDialogueActive)
            {
                if (Input.GetKeyDown(startKey))
                {
                    StartDialogue();
                }
            }
        }

        // 对话进行中
        if (isDialogueActive)
        {
            UpdateDialoguePosition();

            // 按R继续下一句
            if (Input.GetKeyDown(nextKey))
            {
                NextLine();
            }

            // 按ESC退出
            if (allowESCToExit && Input.GetKeyDown(KeyCode.Escape))
            {
                OnESCExit();
            }
        }

        // 实时更新提示位置（如果可见）
        if (playerInRange && !isDialogueActive && !hasAutoDialogueCompleted)
        {
            if ((interactionHintSprite != null && interactionHintSprite.enabled) ||
                (interactionHintUI != null && interactionHintUI.activeSelf))
            {
                UpdateHintPosition();
            }
        }
        
        // 新增：测试快捷键（仅在编辑器中使用）
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T) && Application.isEditor)
        {
            TestSceneSwitch();
        }
        #endif
    }

    // 新增：测试场景切换
    void TestSceneSwitch()
    {
        Debug.Log("🎮 测试：电视状态=" + hasInteractedWithTV + ", 茶几状态=" + hasInteractedWithTable);
        if (beforeObject != null && afterObject != null)
        {
            Debug.Log("🎮 Before: " + beforeObject.activeSelf + ", After: " + afterObject.activeSelf);
        }
    }

    void OnESCExit()
    {
        isDialogueActive = false;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // ESC退出时不标记为已完成
        if (resetOnESCExit)
        {
            currentIndex = 0;
            // 如果是自动模式，不标记为已完成
            if (autoTrigger)
            {
                hasAutoDialogueCompleted = false;
            }
        }

        // 解冻玩家移动
        if (freezePlayerDuringDialogue)
        {
            FreezePlayer(false);
        }

        // 隐藏对话框
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 显示交互提示（如果是手动模式）
        if (playerInRange && !autoTrigger && !hasAutoDialogueCompleted)
        {
            ShowInteractionHint(true);
        }

        Debug.Log("对话已通过ESC退出");
    }

    void StartDialogue()
    {
        if (textList.Count == 0 || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("无法开始对话：组件缺失");
            return;
        }

        // 新增：记录交互状态
        if (isTV)
        {
            hasInteractedWithTV = true;
            Debug.Log("📺 电视交互已记录");
        }
        else if (isCoffeeTable)
        {
            hasInteractedWithTable = true;
            Debug.Log("🪑 茶几交互已记录");
        }

        isDialogueActive = true;
        currentIndex = 0;

        // 冻结玩家移动
        if (freezePlayerDuringDialogue)
        {
            FreezePlayer(true);
        }

        // 隐藏交互提示
        ShowInteractionHint(false);

        // 显示对话框
        dialoguePanel.SetActive(true);

        // 更新位置
        UpdateDialoguePosition();

        // 播放第一句（带打字机效果）
        if (useTypewriterEffect)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(textList[currentIndex]));
        }
        else
        {
            dialogueText.text = textList[currentIndex];
        }

        currentIndex++;
        
        // 新增：检查是否需要切换场景
        CheckForSceneSwitch();
    }

    void NextLine()
    {
        if (currentIndex >= textList.Count)
        {
            EndDialogue();
            return;
        }

        // 播放下一句（带打字机效果）
        if (useTypewriterEffect)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(textList[currentIndex]));
        }
        else
        {
            dialogueText.text = textList[currentIndex];
        }

        currentIndex++;
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // 解冻玩家移动
        if (freezePlayerDuringDialogue)
        {
            FreezePlayer(false);
        }

        // 正常结束：记录对话完成状态
        if (autoTrigger)
        {
            hasAutoDialogueCompleted = true;
            // 如果设置了自动对话后删除物体
            if (destroyAfterAutoDialogue)
            {
                StartCoroutine(DestroyAfterDelay(0.5f));
            }
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 如果玩家还在范围内且不是自动触发模式，显示提示
        if (playerInRange && !autoTrigger && !hasAutoDialogueCompleted)
        {
            ShowInteractionHint(true);
        }

        currentIndex = 0;
    }

    // 新增：检查是否需要切换场景
    void CheckForSceneSwitch()
    {
        // 确保有电视和茶几实例
        if (tvInstance == null || tableInstance == null)
        {
            Debug.LogWarning("⚠️ 电视或茶几实例未找到，请检查场景设置");
            return;
        }

        // 检查两个交互都已完成
        if (hasInteractedWithTV && hasInteractedWithTable)
        {
            // 使用电视实例的对象引用（确保一致）
            GameObject beforeObj = tvInstance.beforeObject;
            GameObject afterObj = tvInstance.afterObject;
            
            if (beforeObj != null && afterObj != null)
            {
                Debug.Log("🎯 切换条件满足！开始切换场景对象...");
                beforeObj.SetActive(false);
                afterObj.SetActive(true);
                Debug.Log("✅ 场景切换完成");
            }
            else
            {
                Debug.LogError("❌ 切换对象未设置！请确保电视脚本中的Before和After对象已正确赋值");
            }
        }
        else
        {
            Debug.Log($"⏳ 等待交互：电视={hasInteractedWithTV}, 茶几={hasInteractedWithTable}");
        }
    }

    void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 1. 通过反射控制玩家移动状态
        if (playerControlComponent != null && freezePlayerDuringDialogue)
        {
            System.Type type = playerControlComponent.GetType();
            FieldInfo canMoveField = type.GetField("canMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo canMoveProp = type.GetProperty("CanMove", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (canMoveField != null)
            {
                if (freeze)
                {
                    object currentValue = canMoveField.GetValue(playerControlComponent);
                    if (currentValue is bool)
                    {
                        wasPlayerMovable = (bool)currentValue;
                    }
                    canMoveField.SetValue(playerControlComponent, false);
                }
                else
                {
                    canMoveField.SetValue(playerControlComponent, wasPlayerMovable);
                }
            }
            else if (canMoveProp != null && canMoveProp.CanWrite)
            {
                if (freeze)
                {
                    object currentValue = canMoveProp.GetValue(playerControlComponent);
                    if (currentValue is bool)
                    {
                        wasPlayerMovable = (bool)currentValue;
                    }
                    canMoveProp.SetValue(playerControlComponent, false);
                }
                else
                {
                    canMoveProp.SetValue(playerControlComponent, wasPlayerMovable);
                }
            }
        }

        // 2. 冻结物理
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (freeze)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // 3. 禁用所有玩家脚本（备用方案）
        if (freeze)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != this)
                {
                    script.enabled = false;
                }
            }
        }
        else
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = true;
            }
        }
    }

    // 打字机核心协程（逐字显示文本）
    IEnumerator TypeText(string textToType)
    {
        dialogueText.text = "";
        foreach (char c in textToType.ToCharArray())
        {
            dialogueText.text += c;
            if (typeAudioSource != null && typeSound != null)
            {
                typeAudioSource.PlayOneShot(typeSound);
            }
            yield return new WaitForSeconds(typewriterSpeed);
        }
        typingCoroutine = null;
    }

    // 关键修复：正确的Canvas坐标计算方法（像原版那样）
    void UpdateDialoguePosition()
    {
        if (dialoguePanel == null || panelRect == null || mainCamera == null)
            return;

        // 如果不跟随玩家，固定在屏幕中央
        if (!followPlayer)
        {
            panelRect.anchoredPosition = new Vector2(0, verticalOffset);
            return;
        }

        // 跟随玩家时
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        // 获取玩家在屏幕上的位置
        Vector3 playerHeadPos = playerTransform.position + Vector3.up * 1.5f;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(playerHeadPos);

        // 如果玩家在屏幕外，使用默认位置
        if (screenPos.z < 0)
        {
            panelRect.anchoredPosition = new Vector2(0, verticalOffset);
            return;
        }

        // 转换到Canvas本地坐标系（关键步骤）
        if (uiCanvas != null && canvasRect != null)
        {
            Vector2 canvasPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                uiCanvas.worldCamera ?? mainCamera,
                out canvasPos))
            {
                // 像原版那样：X轴居中，Y轴基于玩家位置
                panelRect.anchoredPosition = new Vector2(0, canvasPos.y + verticalOffset);
            }
            else
            {
                // 转换失败，使用备用位置
                panelRect.anchoredPosition = new Vector2(0, verticalOffset);
            }
        }
        else
        {
            // 没有Canvas，使用简单的屏幕位置（备用方案）
            panelRect.anchoredPosition = new Vector2(0, screenPos.y + verticalOffset);
        }
    }

    // 以下为原脚本的辅助方法
    void SetupInteractionArea()
    {
        if (interactionArea == null)
        {
            interactionArea = gameObject.AddComponent<CircleCollider2D>();
            interactionArea.isTrigger = useTriggerDetection;
            interactionArea.radius = interactionRange;
        }
        else
        {
            interactionArea.isTrigger = useTriggerDetection;
            interactionArea.radius = interactionRange;
        }
    }

    void LoadDialogue()
    {
        if (dialogueFile != null)
        {
            string fullText = dialogueFile.text;
            string[] lines = fullText.Split(new string[] { "\n", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            textList.AddRange(lines);
        }
        else
        {
            Debug.LogWarning("未指定对话文件！");
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (useTriggerDetection && other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateInteractionHint();

            // 自动触发对话
            if (autoTrigger && !hasAutoDialogueCompleted && !isDialogueActive)
            {
                StartCoroutine(AutoTriggerDialogue());
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (useTriggerDetection && other.CompareTag("Player"))
        {
            playerInRange = false;
            UpdateInteractionHint();
        }
    }

    IEnumerator AutoTriggerDialogue()
    {
        if (isAutoTriggering) yield break;
        isAutoTriggering = true;
        yield return new WaitForSeconds(autoTriggerDelay);
        if (playerInRange && !hasAutoDialogueCompleted && !isDialogueActive)
        {
            StartDialogue();
        }
        isAutoTriggering = false;
    }

    void UpdateInteractionHint()
    {
        if (isDialogueActive || hasAutoDialogueCompleted || autoTrigger)
        {
            ShowInteractionHint(false);
        }
        else
        {
            ShowInteractionHint(playerInRange);
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

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    // ========== 新增：调试工具 ==========
    
    [ContextMenu("手动触发电视交互")]
    void DebugMarkTVInteracted()
    {
        if (isTV)
        {
            hasInteractedWithTV = true;
            Debug.Log("🎮 手动：电视交互已记录");
            CheckForSceneSwitch();
        }
    }
    
    [ContextMenu("手动触发茶几交互")]
    void DebugMarkTableInteracted()
    {
        if (isCoffeeTable)
        {
            hasInteractedWithTable = true;
            Debug.Log("🎮 手动：茶几交互已记录");
            CheckForSceneSwitch();
        }
    }
    
    [ContextMenu("重置交互状态")]
    void DebugResetInteractions()
    {
        hasInteractedWithTV = false;
        hasInteractedWithTable = false;
        
        if (tvInstance != null && tvInstance.beforeObject != null)
            tvInstance.beforeObject.SetActive(true);
        if (tvInstance != null && tvInstance.afterObject != null)
            tvInstance.afterObject.SetActive(false);
            
        Debug.Log("🔄 所有交互状态已重置");
    }
    
    [ContextMenu("显示当前状态")]
    void DebugShowStatus()
    {
        Debug.Log($"📊 电视交互: {hasInteractedWithTV}");
        Debug.Log($"📊 茶几交互: {hasInteractedWithTable}");
        Debug.Log($"📊 电视实例: {(tvInstance != null ? "存在" : "缺失")}");
        Debug.Log($"📊 茶几实例: {(tableInstance != null ? "存在" : "缺失")}");
        
        if (tvInstance != null)
        {
            Debug.Log($"📊 Before: {(tvInstance.beforeObject != null ? tvInstance.beforeObject.activeSelf : "null")}");
            Debug.Log($"📊 After: {(tvInstance.afterObject != null ? tvInstance.afterObject.activeSelf : "null")}");
        }
    }
}
