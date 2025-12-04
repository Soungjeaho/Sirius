using Project.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5;
    [SerializeField] private float runSpeed = 8;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private int maxJumpCount = 2;

    [Header("Run Settings")]
    [SerializeField] private float doubleTapTime = 0.25f;

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
    private bool isAttackLoopRunning = false;

    [Header("Mouse Front Limit")]
    [SerializeField] private float mouseFrontOffsetX = 0.2f;   // 손 기준으로 얼마나 앞에서부터 허용할지

    [Header("Attack VFX")]
    [SerializeField] private GameObject slashVFX;
    [SerializeField] private float slashAngleOffset = 0f;
    [SerializeField] private float slashBaseLength = 1f; // scale.x = 1일 때 이펙트의 기준 길이


    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnergyGauge energyGauge;
    [SerializeField] private Reelback reelback;
    [SerializeField] private HeavyFloat heavyFloat;
    [SerializeField] private HookModeUI hookModeUI;

    private Rigidbody2D rb;
    private float xAxis;
    private int jumpCount;
    private bool wasGrounded = false;
    private bool facingRight = true;

    private bool isRunning = false;
    private float lastTapTime = -1f;
    private int lastTapDir = 0; // -1(left), 1(right), 0(none)

    private int hookMode = 1; // 1: Normal, 2: Heavy
    private Vector2 lastAttackDir = Vector2.right;

    public static PlayerController Instance;

    public bool IsRunning
    {
        get
        {
            return isRunning;
        }
    }

    public float HorizontalInput
    {
        get
        {
            return xAxis;
        }
    }

    public bool IsNormalHook
    {
        get
        {
            return hookMode == 1;
        }
    }

    public float RunSpeed
    {
        get
        {
            return runSpeed;
        }
    }

    public bool FacingRight
    {
        get
        {
            return facingRight;
        }
    }

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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetHookMode(1);
    }

    private void Update()
    {
        HandleHookSwitch();
        GetInputs();
        HandleFlip();

        if (!reelback.IsGrappling)
        {
            Move();
        }

        Jump();
        Attack();

        bool groundedNow = Grounded();

        if (groundedNow && !wasGrounded)
        {
            jumpCount = 0;
        }

        wasGrounded = groundedNow;
    }

    private void LateUpdate()
    {
        AimFirePointToMouse();
    }

    private void HandleFlip()
    {
        if (xAxis > 0.01f && !facingRight)
        {
            Flip();
        }
        else if (xAxis < -0.01f && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        if (spriteRenderer != null)
        {
            Transform spriteTr = spriteRenderer.transform;
            Vector3 scale = spriteTr.localScale;

            scale.x = facingRight ? 1f : -1f;
            spriteTr.localScale = scale;
        }

        if (firePoint != null)
        {
            Vector3 pos = firePoint.localPosition;

            if (facingRight)
            {
                pos.x = 0f;
            }
            else
            {
                pos.x = -1f;
            }

            firePoint.localPosition = pos;
        }
    }

    private void AimFirePointToMouse()
    {
        if (firePoint == null)
        {
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - firePoint.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    private void HandleHookSwitch()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        int prevMode = hookMode;

        if (scroll > 0f)
        {
            hookMode--;
        }
        else if (scroll < 0f)
        {
            hookMode++;
        }

        hookMode = Mathf.Clamp(hookMode, 1, 2);

        if (hookMode != prevMode)
        {
            SetHookMode(hookMode);
        }
    }

    private void SetHookMode(int mode)
    {
        switch (mode)
        {
            case 1:
                {
                    reelback.enabled = true;
                    if (heavyFloat != null)
                    {
                        heavyFloat.enabled = false;
                    }
                    break;
                }

            case 2:
                {
                    reelback.enabled = false;
                    if (heavyFloat != null)
                    {
                        heavyFloat.enabled = true;
                    }
                    break;
                }
        }

        if (hookModeUI != null)
        {
            hookModeUI.UpdateUI(mode);
        }

        Debug.Log($"[Hook Mode] 현재 찌: {(mode == 1 ? "Normal" : "Heavy")}");
    }

    private void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");

        int currentTapDir = 0;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentTapDir = -1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentTapDir = 1;
        }

        if (currentTapDir != 0)
        {
            if (currentTapDir == lastTapDir && Time.time - lastTapTime <= doubleTapTime)
            {
                isRunning = true;
            }

            lastTapDir = currentTapDir;
            lastTapTime = Time.time;
        }

        if (Mathf.Approximately(xAxis, 0f))
        {
            isRunning = false;
        }
        else if (Mathf.Sign(xAxis) != lastTapDir)
        {
            isRunning = false;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (!Mathf.Approximately(xAxis, 0f))
            {
                isRunning = true;
            }
        }
    }

    private void Move()
    {
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(targetSpeed * xAxis, rb.velocity.y);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (Grounded() || jumpCount < maxJumpCount)
            {
                if (reelback.IsGrappling)
                {
                    reelback.StopGrapple();
                }

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
        {
            return;
        }

        // 마우스가 FirePoint y선을 벗어나면 공격 금지
        if (!IsMouseInFrontOfPlayer())
        {
            return;
        }

        if (isAttackLoopRunning)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            StartCoroutine(AttackLoop());
        }
    }


    private IEnumerator AttackLoop()
    {
        isAttackLoopRunning = true;

        while (Input.GetMouseButton(0))
        {
            DoAttackOnce();
            yield return new WaitForSeconds(attackDelay);
        }

        isAttackLoopRunning = false;
    }

    private void DoAttackOnce()
    {
        if (!IsMouseInFrontOfPlayer())
        {
            return;
        }

        bool killedEnemy = false;

        Vector2 origin = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - origin).normalized;
        lastAttackDir = dir;

        // 최대 사거리 지점(판정 중심)
        Vector2 attackCenter = origin + dir * attackMaxDistance;

        // 이펙트는 min~max 구간의 중간 지점에 배치
        float midDistance = (attackMinDistance + attackMaxDistance) * 0.5f;
        Vector2 visualCenter = origin + dir * midDistance;

        SpawnSlashEffect(visualCenter, dir);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy"))
            {
                continue;
            }

            float dist = Vector2.Distance(origin, hit.transform.position);

            if (dist < attackMinDistance || dist > attackMaxDistance)
            {
                continue;
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead)
            {
                continue;
            }

            Vector2 hitPoint = hit.ClosestPoint(attackCenter);
            Vector2 hitNormal = -dir;

            damageable.ApplyDamage(attackDamage, hitPoint, hitNormal, this);

            if (damageable.IsDead)
            {
                killedEnemy = true;
            }
        }

        if (killedEnemy && energyGauge != null)
        {
            energyGauge.AddGauge(1);
        }
    }


    private void SpawnSlashEffect(Vector2 center, Vector2 dir)
    {
        if (slashVFX == null)
        {
            return;
        }

        GameObject vfx = Instantiate(slashVFX, center, Quaternion.identity);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx.transform.rotation = Quaternion.Euler(0f, 0f, angle + slashAngleOffset);

        // 공격 구간 길이 = max - min
        float attackLength = Mathf.Max(0f, attackMaxDistance - attackMinDistance);

        // slashBaseLength가 0 이하로 들어가면 나눗셈 방지용
        if (slashBaseLength <= 0f)
        {
            slashBaseLength = 1f;
        }

        // 현재 이펙트의 기본 스케일 가져온 뒤 x만 조정
        Vector3 scale = vfx.transform.localScale;
        scale.x = attackLength / slashBaseLength;
        vfx.transform.localScale = scale;

        Destroy(vfx, 1.0f);
    }

    public bool Grounded()
    {
        return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
    }

    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }
    public bool IsMouseInFrontOfPlayer()
    {
        if (firePoint == null || Camera.main == null)
        {
            return true;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float dx = mouseWorld.x - firePoint.position.x;

        // 플레이어가 오른쪽을 보고 있을 때는 firePoint보다 "오른쪽"이 앞
        if (facingRight)
        {
            return dx >= mouseFrontOffsetX;
        }
        // 플레이어가 왼쪽을 보고 있을 때는 firePoint보다 "왼쪽"이 앞
        else
        {
            return dx <= -mouseFrontOffsetX;
        }
    }



    // 공격범위 체크용 기즈모
    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (groundCheckPoint != null)
        {
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (Camera.main != null)
        {
            Vector2 origin = firePoint != null
                ? (Vector2)firePoint.position
                : (Vector2)transform.position;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = Application.isPlaying
                ? lastAttackDir
                : (mousePos - origin).normalized;

            Vector2 attackCenter = origin + dir * attackMaxDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin + dir * attackMinDistance, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin + dir * attackMaxDistance, 0.1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackCenter, attackRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, origin + dir * attackMaxDistance);
        }
    }*/
}
