using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("门连接设置")]
    public int linkedRoomIndex; // 这个门属于哪个房间（对应RoomTeleporter里destinations的索引）
    public int doorConnectionIndex; // 这个门使用的是该房间doorConnections列表中的第几个连接点

    private bool playerInRange = false;
    private RoomTeleporter roomTeleporter;

    void Start()
    {
        roomTeleporter = FindObjectOfType<RoomTeleporter>();
        if (roomTeleporter == null)
        {
            Debug.LogError("DoorTrigger: 未找到RoomTeleporter脚本！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("靠近门，可按E传送");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("离开门范围");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && roomTeleporter != null)
        {
            // 调用RoomTeleporter里新增的方法，传入这个门的“地址”
            roomTeleporter.TeleportToSpecificDoor(linkedRoomIndex, doorConnectionIndex);
        }
    }

    // 在Scene视图里可视化门的位置和连接关系（调试用）
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.7f);
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"Room:{linkedRoomIndex}\nDoor:{doorConnectionIndex}");
        #endif
    }
}