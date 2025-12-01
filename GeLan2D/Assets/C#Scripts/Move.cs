using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        float moveX = 0;
        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1;
            transform.localScale = new Vector3(-1, 1, 1);// 向左翻转
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1;
            transform.localScale = new Vector3(1, 1, 1);// 向右翻转
        }
        
        rb.velocity = new Vector2(moveX * speed, rb.velocity.y);
    }
}