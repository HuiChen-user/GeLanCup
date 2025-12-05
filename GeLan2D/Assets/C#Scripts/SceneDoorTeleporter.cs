using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneDoorTeleporter : Interactable
{
    [Header("目标场景设置")]
    public string targetSceneName;                    // 目标场景名称
    public Vector3 targetSpawnPosition = Vector3.zero; // 在目标场景中的出生位置
    
    [Header("传送效果")]
    public float fadeDuration = 0.5f;                 // 淡入淡出时间
    public Image fadeOverlay;                         // 黑屏遮罩（Canvas下的Image）
    
    [Header("摄像机设置")]
    public Rect targetRoomBounds;                     // 目标场景的摄像机边界
    
    [Header("钥匙需求")]
    public bool requiresKey = true;                   // 是否需要钥匙
    public int requiredKeyID = 1001;                  // 需要的钥匙ID
    public Sprite lockedHintSprite;                   // 锁住时的提示图标
    
    [Header("状态")]
    public bool isLocked = true;                      // 当前是否锁住
    public float lockedHintDuration = 1.5f;           // 锁住提示显示时间
    
    private bool isShowingLockedHint = false;
    private bool isTransitioning = false;
    private GameManager gameManager;
    
    void Start()
    {
        // 自动查找GameManager
        gameManager = GameManager.Instance;
        
        // 如果没有手动指定黑屏遮罩，尝试查找
        if (fadeOverlay == null)
        {
            GameObject fadeObj = GameObject.Find("FadeOverlay");
            if (fadeObj != null) fadeOverlay = fadeObj.GetComponent<Image>();
        }
    }
    
    // 重写进入范围方法：总是显示感叹号
    protected override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
    }
    
    // 重写交互方法
    protected override void OnInteract()
    {
        if (isShowingLockedHint || isTransitioning) return;
        
        // 如果门需要钥匙且被锁住
        if (requiresKey && isLocked)
        {
            // 检查背包是否有钥匙
            if (HasKey())
            {
                // 有钥匙，解锁并传送
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
            StartTeleport();
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
        StartTeleport();
        
        Debug.Log("使用钥匙解锁门，钥匙已消耗");
    }
    
    // 开始传送流程
    private void StartTeleport()
    {
        if (isTransitioning) return;
        
        StartCoroutine(TeleportCoroutine());
    }
    
    // 传送协程（包含黑屏过渡）
    private IEnumerator TeleportCoroutine()
    {
        isTransitioning = true;
        
        // 1. 隐藏交互提示
        if (InteractionHintManager.Instance != null)
        {
            InteractionHintManager.Instance.HideHint();
        }
        
        // 2. 冻结玩家控制
        FreezePlayer(true);
        
        // 3. 淡出到黑屏
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }
        
        // 4. 保存当前场景数据（如果需要返回）
        if (gameManager != null)
        {
            gameManager.SaveCurrentSceneData();
        }
        
        // 5. 加载目标场景
        Scene currentScene = SceneManager.GetActiveScene();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 6. 场景加载完成，激活新场景
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(targetScene);
        
        // 7. 移动玩家到目标位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = targetSpawnPosition;
        }
        
        // 8. 设置摄像机边界（如果目标场景有边界）
        if (Camera.main != null)
        {
            RoomCamera roomCamera = Camera.main.GetComponent<RoomCamera>();
            if (roomCamera != null && targetRoomBounds != new Rect(0, 0, 0, 0))
            {
                roomCamera.currentRoomBounds = targetRoomBounds;
                
                // 立即更新摄像机位置
                Vector3 desiredPos = new Vector3(
                    Mathf.Clamp(targetSpawnPosition.x, targetRoomBounds.xMin, targetRoomBounds.xMax),
                    Mathf.Clamp(targetSpawnPosition.y, targetRoomBounds.yMin, targetRoomBounds.yMax),
                    -10
                );
                Camera.main.transform.position = desiredPos;
            }
        }
        
        // 9. 淡入恢复正常
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }
        
        // 10. 恢复玩家控制
        FreezePlayer(false);
        
        // 11. 卸载旧场景（延迟一帧，确保一切正常）
        yield return null;
        if (currentScene.name != "PersistentScene") // 不卸载持久化场景
        {
            SceneManager.UnloadSceneAsync(currentScene);
        }
        
        isTransitioning = false;
        
        Debug.Log($"传送完成！到达场景: {targetSceneName}");
    }
    
    // 冻结/解冻玩家
    private void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // 冻结移动脚本
        Move moveScript = player.GetComponent<Move>();
        if (moveScript != null)
        {
            moveScript.enabled = !freeze;
        }
        
        // 冻结刚体
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (freeze)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
    
    // 黑屏淡入淡出效果
    private IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
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
            base.OnPlayerEnter(); // 显示默认的感叹号提示
        }
        
        isShowingLockedHint = false;
    }
    
    // 重写离开范围方法
    protected override void OnPlayerExit()
    {
        isShowingLockedHint = false;
        base.OnPlayerExit();
    }
    
    // 在编辑器中可视化
    void OnDrawGizmos()
    {
        // 绘制传送点位置
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "SceneLoadTrigger");
        
        // 绘制目标位置（如果是在同一场景中预览）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetSpawnPosition, 0.3f);
        Gizmos.DrawLine(transform.position, targetSpawnPosition);
    }
}