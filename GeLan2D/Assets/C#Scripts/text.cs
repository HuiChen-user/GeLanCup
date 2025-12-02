using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI组件 - 必须赋值！")]
    public Text textLabel;
    public GameObject dialoguePanel; // 添加专门的对话面板
    
    [Header("对话文件")]
    public TextAsset dialogueFile;
    
    [Header("对话设置")]
    public KeyCode interactKey = KeyCode.R;
    public float interactionRange = 3f;
    
    private List<string> textList = new List<string>();
    private int currentIndex = 0;
    private bool isPlayerInRange = false;
    private bool isDialogueActive = false;

    void Awake()
    {
        // 初始化时检查组件
        InitializeComponents();
        
        // 加载对话文件
        if (dialogueFile != null)
        {
            GetTextFromFile(dialogueFile);
        }
        else
        {
            Debug.LogWarning($"NPC {gameObject.name} 没有设置对话文件！");
        }
    }
    
    void InitializeComponents()
    {
        // 如果未手动赋值，尝试自动查找
        if (textLabel == null)
        {
            // 查找场景中的Text组件
            textLabel = GameObject.Find("DialogueText")?.GetComponent<Text>();
            if (textLabel == null)
            {
                Debug.LogError("找不到Text组件！请手动将Text对象拖拽到textLabel字段");
            }
        }
        
        // 初始化隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            // 如果textLabel有父对象，使用父对象作为面板
            if (textLabel != null && textLabel.transform.parent != null)
            {
                dialoguePanel = textLabel.transform.parent.gameObject;
                dialoguePanel.SetActive(false);
            }
        }
    }

    void Update()
    {
        // 检测玩家距离
        CheckPlayerDistance();
        
        // 如果在对话中，处理对话逻辑
        if (isDialogueActive)
        {
            HandleDialogueInput();
        }
    }

    void CheckPlayerDistance()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) 
        {
            Debug.LogWarning("找不到Player对象！请确保有Tag为Player的游戏对象");
            return;
        }
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerInRange = distance <= interactionRange;
        
        // 玩家在范围内按R键开始对话
        if (isPlayerInRange && Input.GetKeyDown(interactKey) && !isDialogueActive)
        {
            StartDialogue();
        }
        
        // 玩家离开范围时结束对话
        if (!isPlayerInRange && isDialogueActive)
        {
            EndDialogue();
        }
    }

    void StartDialogue()
    {
        // 安全检查
        if (textList.Count == 0)
        {
            Debug.LogWarning("对话列表为空！请检查对话文件");
            return;
        }
        
        if (textLabel == null)
        {
            Debug.LogError("textLabel为空！无法显示对话");
            return;
        }
        
        isDialogueActive = true;
        currentIndex = 0;
        
        // 激活对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        else if (textLabel != null && textLabel.transform.parent != null)
        {
            textLabel.transform.parent.gameObject.SetActive(true);
        }
        
        // 显示第一句对话
        textLabel.text = textList[currentIndex];
        Debug.Log($"开始对话：{textList[currentIndex]}");
        
        currentIndex++;
    }

    void HandleDialogueInput()
    {
        // 安全检查
        if (textLabel == null || textList == null)
        {
            Debug.LogError("textLabel 或 textList 为空！");
            EndDialogue();
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentIndex >= textList.Count)
            {
                EndDialogue();
                return;
            }
            
            // 显示下一句对话
            textLabel.text = textList[currentIndex];
            Debug.Log($"下一句对话：{textList[currentIndex]}");
            
            currentIndex++;
        }
        
        // 按ESC键可以随时退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        currentIndex = 0;
        
        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else if (textLabel != null && textLabel.transform.parent != null)
        {
            textLabel.transform.parent.gameObject.SetActive(false);
        }
        
        Debug.Log("对话结束");
    }

    void GetTextFromFile(TextAsset file)
    {
        textList.Clear();
        currentIndex = 0;

        if (file != null)
        {
            var lineData = file.text.Split('\n');
            Debug.Log($"从文件 {file.name} 读取到 {lineData.Length} 行");

            foreach (var line in lineData)
            {
                // 跳过空行和注释行
                string trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                {
                    textList.Add(trimmedLine);
                }
            }
            
            Debug.Log($"成功加载 {textList.Count} 句对话");
        }
        else
        {
            Debug.LogError("对话文件为空！");
        }
    }
    
    // 调试方法：在编辑器中测试对话
    [ContextMenu("测试对话")]
    void TestDialogue()
    {
        if (Application.isPlaying)
        {
            StartDialogue();
        }
        else
        {
            Debug.Log("请在运行模式下测试对话");
        }
    }
    
    // 可视化交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}