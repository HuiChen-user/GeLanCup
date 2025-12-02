using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Interactable // 继承基类
{
    [Header("门连接设置")]
    public int linkedRoomIndex;
    public int doorConnectionIndex;

    // 实现基类的抽象方法：当按下E时执行门的传送
    protected override void OnInteract()
    {
        // 调用传送
        RoomTeleporter teleporter = FindObjectOfType<RoomTeleporter>();
        if (teleporter != null)
        {
            teleporter.TeleportToSpecificDoor(linkedRoomIndex, doorConnectionIndex);
        }

        // 交互后，可以立即隐藏提示（可选，因为传送会改变位置）
        // 基类的 OnPlayerExit 会在传送后因位置变化被调用，所以通常自动处理。
    }

    // （可选）你可以重写 OnPlayerEnter 来为门使用特定的提示图标
    // protected override void OnPlayerEnter()
    // {
    //     base.OnPlayerEnter(); // 调用基类方法显示默认提示
    //     // 或使用自定义逻辑
    // }
}