using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DoorLockManager // 注意：这是静态类，不继承MonoBehaviour
{
    // 静态字典：键是门的唯一标识（建议用：场景名+物体名），值是是否已解锁
    private static Dictionary<string, bool> doorUnlockState = new Dictionary<string, bool>();

    // 生成一个唯一ID（简单方案：场景名_门对象名）
    public static string GetDoorID(GameObject doorObject)
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + doorObject.name;
    }

    // 检查这个门是否已解锁
    public static bool IsDoorUnlocked(string doorID)
    {
        if (doorUnlockState.ContainsKey(doorID))
        {
            return doorUnlockState[doorID]; // 返回保存的状态
        }
        return false; // 如果没记录过，默认是锁着的
    }

    // 标记这个门为已解锁
    public static void UnlockDoor(string doorID)
    {
        doorUnlockState[doorID] = true;
        Debug.Log($"<color=yellow>门状态已保存：{doorID} 永久解锁。</color>");
    }

    // 调试用：打印所有门状态
    public static void DebugLogAllDoors()
    {
        foreach (var kvp in doorUnlockState)
        {
            Debug.Log($"门ID: {kvp.Key}, 状态: {(kvp.Value ? "已解锁" : "锁住")}");
        }
    }
}