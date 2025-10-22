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

    [Header("지면 체크 (선택사항)")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected LayerMask groundLayer;
    protected bool isGrounded = true;

    protected Transform player;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected bool isDead = false;
    protected float lastAttackTime = 0f;

    // ----------------------
    // 초기화
    // ----------------------
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectRange * 1.5f) return; // 너무 멀면 비활성

        if (distance < detectRange && isGrounded)
        {
            MoveTowardsPlayer();
            TryAttack(distance);
        }
    }

    // ----------------------
    // 이동 로직
    // ----------------------
    protected virtual void MoveTowardsPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange * 0.9f)
        {
            anim?.SetBool("isMoving", false);
            return;
        }

        Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);

        if (dir.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

        anim?.SetBool("isMoving", true);
    }

    // ----------------------
    // 공격 시도
    // ----------------------
    protected virtual void TryAttack(float distance)
    {
        if (Time.time - lastAttackTime < attackDelay) return;

        if (distance <= attackRange)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    // ----------------------
    // 자식에서 구현 (공격 패턴)
    // ----------------------
    protected abstract void Attack();

    // ----------------------
    // 피격 / 사망
    // ----------------------
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"{name} 피격! 남은 HP: {currentHP}");
        anim?.SetTrigger("Hit");

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{name} 사망");
        anim?.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        Destroy(gameObject, 1f);
    }

    // ----------------------
    // Player 공격 판정
    // ----------------------
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
            Debug.Log($"{name} Player 공격 피격!");
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("PlayerAttack"))
        {
            TakeDamage(1);
        }
    }

    // ----------------------
    // 에디터 시각화
    // ----------------------
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
