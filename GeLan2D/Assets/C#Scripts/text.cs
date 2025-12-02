using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueScreenCenter : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject dialoguePanel;
    public Text dialogueText;
    
    [Header("中央位置设置")]
    public float verticalOffset = 100f;  // 从角色位置向上的偏移量
    public bool followPlayer = true;     // 对话框是否跟随玩家
    
    [Header("对话文件")]
    public TextAsset dialogueFile;
    
    [Header("对话设置")]
    public KeyCode interactKey = KeyCode.R;
    public float interactionRange = 3f;
    
    private List<string> textList = new List<string>();
    private int currentIndex = 0;
    private bool isDialogueActive = false;
    private RectTransform panelRect;
    private Camera mainCamera;
    private Transform playerTransform;
    
    void Start()
    {
        // 获取组件
        if (dialoguePanel != null)
        {
            panelRect = dialoguePanel.GetComponent<RectTransform>();
            SetupScreenCenterPosition();
            dialoguePanel.SetActive(false);
        }
        
        mainCamera = Camera.main;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 加载对话
        if (dialogueFile != null)
        {
            LoadDialogue();
        }
    }
    
    void SetupScreenCenterPosition()
    {
        if (panelRect == null) return;
        
        // 设置锚点为屏幕中心
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        
        // 初始位置在屏幕中心
        panelRect.anchoredPosition = new Vector2(0, verticalOffset);
    }
    
    void Update()
    {
        // 检测玩家交互
        if (IsPlayerInRange() && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }
        
        // 如果在对话中
        if (isDialogueActive)
        {
            // 更新对话框位置（基于角色位置）
            UpdateDialoguePosition();
            
            // 处理对话输入
            if (Input.GetKeyDown(KeyCode.R))
            {
                NextLine();
            }
            
            // 按ESC退出
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
            
            // 玩家离开范围时结束对话
            if (!IsPlayerInRange())
            {
                EndDialogue();
            }
        }
    }
    
    void UpdateDialoguePosition()
    {
        if (panelRect == null || mainCamera == null) return;
        
        // 决定参考目标：跟随玩家或固定NPC
        Transform referenceTarget = followPlayer ? playerTransform : transform;
        if (referenceTarget == null) return;
        
        // 获取参考目标在屏幕上的位置
        Vector3 screenPos = mainCamera.WorldToScreenPoint(referenceTarget.position);
        
        // 如果目标在屏幕外，使用默认位置
        if (screenPos.z < 0)
        {
            panelRect.anchoredPosition = new Vector2(0, verticalOffset);
            return;
        }
        
        // 转换为Canvas坐标
        Vector2 canvasPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect.parent as RectTransform,
            screenPos,
            mainCamera,
            out canvasPos))
        {
            // 在角色位置的基础上，向上偏移
            // X轴保持屏幕中央，Y轴以角色位置为基准
            panelRect.anchoredPosition = new Vector2(0, canvasPos.y + verticalOffset);
        }
    }
    
    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }
    
    void StartDialogue()
    {
        if (textList.Count == 0 || dialoguePanel == null || dialogueText == null)
        {
            Debug.LogWarning("无法开始对话：组件缺失");
            return;
        }
        
        isDialogueActive = true;
        currentIndex = 0;
        
        // 显示对话框
        dialoguePanel.SetActive(true);
        
        // 立即更新位置
        UpdateDialoguePosition();
        
        // 显示第一句对话
        dialogueText.text = textList[currentIndex];
        currentIndex++;
    }
    
    void NextLine()
    {
        if (currentIndex >= textList.Count)
        {
            EndDialogue();
            return;
        }
        
        dialogueText.text = textList[currentIndex];
        currentIndex++;
    }
    
    void EndDialogue()
    {
        isDialogueActive = false;
        currentIndex = 0;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    void LoadDialogue()
    {
        textList.Clear();
        
        if (dialogueFile != null)
        {
            string[] lines = dialogueFile.text.Split('\n');
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    textList.Add(line.Trim());
                }
            }
        }
    }
    
    // 可视化交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}