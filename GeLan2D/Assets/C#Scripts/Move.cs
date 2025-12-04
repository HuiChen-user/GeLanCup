using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    // 1. 新增：声明动画控制器变量
    private Animator animator;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 2. 新增：获取角色身上的Animator组件
        animator = GetComponent<Animator>();
        
        // 确保刚体设置正确
        if (rb != null)
        {
            rb.gravityScale = 0;  // 无重力
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation; // 锁定Y轴移动和旋转
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }
    }
    
    void Update()
    {   if(!enabled)return;//如果脚本被禁用不执行：
        {
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
    }
    
    void FixedUpdate()
    {
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
}