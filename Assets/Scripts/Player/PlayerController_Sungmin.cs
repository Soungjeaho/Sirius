using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Sungmin : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private int maxJumpCount = 2; // ✅ 점프 가능 횟수 (2면 더블 점프)

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("References")]
    [SerializeField] private NewReelback reelback;

    private Rigidbody2D rb;
    private float xAxis;
    private int jumpCount; // ✅ 현재 점프 횟수

    public static PlayerController_Sungmin Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GetInputs();

        if (!reelback.IsGrappling)
        {
            Move();
        }

        Jump();

        // ✅ 땅에 닿으면 점프 횟수 초기화
        if (Grounded())
        {
            jumpCount = 0;
        }
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
            // ✅ 점프 가능 조건: (땅 위) 또는 (점프 횟수가 최대 미만)
            if (Grounded() || jumpCount < maxJumpCount)
            {
                // 그래플 중이라면 해제
                if (reelback.IsGrappling)
                    reelback.StopGrapple();

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++; // ✅ 점프 횟수 증가
            }
        }

        // 점프키를 떼면 상승 중일 때 점프 끊기
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    public bool Grounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
