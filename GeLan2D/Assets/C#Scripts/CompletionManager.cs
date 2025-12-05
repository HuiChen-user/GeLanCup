using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletionManager : MonoBehaviour
{
    [Header("要显示的物体")]
    public GameObject hiddenObject; // 拖拽Hierarchy中默认隐藏的图片/物体
    
    [Header("音效设置")]
    public AudioClip completionSound;
    public float soundVolume = 1.0f;
    
    [Header("交互物体")]
    public NPCDialogue2D tvObject;     // 拖拽电视物体
    public NPCDialogue2D otherObject;  // 拖拽合同/茶几物体
    
    [Header("调试")]
    public bool showDebugLogs = true;
    
    // 状态
    private bool tvInteracted = false;
    private bool otherInteracted = false;
    private bool hasTriggered = false;
    
    void Start()
    {
        // 确保开始时隐藏物体
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(false);
        }
        
        // 重置状态
        ResetState();
        
        Debug.Log("完成管理器已初始化");
    }
    
    void Update()
    {
        // 检查电视是否被交互
        if (!tvInteracted && tvObject != null)
        {
            // 通过反射获取交互状态
            var tvField = tvObject.GetType().GetField("hasInteractedWithTV", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (tvField != null)
            {
                tvInteracted = (bool)tvField.GetValue(null);
            }
        }
        
        // 检查其他物体是否被交互
        if (!otherInteracted && otherObject != null)
        {
            // 需要根据物体类型检查不同的字段
            var tableField = otherObject.GetType().GetField("hasInteractedWithTable", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (tableField != null)
            {
                otherInteracted = (bool)tableField.GetValue(null);
            }
        }
        
        // 检查是否满足条件且未触发
        if (!hasTriggered && tvInteracted && otherInteracted)
        {
            TriggerCompletion();
        }
    }
    
    void TriggerCompletion()
    {
        hasTriggered = true;
        
        // 显示物体
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(true);
            Debug.Log($"🎉 显示物体: {hiddenObject.name}");
        }
        
        // 播放音效
        if (completionSound != null)
        {
            AudioSource.PlayClipAtPoint(completionSound, Camera.main.transform.position, soundVolume);
            Debug.Log("🔊 播放完成音效");
        }
        
        // 可选：添加视觉效果
        StartCoroutine(CompletionEffect());
    }
    
    IEnumerator CompletionEffect()
    {
        // 示例：让显示的物体闪烁几次
        if (hiddenObject != null)
        {
            Renderer renderer = hiddenObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color originalColor = renderer.material.color;
                for (int i = 0; i < 3; i++)
                {
                    renderer.material.color = Color.yellow;
                    yield return new WaitForSeconds(0.2f);
                    renderer.material.color = originalColor;
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }
    
    // 重置状态（用于调试）
    [ContextMenu("重置状态")]
    void ResetState()
    {
        tvInteracted = false;
        otherInteracted = false;
        hasTriggered = false;
        
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(false);
        }
        
        Debug.Log("完成管理器状态已重置");
    }
    
    // 显示当前状态
    [ContextMenu("显示状态")]
    void ShowStatus()
    {
        Debug.Log($"电视交互: {tvInteracted}");
        Debug.Log($"其他交互: {otherInteracted}");
        Debug.Log($"已触发: {hasTriggered}");
    }
}