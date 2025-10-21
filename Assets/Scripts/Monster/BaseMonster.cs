using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseMonster : MonoBehaviour
{
    [Header("기본 스탯")]
    public int maxHP = 3;
    protected int currentHP;

    public float moveSpeed = 2f;
    public int contactDamage = 1;

    [Header("전투")]
    public float detectRange = 5f;
    public float attackRange = 1f;
    public float attackDelay = 2f;
    public int attackDamage = 1;

    [Header("지면 체크")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    protected bool isGrounded = false;

    protected Transform player;
    protected Rigidbody2D rb;
    protected bool isDead = false;
    protected float lastAttackTime = 0f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        // 땅 감지
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        // 플레이어 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        // 너무 멀면 비활성 (성능 절약)
        if (distance > detectRange * 1.5f) return;

        // 이동/공격
        if (distance < detectRange && isGrounded)
        {
            MoveTowardsPlayer();
            TryAttack(distance);
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange * 0.9f) return;

        float step = moveSpeed * Time.deltaTime;
        Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;

        // Sprite 좌우 전환
        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        rb.MovePosition(rb.position + dir * step);
    }

    protected virtual void TryAttack(float distance)
    {
        if (distance <= attackRange && Time.time - lastAttackTime > attackDelay)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    protected abstract void Attack();

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} 쓰러짐!");
        Destroy(gameObject, 0.5f);
    }

    // ----------------------
    // Player 공격 판정 추가
    // ----------------------
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
            Debug.Log($"{gameObject.name} Player 공격 피격!");
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
        }
    }

    // 에디터용 GroundCheck 표시
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
