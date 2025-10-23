using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordHumanController : MonoBehaviour
{
    [Header("기본 스탯")]
    public int maxHP = 4;
    private int currentHP;

    public float moveSpeed = 1.5f;

    [Header("공격")]
    public int attackDamage = 2;
    public float attackRange = 35f;
    public float jumpAttackRange = 50f;
    public float jumpSpeed = 5f;

    public float attackDelay = 2.5f;

    [Header("Hitbox")]
    public Collider2D hitbox; // Inspector에서 할당

    [Header("Animator")]
    public Animator anim;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime = 0f;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (hitbox != null)
            hitbox.enabled = false; // 평소에는 Hitbox 비활성화
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            if (Time.time - lastAttackTime > attackDelay)
            {
                lastAttackTime = Time.time;
                StartCoroutine(JumpAttack());
                StartCoroutine(HorizontalSlash());
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
        if (anim != null) anim.SetBool("isMoving", true);
    }

    IEnumerator JumpAttack()
    {
        if (anim != null) anim.SetTrigger("JumpAttack");
        if (hitbox != null) hitbox.enabled = true;

        // 플레이어 방향으로 점프 이동
        Vector2 dir = (player.position - transform.position).normalized;
        float jumpDuration = 0.5f;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            rb.MovePosition(rb.position + dir * jumpSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f); // 경직
        if (hitbox != null) hitbox.enabled = false;
        Debug.Log("점프 공격 완료!");
    }

    IEnumerator HorizontalSlash()
    {
        if (hitbox != null) hitbox.enabled = true;

        for (int i = 0; i < 2; i++)
        {
            if (anim != null) anim.SetTrigger("Slash");
            Debug.Log("횡베기 공격!");
            yield return new WaitForSeconds(0.5f);
        }

        if (hitbox != null) hitbox.enabled = false;
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
        Debug.Log($"{gameObject.name}이 쓰러졌습니다.");
        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log("플레이어 피격! (실제 HP는 없다)");
        }
    }
}
