using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Sungmin : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private int maxJumpCount = 2;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float attackMinDistance = 0.5f;
    [SerializeField] private float attackMaxDistance = 2.0f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private int attackDamage = 1;

    [Header("References")]
    [SerializeField] private NewReelback reelback;

    private Rigidbody2D rb;
    private float xAxis;
    private int jumpCount;
    private bool canAttack = true;

    private Vector2 lastAttackDir = Vector2.right; //  마지막 공격 방향 저장

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
        Attack();

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
            if (Grounded() || jumpCount < maxJumpCount)
            {
                if (reelback.IsGrappling)
                    reelback.StopGrapple();

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpCount++;
            }
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        // 마우스 방향 계산
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        lastAttackDir = dir; //  기즈모용 방향 저장
        Vector2 attackCenter = (Vector2)transform.position + dir * attackMaxDistance;

        //  충돌체 탐색 (모든 오브젝트 대상으로)
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy")) //  태그로 판정
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);

                if (dist >= attackMinDistance && dist <= attackMaxDistance)
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage);
                    }
                }
            }
        }

        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
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

            //공격 범위 기즈모
        if (Camera.main != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = Application.isPlaying ? lastAttackDir : (mousePos - (Vector2)transform.position).normalized;
            Vector2 attackCenter = (Vector2)transform.position + dir * attackMaxDistance;

            // 최소/최대 사거리 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackMinDistance, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackMaxDistance, 0.1f);

            // 실제 공격 범위 (적중 구체)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackCenter, attackRadius);

            // 공격 방향 선
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * attackMaxDistance);
        }
    }
}
