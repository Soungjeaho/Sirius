using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("플레이어 스탯")]
    public PlayerStats stats = new PlayerStats();

    [Header("상호작용 키")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI 연결")]
    public GameObject inventoryPanel;
    public GameObject mapPanel;
    public GameObject menuPanel;
    public GameObject gaugePanel; // 게이지 UI 패널 (추후 연결)

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private float moveInput;
    private bool isGrounded = false;
    private bool canDoubleJump = false;

    // 달리기 관련
    public float doubleTapTime = 0.5f;
    private float lastKeyTime = -1f;
    private KeyCode lastKey;
    private bool isRunning = false;

    // 공격 쿨타임
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        stats.currentHP = stats.maxHP;
        stats.gaugeCurrent = stats.gaugeMax;

        // UI 초기화
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gaugePanel != null) gaugePanel.SetActive(false);
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    #region 이동 / 점프 / 달리기
    void HandleInput()
    {
        moveInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
            HandleRun(KeyCode.A);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
            HandleRun(KeyCode.D);
        }

        // 점프 / 더블점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump(stats.jumpForce);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                Jump(stats.doubleJumpForce);
                canDoubleJump = false;
            }
        }

        // 상호작용
        if (Input.GetKeyDown(interactKey)) Interact();

        // 인벤토리 / 지도 / 메뉴
        if (Input.GetKeyDown(KeyCode.I) && inventoryPanel != null)
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.Tab) && mapPanel != null)
            mapPanel.SetActive(!mapPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.Escape) && menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);
    }

    void Move()
    {
        float speed = isRunning ? stats.runSpeed : stats.walkSpeed;
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        // 스프라이트 반전
        if (moveInput < 0) sr.flipX = true;
        if (moveInput > 0) sr.flipX = false;

        // 애니메이션 이동 파라미터
        if (anim != null) anim.SetBool("isMoving", moveInput != 0);
    }

    void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        if (anim != null) anim.SetTrigger("Jump");
    }

    void HandleRun(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            if (lastKey == key && (Time.time - lastKeyTime) <= doubleTapTime)
            {
                isRunning = true;
            }

            lastKey = key;
            lastKeyTime = Time.time;
        }

        if (Input.GetKey(key))
            ; // 속도 적용은 Move()에서 처리

        if (Input.GetKeyUp(key))
        {
            isRunning = false;
        }
    }
    #endregion

    #region 공격 / 게이지
    public void Attack()
    {
        if (Time.time - lastAttackTime < stats.attackDelay) return;

        lastAttackTime = Time.time;

        // 공격 범위 내 적 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponent<BaseMonster>();
                if (enemy != null)
                {
                    enemy.TakeDamage(stats.attackDamage);
                    Debug.Log($"적 피격! 피해: {stats.attackDamage}");
                }
            }
        }

        if (anim != null) anim.SetTrigger("Attack");
    }

    public void RecoverGaugeOnKill()
    {
        stats.gaugeCurrent += stats.gaugeRecoverOnKill;
        if (stats.gaugeCurrent > stats.gaugeMax) stats.gaugeCurrent = stats.gaugeMax;
    }

    #endregion

    #region 상호작용
    void Interact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Interactable"))
            {
                var interactable = hit.GetComponent<Interactable>();
                interactable?.OnInteract();
            }
        }
    }
    #endregion

    #region 충돌
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
    #endregion

    #region 디버그
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        Gizmos.DrawWireSphere(transform.position, 1.5f); // 상호작용 반경
    }
    #endregion
}

[System.Serializable]
public class PlayerStats
{
    public int maxHP = 5;
    public int currentHP;

    public float walkSpeed = 30f;
    public float runSpeed => walkSpeed * 1.5f;

    public float jumpForce = 3f;
    public float doubleJumpForce = 2f;

    public int attackDamage = 1;
    public float attackRange = 25f;
    public float attackDelay = 1.5f;

    public int gaugeMax = 12;
    public int gaugeCurrent;
    public int gaugeRecoverOnKill = 3;

    // 성장용
    public void IncreaseMaxHP(int value) { maxHP += value; currentHP += value; }
    public void IncreaseGaugeMax(int value) { gaugeMax += value; gaugeCurrent += value; }
    public void DoubleAttackDamage() { attackDamage *= 2; }
}
