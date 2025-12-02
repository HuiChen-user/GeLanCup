using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [Header("房间传送点列表")]
    public RoomDestination[] destinations; // 在Inspector中配置传送点

    [Header("调试")]
    public bool showDebugGizmos = true; // 在场景中显示传送点位置

    private int currentDestinationIndex = 0; // 当前目标索引（初始为0，即第一个房间）
    private RoomCamera roomCamera; // 你之前写的摄像机脚本

    void Start()
    {
        // 自动查找摄像机脚本
        roomCamera = Camera.main.GetComponent<RoomCamera>();
        if (roomCamera == null)
        {
            Debug.LogError("未找到SimpleRoomCamera脚本！请确保它挂在主摄像机上。");
        }

        // 初始传送到第一个点（可选，确保出生位置正确）
        if (destinations.Length > 0)
        {
            TeleportToDestination(0);
        }
    }

    void Update()
    {
        // 检测按E键传送
        if (Input.GetKeyDown(KeyCode.E))
        {
            TeleportToNextRoom();
        }
    }

    // 传送到下一个房间（循环）
    void TeleportToNextRoom()
    {
        if (destinations.Length == 0) return;

        // 计算下一个房间的索引
        currentDestinationIndex = (currentDestinationIndex + 1) % destinations.Length;

        TeleportToDestination(currentDestinationIndex);
    }

    // 传送到指定索引的房间
    void TeleportToDestination(int index)
    {
        if (index < 0 || index >= destinations.Length)
        {
            Debug.LogWarning($"传送索引 {index} 无效！");
            return;
        }

        RoomDestination dest = destinations[index];

        Debug.Log($"传送至: {dest.roomName}");

        // 1. 更新摄像机边界（最关键的一步！）
        if (roomCamera != null)
        {
            roomCamera.currentRoomBounds = dest.roomBounds;
            Debug.Log($"摄像机边界已更新为: {dest.roomBounds}");
        }

        // 2. 传送玩家到目标位置
        transform.position = dest.playerSpawnPosition;

        // 3. （可选）如果需要，可以在这里立即将摄像机“快进”到新位置
        // 取消下面这行代码的注释，可以让摄像机立刻跳转，而不是平滑移动过去
        // if (roomCamera != null) Camera.main.transform.position = new Vector3(dest.playerSpawnPosition.x, dest.playerSpawnPosition.y, -10);
    }

    // 在Scene视图中可视化传送点（仅编辑时可见）
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || destinations == null) return;

        for (int i = 0; i < destinations.Length; i++)
        {
            RoomDestination dest = destinations[i];

            // 用不同颜色区分当前选中的点
            Gizmos.color = (i == currentDestinationIndex) ? Color.green : Color.yellow;

            // 绘制传送点位置（玩家出生点）
            Gizmos.DrawWireCube(dest.playerSpawnPosition, Vector3.one * 0.5f);
            Gizmos.DrawIcon(dest.playerSpawnPosition, "TeleportIcon.png", true);

            // 绘制房间边界
            Gizmos.color = new Color(0, 1, 0, 0.3f); // 半透明绿色
            Vector3 center = new Vector3(dest.roomBounds.center.x, dest.roomBounds.center.y, 0);
            Vector3 size = new Vector3(dest.roomBounds.width, dest.roomBounds.height, 0.1f);
            Gizmos.DrawWireCube(center, size);

            // 标注房间名和索引
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(dest.playerSpawnPosition + Vector2.up * 0.6f, 
                $"[{i}] {dest.roomName}\n({dest.playerSpawnPosition.x:F1}, {dest.playerSpawnPosition.y:F1})");
            #endif
        }
    }
}

// 房间目标数据类（一个房间对应一个传送目标）
[System.Serializable]
public class RoomDestination
{
    public string roomName = "新房间";
    public Rect roomBounds = new Rect(0, 0, 16, 9); // 房间边界
    public Vector2 playerSpawnPosition = Vector2.zero; // 玩家在该房间的出生/传送点
}