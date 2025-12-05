using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    
    // 1. 新增：声明动画控制器变量
    private Animator animator;
    
    // 2. 新增：移动控制变量
    public bool canMove = true;  // 添加这个变量来控制是否可以移动
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 2. 新增：获取角色身上的Animator组件
        animator = GetComponent<Animator>();
        
        // 确保刚体设置正确
        if (rb != null)
        {
            rb.gravityScale = 0; // 无重力
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 只锁定旋转
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }
    
    void Update()
    {
        if(!enabled || !canMove) return;  // 3. 修改：增加canMove检查
        
        // 方向翻转
        if (Input.GetKey(KeyCode.A))
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
    void FixedUpdate()
    {
        if (!canMove)  // 4. 新增：如果不能移动，速度归零
        {
            rb.velocity = Vector2.zero;
            if (animator != null)
                animator.SetFloat("Speed", 0);
            return;
        }
        
        float moveX = 0;
        
        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1;
        }
        
        // 只设置X轴速度，Y轴保持为0
        rb.velocity = new Vector2(moveX * speed, 0);
        
        // 3. 新增：最重要的一行！将移动速度的绝对值赋值给动画参数"Speed"
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveX));
        }
    }
    
    // 5. 新增：冻结/解冻玩家的公共方法
    public void FreezeMovement(bool freeze)
    {
        canMove = !freeze;
        
        if (freeze && rb != null)
        {
            rb.velocity = Vector2.zero;  // 立即停止移动
        }
        
        Debug.Log($"玩家移动{(freeze ? "冻结" : "解冻")}");
    }
}