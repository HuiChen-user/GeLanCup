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
    private RoomCamera roomCamera; // 请确保你的摄像机脚本名为 RoomCamera
    private Move playerMovement;
    private bool isTransitioning = false;

    void Start()
    {
        roomCamera = Camera.main.GetComponent<RoomCamera>();
        playerMovement = GetComponent<Move>();
        
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

        if (destinations.Length > 0)
        {
            StartCoroutine(InitialTeleport());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            TeleportToNextRoomWithFade();
        }
    }

    IEnumerator InitialTeleport()
    {
        yield return new WaitForEndOfFrame();
        TeleportToDestination(0, false);
        Debug.Log("初始位置已设置到房间：" + destinations[0].roomName);
    }

    void TeleportToNextRoomWithFade()
    {
        if (destinations.Length == 0) return;
        currentDestinationIndex = (currentDestinationIndex + 1) % destinations.Length;
        StartCoroutine(TransitionRoutine(currentDestinationIndex));
    }

    // ===== 核心协程：严格控制执行顺序 =====
    IEnumerator TransitionRoutine(int destinationIndex)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // 1. 立即禁用玩家移动
        if (playerMovement != null) playerMovement.enabled = false;

        // 2. 屏幕淡出至全黑
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));
            Debug.Log("屏幕已全黑，开始执行传送与摄像机定位");
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3. 执行传送（这包括传送玩家和强制设置摄像机位置）
        TeleportToDestination(destinationIndex, false);

        // 4. 可选但推荐：确保摄像机在新位置稳定（非常重要！）
        // 这一帧让Unity完成所有变换和物理更新，确保摄像机位置被正确应用。
        yield return null; // 等待一帧

        // 5. 屏幕从全黑开始淡入（此时摄像机已绝对就位）
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        // 6. 完全亮起后恢复玩家移动
        if (playerMovement != null) playerMovement.enabled = true;

        isTransitioning = false;
        Debug.Log("传送完成！角色与摄像机均已就位。");
    }

    // ===== 修改关键：传送逻辑，包含强制摄像机定位 =====
    void TeleportToDestination(int index, bool withEffects = true)
    {
        if (index < 0 || index >= destinations.Length)
        {
            Debug.LogWarning($"传送索引 {index} 无效！");
            return;
        }

        RoomDestination dest = destinations[index];
        Debug.Log($"正在传送至: {dest.roomName}");

        // 1. 更新摄像机脚本的房间边界（必须先做！）
        if (roomCamera != null)
        {
            roomCamera.currentRoomBounds = dest.roomBounds;
            Debug.Log($"摄像机边界已更新为: {dest.roomBounds}");
        }

        // 2. 传送玩家到目标位置
        transform.position = dest.playerSpawnPosition;
        Debug.Log($"玩家已传送到: {dest.playerSpawnPosition}");

        // 3. === 核心修复：强制计算并设置摄像机位置 ===
        if (roomCamera != null && Camera.main != null)
        {
            // 3.1 根据玩家新位置和房间新边界，立即计算出摄像机应有的位置
            Vector3 desiredCamPos = CalculateCameraPosition(dest.playerSpawnPosition, dest.roomBounds);
            
            // 3.2 直接将主摄像机的位置设置过去，绕开平滑跟随
            Camera.main.transform.position = desiredCamPos;
            
            // 3.3 （可选但推荐）同时更新RoomCamera脚本内部的缓存位置，防止它下一帧往回跳
            // 如果你的RoomCamera脚本用某个变量（如targetPosition）记录目标，也需要更新它。
            // 例如，如果它有public Vector3 targetPosition，可以在这里设置：
            // roomCamera.targetPosition = desiredCamPos;
            
            Debug.Log($"摄像机已强制设置到: {desiredCamPos}");
        }
    }

    // ===== 新增：计算摄像机在房间边界内的合法位置 =====
    // 这个逻辑应该与你RoomCamera脚本中的限制逻辑完全一致
    Vector3 CalculateCameraPosition(Vector2 playerPos, Rect roomBounds)
    {
        if (roomCamera == null || Camera.main == null)
        {
            return new Vector3(playerPos.x, playerPos.y, Camera.main.transform.position.z);
        }

        Camera cam = Camera.main;
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // 计算摄像机不能超出的边界（与RoomCamera脚本中的逻辑匹配）
        float minX = roomBounds.xMin + cameraWidth;
        float maxX = roomBounds.xMax - cameraWidth;
        float minY = roomBounds.yMin + cameraHeight;
        float maxY = roomBounds.yMax - cameraHeight;

        // 如果房间太小，摄像机居中
        if (maxX < minX) minX = maxX = (roomBounds.xMin + roomBounds.xMax) / 2f;
        if (maxY < minY) minY = maxY = (roomBounds.yMin + roomBounds.yMax) / 2f;

        // 将玩家位置作为期望的摄像机中心，然后进行钳制
        float desiredX = Mathf.Clamp(playerPos.x, minX, maxX);
        float desiredY = Mathf.Clamp(playerPos.y, minY, maxY);

        // 保持摄像机原有的Z轴位置（通常是-10）
        return new Vector3(desiredX, desiredY, cam.transform.position.z);
    }

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

    void OnDrawGizmos()
    {
        // ... (你原来的Gizmos绘制代码，保持不变)
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
[System.Serializable]
public class RoomDestination
{
    public string roomName="新房间";
    public Rect roomBounds=new Rect(0,0,16,9);
    public Vector2 playerSpawnPosition=Vector2.zero;
}