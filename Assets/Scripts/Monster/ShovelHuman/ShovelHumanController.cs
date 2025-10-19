using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShovelHumanController : BaseMonster
{
    [Header("투사체")]
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float projectileSpeed = 5f;

    [Header("Animator")]
    public Animator animator;

    protected override void Attack()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            MeleeAttack();
        }
        else if (distance <= detectRange)
        {
            ThrowProjectile();
        }
    }
    protected override void MoveTowardsPlayer()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange && distance > attackRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }
    }

    protected override void TryAttack(float distance)
    {
        if (Time.time - lastAttackTime < attackDelay) return;

        if (distance <= attackRange)
        {
            MeleeAttack();
            lastAttackTime = Time.time;
        }
        else if (distance <= detectRange)
        {
            ThrowProjectile();
            lastAttackTime = Time.time;
        }
    }

    protected override void Update()
    {
        if( isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        MoveTowardsPlayer();

        if(Time.time - lastAttackTime > attackDelay)
            Attack();
    }

    void MeleeAttack()
    {
        animator.SetTrigger("Attack");
        Debug.Log($"{gameObject.name} 근접 공격!");
    }

    void ThrowProjectile()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
        {
            Vector2 direction = (player.position - throwPoint.position).normalized;
            rbProj.velocity = direction * projectileSpeed;  // Rigidbody2D에서 velocity 사용
        }

        Debug.Log($"{gameObject.name} 투사체 발사!");
    }
}
