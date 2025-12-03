using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Interactable
{
    [Header("物品设置")]
    public ItemData itemData = new ItemData();  // 物品数据
    public Sprite itemSprite;                   // 物品图片
    
    protected override void OnInteract()
    {
        // 添加到背包
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemData, itemSprite);
        }
        
        // 播放音效（如果有）
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.clip != null)
        {
            audio.Play();
        }
        
        // 隐藏自己（可以先隐藏渲染器，延迟销毁）
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // 禁用碰撞器，防止重复拾取
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // 延迟销毁，确保音效播放完毕
        Destroy(gameObject, 1f);
        
        Debug.Log($"拾取了: {itemData.itemName}");
    }
    
    // 进入范围显示提示
    protected override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
    }
}