using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("交互提示")]
    public Sprite customHintSprite; // 可选的自定义提示图标

    protected bool playerInRange = false;

    // 当玩家进入触发范围
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            OnPlayerEnter();
        }
    }

    // 当玩家离开触发范围
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            OnPlayerExit();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    // 玩家进入时的处理（显示提示）
    protected virtual void OnPlayerEnter()
    {
        // 通知管理器显示提示，可使用自定义图标
        if (InteractionHintManager.Instance != null)
            InteractionHintManager.Instance.ShowHint(customHintSprite);
    }

    // 玩家离开时的处理（隐藏提示）
    protected virtual void OnPlayerExit()
    {
        if (InteractionHintManager.Instance != null)
            InteractionHintManager.Instance.HideHint();
    }

    // 核心交互逻辑（由子类实现）
    protected abstract void OnInteract();
}
