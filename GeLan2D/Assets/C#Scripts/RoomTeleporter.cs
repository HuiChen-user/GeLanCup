using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomTeleporter : MonoBehaviour
{
    [Header("房间传送点列表")]
    public RoomDestination[] destinations;

    [Header("黑屏过渡设置")]
    public float fadeDuration = 0.5f;
    public Image fadeOverlay;

    [Header("调试")]
    public bool showDebugGizmos = true;

    private int currentDestinationIndex = 0;
    private RoomCamera roomCamera;
    private Move playerMovement;
    private Rigidbody2D playerRigidbody; //缓存玩家刚体引用
    private bool isTransitioning = false;

    void Start()
{
    roomCamera = Camera.main.GetComponent<RoomCamera>();
    playerMovement = GetComponent<Move>();
    playerRigidbody = GetComponent<Rigidbody2D>();
    
    if (fadeOverlay == null)
    {
        GameObject fadeObj = GameObject.Find("FadeOverlay");
        if (fadeObj != null) fadeOverlay = fadeObj.GetComponent<Image>();
    }
    
    if (fadeOverlay != null)
    {
        Color color = fadeOverlay.color;
        color.a = 0f;
        fadeOverlay.color = color;
        fadeOverlay.raycastTarget = false;
    }

    //同步初始化位置，移除协程延迟
    if (destinations.Length > 0)
    {
        InitializePlayerAtStart();
    }
}

//同步初始化方法
private void InitializePlayerAtStart()
{
    // 1. 获取第一个房间的配置
    RoomDestination firstRoom = destinations[0];
    
    // 2. 立即设置玩家位置到出生点
    transform.position = firstRoom.playerSpawnPosition;
    Debug.Log($"玩家初始位置已同步设置到房间：{firstRoom.roomName} 的出生点 {firstRoom.playerSpawnPosition}");
    
    // 3. 立即设置摄像机边界
    if (roomCamera != null)
    {
        roomCamera.currentRoomBounds = firstRoom.roomBounds;
        
        // 4. 立即强制计算并设置摄像机位置
        if (Camera.main != null)
        {
            Vector3 immediateCameraPos = CalculateCameraPosition(firstRoom.playerSpawnPosition, firstRoom.roomBounds);
            Camera.main.transform.position = immediateCameraPos;
            Debug.Log($"摄像机初始位置已同步设置到：{immediateCameraPos}");
        }
    }
    
    //确保刚体状态正确
    if (playerRigidbody != null)
    {
        playerRigidbody.velocity = Vector2.zero;
        // 根据你的移动方式决定是否设为Kinematic
        // playerRigidbody.bodyType = RigidbodyType2D.Kinematic;
    }
}

    void Update()
    {
        // Update方法已清空，确保没有全局E键检测
        // 传送仅由 DoorTrigger 调用
    }

//核心公开方法：供 DoorTrigger 调用
public void TeleportToSpecificDoor(int fromRoomIndex, int fromDoorIndex)
{
    if (isTransitioning) return;
    if (destinations.Length == 0) return;

    // 1. 安全检查
    if (fromRoomIndex < 0 || fromRoomIndex >= destinations.Length)
    {
        Debug.LogError($"传送错误：房间索引 {fromRoomIndex} 不存在！");
        return;
    }
    RoomDestination currentRoom = destinations[fromRoomIndex];
    if (fromDoorIndex < 0 || fromDoorIndex >= currentRoom.doorConnections.Count)
    {
        Debug.LogError($"传送错误：房间 '{currentRoom.roomName}' 不存在索引为 {fromDoorIndex} 的门连接！");
        return;
    }

    // 2. 获取连接信息
    DoorConnection usedDoor = currentRoom.doorConnections[fromDoorIndex];
    int targetRoomIndex = usedDoor.targetRoomIndex;
    Vector2 targetDoorPos = usedDoor.targetDoorPosition;

    if (targetRoomIndex < 0 || targetRoomIndex >= destinations.Length)
    {
        Debug.LogError($"传送错误：目标房间索引 {targetRoomIndex} 不存在！");
        return;
    }
    RoomDestination targetRoom = destinations[targetRoomIndex];

    Debug.Log($"从 '{currentRoom.roomName}' 传送到 '{targetRoom.roomName}' 的门口");

    // 3. 启动唯一的核心传送协程
    StartCoroutine(CoreTeleportRoutine(targetRoomIndex, targetDoorPos, targetRoom.roomBounds));
}

//唯一的、统一的核心传送协程
private IEnumerator CoreTeleportRoutine(int targetRoomIndex, Vector2 targetPosition, Rect targetRoomBounds)
{
    //防止重复触发
    if (isTransitioning) yield break;
    isTransitioning = true;
    Debug.Log("传送启动：开始冻结与过渡");

    //第1步：立即彻底冻结物理运动 (最关键！)
    if (playerRigidbody != null)
    {
        playerRigidbody.velocity = Vector2.zero; // 速度归零
        playerRigidbody.angularVelocity = 0f;
        playerRigidbody.bodyType = RigidbodyType2D.Kinematic; // 设为运动学，无视物理
    }
    if (playerMovement != null)
    {
        playerMovement.enabled = false; // 禁用移动脚本
    }

    //第2步：屏幕淡出至全黑 ===
    if (fadeOverlay != null)
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));
        Debug.Log("屏幕已全黑，执行瞬间传送");
    }
    else
    {
        yield return new WaitForSeconds(fadeDuration);
    }

    //第3步：黑屏瞬间完成传送与摄像机跳转
    // 3.1 更新摄像机边界
    if (roomCamera != null)
    {
        roomCamera.currentRoomBounds = targetRoomBounds;
    }
    // 3.2 传送玩家
    transform.position = targetPosition;
    // 3.3 强制摄像机跳转
    if (roomCamera != null && Camera.main != null)
    {
        Vector3 desiredCamPos = CalculateCameraPosition(targetPosition, targetRoomBounds);
        Camera.main.transform.position = desiredCamPos;
    }
    yield return null; // 确保一帧完成

    //第4步：屏幕从全黑开始淡入
    if (fadeOverlay != null)
    {
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
    }
    else
    {
        yield return new WaitForSeconds(fadeDuration);
    }

    //第5步：恢复控制与物理
    if (playerRigidbody != null)
    {
        // 根据你的游戏需要，决定是改回Dynamic还是保持Kinematic
        // 如果你的移动脚本是通过Transform直接移动，保持Kinematic即可
        // playerRigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
    if (playerMovement != null)
    {
        playerMovement.enabled = true;
    }

    isTransitioning = false;
    Debug.Log($"传送完成！角色已在：{destinations[targetRoomIndex].roomName}");
}

//辅助方法
    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;
        float elapsedTime = 0f;
        Color color = fadeOverlay.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / duration));
            fadeOverlay.color = color;
            yield return null;
        }
        color.a = endAlpha;
        fadeOverlay.color = color;
    }

    Vector3 CalculateCameraPosition(Vector2 playerPos, Rect roomBounds)
    {
        if (roomCamera == null || Camera.main == null)
        {
            return new Vector3(playerPos.x, playerPos.y, Camera.main.transform.position.z);
        }

        Camera cam = Camera.main;
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = roomBounds.xMin + cameraWidth;
        float maxX = roomBounds.xMax - cameraWidth;
        float minY = roomBounds.yMin + cameraHeight;
        float maxY = roomBounds.yMax - cameraHeight;

        if (maxX < minX) minX = maxX = (roomBounds.xMin + roomBounds.xMax) / 2f;
        if (maxY < minY) minY = maxY = (roomBounds.yMin + roomBounds.yMax) / 2f;

        float desiredX = Mathf.Clamp(playerPos.x, minX, maxX);
        float desiredY = Mathf.Clamp(playerPos.y, minY, maxY);

        return new Vector3(desiredX, desiredY, cam.transform.position.z);
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || destinations == null) return;
        for (int i = 0; i < destinations.Length; i++)
        {
            RoomDestination dest = destinations[i];
            Gizmos.color = (i == currentDestinationIndex) ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(dest.playerSpawnPosition, Vector3.one * 0.5f);
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 center = new Vector3(dest.roomBounds.center.x, dest.roomBounds.center.y, 0);
            Vector3 size = new Vector3(dest.roomBounds.width, dest.roomBounds.height, 0.1f);
            Gizmos.DrawWireCube(center, size);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(dest.playerSpawnPosition + Vector2.up * 0.6f, $"[{i}] {dest.roomName}");
#endif
        }
    }
}

//数据类定义
[System.Serializable]
public class RoomDestination
{
    public string roomName = "新房间";
    public Rect roomBounds = new Rect(0, 0, 16, 9);
    public Vector2 playerSpawnPosition = Vector2.zero;
    public List<DoorConnection> doorConnections = new List<DoorConnection>();
}

[System.Serializable]
public class DoorConnection
{
    public string connectionName = "房门";
    public Vector2 doorPosition;
    public int targetRoomIndex;
    public Vector2 targetDoorPosition;
}