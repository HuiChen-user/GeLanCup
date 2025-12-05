using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHintManager : MonoBehaviour
{
    public static InteractionHintManager Instance; // 单例，方便访问

    [Header("提示设置")]
    public SpriteRenderer hintSpriteRenderer; // 拖拽上面创建的InteractionHint物体
    public Sprite exclamationSprite; // 拖拽感叹号图片资源

    [Header("偏移位置")]
    public Vector3 hintOffset = new Vector3(0.5f, 0.5f, 0); // 相对于玩家的偏移

    void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // 关键！
    }
    else
    {
        Destroy(gameObject);
    }
}

    void Start()
    {
        // 如果没手动指定，尝试查找
        if (hintSpriteRenderer == null)
            hintSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        HideHint(); // 确保开始隐藏
    }

    // 显示提示（可指定不同图标，默认为感叹号）
    public void ShowHint(Sprite customSprite = null)
    {
        if (hintSpriteRenderer != null)
        {
            hintSpriteRenderer.sprite = customSprite != null ? customSprite : exclamationSprite;
            hintSpriteRenderer.enabled = true;
        }
    }

    // 隐藏提示
    public void HideHint()
    {
        if (hintSpriteRenderer != null)
            hintSpriteRenderer.enabled = false;
    }

    // 更新提示位置（如果需要动态调整）
    public void UpdateHintPosition(Vector3 playerPosition)
    {
        if (hintSpriteRenderer != null)
            hintSpriteRenderer.transform.position = playerPosition + hintOffset;
    }

    void LateUpdate()
    {
        // 每帧更新位置以确保跟随玩家（如果玩家移动）
        if (hintSpriteRenderer != null && hintSpriteRenderer.enabled)
        {
            UpdateHintPosition(transform.position);
        }
    }
}

