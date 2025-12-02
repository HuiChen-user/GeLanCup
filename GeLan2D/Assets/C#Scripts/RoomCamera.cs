using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCamera : MonoBehaviour
{
    [Header("基本设置")]
    public Transform target;  // 玩家角色
    public float followSpeed = 5f;  // 跟随速度，越大越紧跟
    
    [Header("房间边界")]
    public Rect currentRoomBounds = new Rect(0, 0, 16, 9);
    
    [Header("调试选项")]
    public bool showBoundsInEditor = true;  // 在编辑器中显示边界
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        if (target == null)
        {
            // 尝试自动查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("自动找到了玩家：" + player.name);
            }
            else
            {
                Debug.LogWarning("请将玩家角色拖拽到Target字段！");
            }
        }
        
        // 初始位置（房间中心）
        transform.position = new Vector3(
            currentRoomBounds.center.x,
            currentRoomBounds.center.y,
            -10
        );
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // 计算目标位置（跟随玩家，但保持Z轴为-10）
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y, -10);
        
        // 边界限制
        float vertExtent = cam.orthographicSize;  // 摄像机垂直视野的一半
        float horzExtent = vertExtent * cam.aspect;  // 摄像机水平视野的一半
        
        // 计算摄像机不能超出的边界
        float minX = currentRoomBounds.xMin + horzExtent;
        float maxX = currentRoomBounds.xMax - horzExtent;
        float minY = currentRoomBounds.yMin + vertExtent;
        float maxY = currentRoomBounds.yMax - vertExtent;
        
        // 如果房间太小（小于摄像机视野），则居中显示
        if (maxX < minX)
        {
            minX = maxX = (currentRoomBounds.xMin + currentRoomBounds.xMax) / 2f;
        }
        if (maxY < minY)
        {
            minY = maxY = (currentRoomBounds.yMin + currentRoomBounds.yMax) / 2f;
        }
        
        // 限制摄像机位置
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);
        
        // 平滑跟随（线性插值）
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }
    
    // 切换房间的方法（为未来预留）
    public void ChangeRoom(Rect newBounds)
    {
        currentRoomBounds = newBounds;
        Debug.Log($"切换到新房间：{newBounds}");
    }
    
    // 在编辑器中显示房间边界
    void OnDrawGizmos()
    {
        if (!showBoundsInEditor) return;
        
        Gizmos.color = Color.green;
        
        // 绘制房间边界
        Vector3 center = new Vector3(
            currentRoomBounds.center.x,
            currentRoomBounds.center.y,
            0
        );
        
        Vector3 size = new Vector3(
            currentRoomBounds.width,
            currentRoomBounds.height,
            0.1f
        );
        
        Gizmos.DrawWireCube(center, size);
        
        // 标注边界坐标
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            new Vector3(currentRoomBounds.xMin, currentRoomBounds.yMax, 0),
            $"({currentRoomBounds.xMin:F1}, {currentRoomBounds.yMax:F1})"
        );
        UnityEditor.Handles.Label(
            new Vector3(currentRoomBounds.xMax, currentRoomBounds.yMin, 0),
            $"({currentRoomBounds.xMax:F1}, {currentRoomBounds.yMin:F1})"
        );
        UnityEditor.Handles.Label(
            center,
            $"房间\n{currentRoomBounds.width:F1}x{currentRoomBounds.height:F1}"
        );
        #endif
    }
}