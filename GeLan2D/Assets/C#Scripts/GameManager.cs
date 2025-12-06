using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("预制体引用")]
    public GameObject playerPrefab;    // 拖入Player预制体
    public GameObject canvasPrefab;    // 拖入Canvas预制体
    public GameObject cameraPrefab;    // 拖入Main Camera预制体
    
    [Header("场景数据")]
    public string currentSceneName;
    public Vector3 lastPlayerPosition;
    public Rect lastRoomBounds;
    
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
            return;
        }
    }
    
    void InitializeGame()
    {
        // 确保场景中有玩家（如果还没有）
        if (GameObject.FindGameObjectWithTag("Player") == null && playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(player);  // 玩家也持久化
        }
        
        // 确保有UI
        if (GameObject.Find("Canvas") == null && canvasPrefab != null)
        {
            GameObject canvas = Instantiate(canvasPrefab);
            DontDestroyOnLoad(canvas);
        }
        
        // 确保有摄像机
        if (Camera.main == null && cameraPrefab != null)
        {
            GameObject camera = Instantiate(cameraPrefab);
            DontDestroyOnLoad(camera);
        }
        
        // 记录当前场景
        currentSceneName = SceneManager.GetActiveScene().name;
    }
    
    void Start()
    {
        // 如果这是第一个场景，加载初始房间
        if (SceneManager.sceneCount == 1)
        {
            // 加载第一个房间（例如：大厅或关卡1）
            SceneManager.LoadScene("Room1", LoadSceneMode.Additive);
        }
    }
    
    // 新增：保存当前场景数据的方法
    public void SaveCurrentSceneData()
    {
        // 保存当前场景名称
        currentSceneName = SceneManager.GetActiveScene().name;
        
        // 保存玩家位置
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }
        
        // 保存摄像机边界
        if (Camera.main != null)
        {
            RoomCamera roomCamera = Camera.main.GetComponent<RoomCamera>();
            if (roomCamera != null)
            {
                lastRoomBounds = roomCamera.currentRoomBounds;
            }
        }
        
        Debug.Log($"保存场景数据：{currentSceneName}，玩家位置：{lastPlayerPosition}");
    }
    
    // 新增：恢复玩家位置（可选，用于返回上个场景）
    public void RestorePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastPlayerPosition;
        }
    }
}