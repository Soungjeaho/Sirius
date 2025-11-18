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
    [SerializeField] private HeavyFloat heavyFloat; // 무거운 찌 추가
    [SerializeField] private HookModeUI hookModeUI;

    private Rigidbody2D rb;
    private float xAxis;
    private int jumpCount;
    private bool wasGrounded = false; // 직전 프레임의 착지 상태 저장
    private bool canAttack = true;

    private int hookMode = 1; // 1: Normal, 2: Heavy

    private Vector2 lastAttackDir = Vector2.right;

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
        SetHookMode(1); // 시작 시 Normal 상태
    }

    void Update()
    {
        HandleHookSwitch();
        GetInputs();

        if (!reelback.IsGrappling)
        {
            Move();
        }

        Jump();
        Attack();

        // ✅ 착지 상태 갱신
        bool groundedNow = Grounded();

        // 착지한 순간에만 jumpCount 초기화
        if (groundedNow && !wasGrounded)
        {
            jumpCount = 0;
        }

        wasGrounded = groundedNow;
    }
    // 마우스 휠로 찌 교체
    private void HandleHookSwitch()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        int prevMode = hookMode; // 이전 모드 저장

        if (scroll > 0f)
        {
            hookMode--;
        }
        else if (scroll < 0f)
        {
            hookMode++;
        }

        hookMode = Mathf.Clamp(hookMode, 1, 2);

        // 🔹 모드가 실제로 바뀐 경우에만 적용
        if (hookMode != prevMode)
        {
            SetHookMode(hookMode);
        }
    }

    // 찌 교체 로직
    private void SetHookMode(int mode)
    {
        switch (mode)
        {
            case 1: // Normal Hook
                reelback.enabled = true;
                if (heavyFloat != null) heavyFloat.enabled = false;
                break;

            case 2: // Heavy Hook
                reelback.enabled = false;
                if (heavyFloat != null) heavyFloat.enabled = true;
                break;
        }

        if (hookModeUI != null) 
            hookModeUI.UpdateUI(mode);
        //  실제로 변경된 경우에만 한 번 출력
        Debug.Log($"[Hook Mode] 현재 찌: {(mode == 1 ? "Normal" : "Heavy")}");
    }
    private void GetInputs()
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
        if (hookMode == 2)
            return;

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;
        lastAttackDir = dir;
        Vector2 attackCenter = (Vector2)transform.position + dir * attackMaxDistance;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
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

        if (Camera.main != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = Application.isPlaying ? lastAttackDir : (mousePos - (Vector2)transform.position).normalized;
            Vector2 attackCenter = (Vector2)transform.position + dir * attackMaxDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackMinDistance, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackMaxDistance, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackCenter, attackRadius);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * attackMaxDistance);
        }
    }
}
