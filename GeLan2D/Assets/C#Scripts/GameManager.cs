using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("玩家预制体")]
    public GameObject playerPrefab;
    public GameObject canvasPrefab;
    public GameObject cameraPrefab;
    
    [Header("场景数据")]
    public string currentSceneName;
    public Vector3 lastPlayerPosition;
    public Rect lastRoomBounds;
    
    // 保存的玩家数据
    private List<ItemData> savedInventory = new List<ItemData>();
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeGame()
    {
        // 确保场景中有玩家（如果还没有）
        if (GameObject.FindGameObjectWithTag("Player") == null && playerPrefab != null)
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
        
        // 确保有UI
        if (GameObject.Find("Canvas") == null && canvasPrefab != null)
        {
            Instantiate(canvasPrefab);
        }
        
        // 记录当前场景
        currentSceneName = SceneManager.GetActiveScene().name;
    }
    
    // 保存当前场景数据
    public void SaveCurrentSceneData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }
        
        // 保存背包数据
        if (InventoryManager.Instance != null)
        {
            savedInventory.Clear();
            // 这里需要根据你的InventoryManager实际结构来保存
            // 例如：savedInventory = InventoryManager.Instance.GetAllItems();
        }
        
        // 保存摄像机边界
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            RoomCamera roomCamera = mainCamera.GetComponent<RoomCamera>();
            if (roomCamera != null)
            {
                lastRoomBounds = roomCamera.currentRoomBounds;
            }
        }
    }
    
    // 恢复玩家数据
    public void RestorePlayerData(Vector3 spawnPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPosition;
        }
        
        // 恢复背包数据
        if (InventoryManager.Instance != null && savedInventory.Count > 0)
        {
            // 恢复背包物品
            // 例如：foreach (ItemData item in savedInventory) {...}
        }
    }
    
    // 返回上一个场景（如果需要）
    public void ReturnToPreviousScene()
    {
        // 这里可以实现返回功能
    }
}