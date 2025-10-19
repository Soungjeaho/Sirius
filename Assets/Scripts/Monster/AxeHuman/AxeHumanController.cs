using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AxeHumanController : MonoBehaviour
{
    [Header("�⺻ ����")]
    public int maxHP = 2;
    private int currentHP;

    public float moveSpeed = 2f;
    public int contactDamage = 1;

    [Header("����")]
    public float detectRange = 5f;
    public float attackRange = 1f;
    public float attackDelay = 2f;
    public int attackDamage = 1;

    [Header("Animator")]
    public Animator animator;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime = 0f;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            MoveTowardsPlayer(distance);
            TryAttack(distance);
        }
        else
        {
            SetMovingAnimation(false);
        }
    }

    void MoveTowardsPlayer(float distance)
    {
        // ���� ���� ���̸� �̵�
        if (distance > attackRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
            SetMovingAnimation(true);
        }
        else
        {
            SetMovingAnimation(false);
        }
    }

    void TryAttack(float distance)
    {
        if (Time.time - lastAttackTime < attackDelay) return;

        if (distance <= attackRange)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
        Debug.Log($"{gameObject.name} ����!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ������!");
        Destroy(gameObject, 0.5f);
    }

    void SetMovingAnimation(bool moving)
    {
        if (animator != null)
            animator.SetBool("IsMoving", moving);
    }
}
