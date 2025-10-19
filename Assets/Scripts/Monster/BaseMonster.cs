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
    public float detectRange = 5f;   // 플레이어 감지 거리
    public float attackRange = 1f;   // 공격 사거리
    public float attackDelay = 2f;   // 공격 쿨타임
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

        // 땅에 있을 때만 이동 + 공격 시도
        if (distance < detectRange && isGrounded)
        {
            MoveTowardsPlayer();
            TryAttack(distance);
        }
    }

    

    protected virtual void MoveTowardsPlayer()
    {

        
        // 공격 사거리보다 멀 때만 이동
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange * 0.9f) return;

        float step = moveSpeed * Time.deltaTime;
        Vector2 dir = (player.position - transform.position).normalized;

        // 좌우 방향 전환 (Sprite 방향 전환용)
        if (dir.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);
        }

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
        Debug.Log($"{gameObject.name}이(가) 쓰러졌습니다.");
        Destroy(gameObject, 0.5f);
    }

    // 에디터에서 GroundCheck 위치 확인용 (Scene 뷰에 원 그리기)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
