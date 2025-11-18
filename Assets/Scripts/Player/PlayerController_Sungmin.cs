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
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("References")]
    [SerializeField] private NewReelback reelback;   // 새로 분리된 릴백 시스템 참조

    private Rigidbody2D rb;
    private float xAxis;

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

        // 그래플링 중이 아닐 때만 이동 가능
        if (!reelback.IsGrappling)
        {
            Move();
        }

        Jump();

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
            // 땅에 있거나 그래플 중일 때 점프 가능
            if (Grounded() || reelback.IsGrappling)
            {
                if (reelback.IsGrappling)
                    reelback.StopGrapple(); // 그래플 중 점프 시 해제

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // 점프키 떼면 상승 중일 때 점프 끊기
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    public bool Grounded()
    {
        // 간단한 원 충돌 기반 땅 체크
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 Ground Check 범위 시각화
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
