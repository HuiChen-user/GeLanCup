using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("门连接设置")]
    public int linkedRoomIndex;
    public int doorConnectionIndex;
    
    [Header("钥匙需求")]
    public bool requiresKey = true;          // 是否需要钥匙
    public int requiredKeyID = 1001;         // 需要的钥匙ID
    public Sprite lockedHintSprite;          // 锁住时的提示图标（灰色钥匙）
    
    [Header("状态")]
    public bool isLocked = true;             // 当前是否锁住
    public float lockedHintDuration = 1.5f;  // 锁住提示显示时间
    
    private RoomTeleporter teleporter;
    private bool isShowingLockedHint = false;
    
    void Start()
    {
        teleporter = FindObjectOfType<RoomTeleporter>();
    }
    
    // 重写进入范围方法：总是显示感叹号
    protected override void OnPlayerEnter()
    {
        // 无论是否有钥匙，都显示感叹号
        base.OnPlayerEnter();
    }
    
    // 重写交互方法
    protected override void OnInteract()
    {
        if (isShowingLockedHint) return; // 防止重复触发
        
        // 如果门需要钥匙且被锁住
        if (requiresKey && isLocked)
        {
            // 检查背包是否有钥匙
            if (HasKey())
            {
                // 有钥匙，开门传送
                UnlockAndTeleport();
            }
            else
            {
                // 没有钥匙，显示锁住提示
                StartCoroutine(ShowLockedHint());
            }
        }
        else
        {
            // 不需要钥匙或已解锁，直接传送
            Teleport();
        }
    }
    
    // 检查背包是否有钥匙
    private bool HasKey()
    {
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.HasItem(requiredKeyID);
        }
        return false;
    }
    
    // 解锁并传送
    private void UnlockAndTeleport()
    {
        // 消耗钥匙
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(requiredKeyID);
        }
        
        // 解锁门（以后不再需要钥匙）
        isLocked = false;
        
        // 执行传送
        Teleport();
        
        Debug.Log($"使用钥匙解锁门，钥匙已消耗");
    }
    
    // 传送方法
    private void Teleport()
    {
        if (teleporter != null)
        {
            teleporter.TeleportToSpecificDoor(linkedRoomIndex, doorConnectionIndex);
        }
    }
    
    // 显示锁住提示的协程
    private IEnumerator ShowLockedHint()
    {
        isShowingLockedHint = true;
        
        // 先隐藏当前的感叹号提示
        if (InteractionHintManager.Instance != null)
        {
            InteractionHintManager.Instance.HideHint();
        }
        
        // 短暂显示灰色钥匙
        if (InteractionHintManager.Instance != null && lockedHintSprite != null)
        {
            InteractionHintManager.Instance.ShowHint(lockedHintSprite);
        }
        
        Debug.Log("门被锁住了！需要钥匙才能打开。");
        
        // 等待一段时间
        yield return new WaitForSeconds(lockedHintDuration);
        
        // 重新显示感叹号（如果玩家还在范围内）
        if (playerInRange && InteractionHintManager.Instance != null)
        {
            // 显示默认的感叹号提示
            base.OnPlayerEnter();
        }
        
        isShowingLockedHint = false;
    }
    
    // 重写离开范围方法
    protected override void OnPlayerExit()
    {
        // 重置锁住提示状态
        isShowingLockedHint = false;
        base.OnPlayerExit();
    }
    
    // 可选：在编辑器中可视化钥匙需求
    void OnDrawGizmos()
    {
        if (requiresKey && isLocked)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "LockIcon");
        }
        else if (!requiresKey || !isLocked)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "UnlockIcon");
        }
    }
}