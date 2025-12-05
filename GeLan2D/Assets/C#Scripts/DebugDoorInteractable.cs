using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDoorInteractable : Interactable // 继承自你的 Interactable 基类
{
    [Header("调试信息")]
    public string doorName = "未命名门"; // 在Inspector中给两个门分别起名，如“图书馆门”、“地下室门”
    public bool isLocked = false; // 可以模拟锁住状态
    public int requiredKeyId = 1001; // 如果需要，模拟需要的钥匙ID

    // 当玩家靠近时，基类Interactable会自动调用OnPlayerEnter显示感叹号
    // 当玩家离开时，也会自动隐藏提示

    // 当玩家按下E时，基类会自动调用这个函数（因为它是抽象的）
    protected override void OnInteract()
    {
        // 1. 模拟检查钥匙（可选）
        if (isLocked)
        {
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem(requiredKeyId))
            {
                Debug.Log($"<color=green>[{doorName}]</color> 使用钥匙打开！钥匙ID: {requiredKeyId}");
                isLocked = false;
                // 这里理论上可以播放一个开门动画或音效
            }
            else
            {
                Debug.Log($"<color=orange>[{doorName}]</color> 被锁住了！需要钥匙ID: {requiredKeyId}");
                return; // 如果门锁着，就不执行后续的“传送”调试
            }
        }

        // 2. 这里是核心调试信息，代替传送功能
        Debug.Log($"<color=cyan>[{doorName}]</color> 被玩家互动！此处应传送到下一个场景。");
        Debug.Log($"玩家位置: {transform.position}, 门位置: {transform.position}");

        // 3. 可以在这里触发一个简单的视觉效果，表明互动成功（比如门抖动一下）
        StartCoroutine(DoorFeedbackEffect());
    }

    // 一个简单的视觉效果协程（可选）
    private System.Collections.IEnumerator DoorFeedbackEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f; // 轻微放大
        yield return new WaitForSeconds(0.1f); // 等待0.1秒
        transform.localScale = originalScale; // 恢复原样
    }

    // 如果你希望在编辑器中更直观地看到这两个门
    void OnDrawGizmos()
    {
        // 画一个半透明的立方体表示交互范围（如果碰撞体是Box Collider 2D）
        Gizmos.color = new Color(0, 1, 1, 0.3f); // 青色半透明
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawCube(transform.position + (Vector3)collider.offset, collider.size);
        }
    }
}