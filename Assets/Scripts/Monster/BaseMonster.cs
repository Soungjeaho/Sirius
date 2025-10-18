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

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < detectRange)
        {
            MoveTowardsPlayer();
            TryAttack(distance);
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        float step = moveSpeed * Time.deltaTime;
        Vector2 dir = (player.position - transform.position).normalized;
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
}
