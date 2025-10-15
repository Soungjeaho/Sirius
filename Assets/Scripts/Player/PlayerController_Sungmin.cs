using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Sungmin : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float jumpForce = 20;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("References")]
    [SerializeField] private Reelback reelback; // 새로 분리된 릴백 시스템 참조

    private Rigidbody2D rb;
    private float xAxis;

    public static PlayerController_Sungmin Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GetInputs();

        if (!reelback.IsGrappling && !reelback.IsPullingEnemy)
            Move();

        Jump();
        reelback.HandleReekback(); 
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (Grounded() || reelback.IsGrappling)
            {
                if (reelback.IsGrappling)
                    reelback.StopReelback();

                if (reelback.IsPullingEnemy)
                    reelback.StopPullEnemy();

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    public bool Grounded()
    {
        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround);
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }
}
