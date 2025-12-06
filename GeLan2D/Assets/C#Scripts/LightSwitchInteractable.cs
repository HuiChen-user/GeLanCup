using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchInteractable : Interactable // 继承自你的 Interactable 基类
{
    [Header("灯光控制对象")]
    public GameObject lightGroup; // 拖拽你Hierarchy中代表“灯光”的父对象或单个物体
    public GameObject[] objectsToReveal; // 一个数组，用于存放另外两个需要显示的物体

    [Header("音频设置")]
    public AudioClip switchSound; // 开灯时播放的音效片段
    private AudioSource audioSource; // 用于播放音效的组件

    [Header("开关状态")]
    public bool canBeTurnedOff = false; // 是否允许再次按E关灯？默认只能开一次
    private bool isLightOn = false; // 记录当前灯的状态
    public bool isUsed = false; // 开关是否已被使用过（如果只能开一次）

    void Start()
    {
        // 1. 确保初始状态：灯和要显示的物体都是隐藏的
        if (lightGroup != null)
        {
            lightGroup.SetActive(false);
        }
        foreach (GameObject obj in objectsToReveal)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. 获取或创建AudioSource组件用于播放音效
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // 如果开关物体本身没有AudioSource，就自动添加一个
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f; // 设置一个合适的默认音量
        }
        Debug.Log("灯光开关初始化完成，初始状态：关闭。");
    }

    // 核心：当玩家在范围内按下E时，由基类自动调用
    protected override void OnInteract()
    {
        // 如果开关只能使用一次且已被使用，则直接返回
        if (!canBeTurnedOff && isUsed) return;

        // 切换灯光状态
        ToggleLight();
    }

    // 切换灯光显示/隐藏的方法
    void ToggleLight()
    {
        // 反转状态
        isLightOn = !isLightOn;
        isUsed = true;

        // 1. 控制主灯光
        if (lightGroup != null)
        {
            lightGroup.SetActive(isLightOn);
            Debug.Log($"主灯光 {(isLightOn ? "开启" : "关闭")}");
        }

        // 2. 控制其他需要联动的物体
        foreach (GameObject obj in objectsToReveal)
        {
            if (obj != null)
            {
                obj.SetActive(isLightOn);
            }
        }

        // 3. 播放音效（只在开灯时播放，或者根据设计调整）
        if (isLightOn && switchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchSound);
            Debug.Log("播放开灯音效");
        }
        else if (!isLightOn && switchSound != null && audioSource != null)
        {
            // 如果你有关灯音效，可以在这里播放
            // audioSource.PlayOneShot(switchOffSound);
        }

        // 4. 如果开关只能开一次，使用后可以隐藏交互提示并禁用碰撞体
        if (!canBeTurnedOff && isUsed)
        {
            Debug.Log("开关已使用，禁用后续交互。");
            // 隐藏当前的交互提示
            if (InteractionHintManager.Instance != null)
            {
                InteractionHintManager.Instance.HideHint();
            }
            // 禁用触发器，防止再次交互
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // 可选：在编辑器中可视化交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 假设你的触发器是一个BoxCollider2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null && collider.isTrigger)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)collider.offset, collider.size);
        }
    }
}