using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrossSceneDoor : Interactable // 继承你的交互基类
{
    [Header("=== 跨场景传送设置 ===")]
    public string targetSceneName;            // 目标场景名称（必须和Build Settings里完全一致！）
    public Vector3 targetSpawnPosition;      // 玩家在目标场景的出生坐标
    public Rect targetRoomBounds;            // 目标场景的摄像机边界（非常重要！）

    [Header("=== 门锁设置 ===")]
    public bool hasLock = true;              // 这个门是否有锁（有锁的一侧设为true，另一侧设为false）
    public int requiredKeyID = 1001;         // 需要的钥匙ID
    public Sprite lockedHintSprite;          // 锁住时显示的图标

    [Header("=== 本地组件引用 ===")]
    public RoomTeleporter playerTeleporter;  // 拖拽玩家身上的RoomTeleporter脚本到这里

    // 内部状态
    private string thisDoorID;               // 这个门的唯一标识
    private bool isTransitioning = false;    // 防止重复触发

    void Start()
    {
        // 1. 生成唯一ID
        thisDoorID = DoorLockManager.GetDoorID(this.gameObject);

        // 2. 如果门有锁，检查是否已被永久解锁
        if (hasLock)
        {
            if (DoorLockManager.IsDoorUnlocked(thisDoorID))
            {
                Debug.Log($"<color=green>检测到已解锁的门：{thisDoorID}</color>");
                hasLock = false; // 本地标记为无锁，避免再次检查钥匙
            }
        }

        // 3. 自动查找玩家身上的传送器（备用方案）
        if (playerTeleporter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTeleporter = player.GetComponent<RoomTeleporter>();
        }
    }

    // 核心交互逻辑
    protected override void OnInteract()
    {
        if (isTransitioning) return;

        // 1. 检查门锁
        if (hasLock)
        {
            if (!CheckKey())
            {
                StartCoroutine(ShowLockedHint());
                return;
            }
            // 有钥匙，永久解锁
            DoorLockManager.UnlockDoor(thisDoorID);
            hasLock = false; // 本地也标记为无锁
        }

        // 2. 执行传送
        StartCoroutine(InitiateSceneTransition());
    }

    // 检查背包钥匙
    bool CheckKey()
    {
        if (InventoryManager.Instance != null)
        {
            bool hasKey = InventoryManager.Instance.HasItem(requiredKeyID);
            if (hasKey)
            {
                Debug.Log($"<color=green>钥匙检查通过！消耗钥匙ID：{requiredKeyID}</color>");
                InventoryManager.Instance.RemoveItem(requiredKeyID);
                return true;
            }
        }
        Debug.Log($"<color=orange>需要钥匙ID：{requiredKeyID}</color>");
        return false;
    }

    // 显示锁住提示（复用你原有逻辑）
    IEnumerator ShowLockedHint()
    {
        if (InteractionHintManager.Instance != null && lockedHintSprite != null)
        {
            Sprite originalSprite = InteractionHintManager.Instance.exclamationSprite;
            InteractionHintManager.Instance.ShowHint(lockedHintSprite);
            yield return new WaitForSeconds(1.5f);
            if (playerInRange) InteractionHintManager.Instance.ShowHint(originalSprite);
        }
    }

    // 核心传送协程
    IEnumerator InitiateSceneTransition()
    {
        isTransitioning = true;
        Debug.Log($"<color=cyan>[传送开始] 前往场景：{targetSceneName}</color>");

        // 1. 隐藏交互提示
        if (InteractionHintManager.Instance != null)
            InteractionHintManager.Instance.HideHint();

        // 2. 情况A：如果找到了玩家的RoomTeleporter，使用它的高级黑屏过渡
        if (playerTeleporter != null)
        {
            yield return StartCoroutine(UsePlayerTeleporter());
        }
        // 情况B：没找到，使用一个简化的黑屏过渡（保底方案）
        else
        {
            yield return StartCoroutine(SimpleFadeTransition());
        }

        isTransitioning = false;
    }

    // 方案A：复用玩家身上现成的RoomTeleporter组件（推荐）
    IEnumerator UsePlayerTeleporter()
    {
        // 关键：临时修改玩家RoomTeleporter的目标，骗它执行一次“同场景传送”
        // 但我们会在黑屏后拦截，改为加载新场景

        // 1. 先让RoomTeleporter开始淡出黑屏
        if (playerTeleporter.fadeOverlay != null)
        {
            yield return playerTeleporter.StartCoroutine(FadeScreen(playerTeleporter.fadeOverlay, 0f, 1f, playerTeleporter.fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 2. 黑屏后，加载新场景
        SceneManager.LoadScene(targetSceneName);
        yield return null; // 等待一帧，确保新场景加载

        // 3. 在新场景中设置玩家位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = targetSpawnPosition;
        }

        // 4. 设置新场景的摄像机边界
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            RoomCamera roomCam = mainCam.GetComponent<RoomCamera>();
            if (roomCam != null)
            {
                roomCam.currentRoomBounds = targetRoomBounds;
                // 强制摄像机立即更新位置
                Vector3 camPos = new Vector3(
                    Mathf.Clamp(targetSpawnPosition.x, targetRoomBounds.xMin, targetRoomBounds.xMax),
                    Mathf.Clamp(targetSpawnPosition.y, targetRoomBounds.yMin, targetRoomBounds.yMax),
                    mainCam.transform.position.z
                );
                mainCam.transform.position = camPos;
            }
        }

        // 5. 淡入（使用新场景中玩家身上的RoomTeleporter）
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RoomTeleporter newTeleporter = player.GetComponent<RoomTeleporter>();
            if (newTeleporter != null && newTeleporter.fadeOverlay != null)
            {
                yield return newTeleporter.StartCoroutine(FadeScreen(newTeleporter.fadeOverlay, 1f, 0f, newTeleporter.fadeDuration));
            }
        }
    }

    // 方案B：简化的黑屏过渡（保底方案）
    IEnumerator SimpleFadeTransition()
    {
        // 创建临时黑屏Canvas
        GameObject fadeCanvas = new GameObject("TempFadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        GameObject fadeImage = new GameObject("FadeOverlay");
        fadeImage.transform.SetParent(fadeCanvas.transform);
        UnityEngine.UI.Image img = fadeImage.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0);

        // 全屏
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 淡出
        yield return StartCoroutine(FadeScreen(img, 0f, 1f, 0.5f));

        // 加载新场景
        SceneManager.LoadScene(targetSceneName);
        yield return null;

        // 设置玩家位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = targetSpawnPosition;

        // 淡入（在新场景中也需要一个淡入效果，这里需要额外处理，为了简单先直接恢复）
        img.color = new Color(0, 0, 0, 1);
        yield return StartCoroutine(FadeScreen(img, 1f, 0f, 0.5f));

        // 清理
        Destroy(fadeCanvas);
    }

    // 通用的淡入淡出协程（复用RoomTeleporter的逻辑）
    IEnumerator FadeScreen(UnityEngine.UI.Image overlay, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = overlay.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / duration));
            overlay.color = color;
            yield return null;
        }

        color.a = endAlpha;
        overlay.color = color;
    }

    void OnDrawGizmosSelected()
    {
        // 绘制传送目标位置（在场景视图中可视化）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetSpawnPosition, 0.5f);
        Gizmos.DrawLine(transform.position, targetSpawnPosition);

        // 绘制摄像机边界
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(targetRoomBounds.center.x, targetRoomBounds.center.y, 0),
                           new Vector3(targetRoomBounds.width, targetRoomBounds.height, 0.1f));
    }
}