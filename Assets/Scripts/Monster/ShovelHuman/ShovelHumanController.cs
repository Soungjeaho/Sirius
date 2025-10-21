using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShovelHumanController : BaseMonster
{
    [Header("Animator")]
    public Animator animator;

    [Header("투사체")]
    public GameObject projectilePrefab;
    public Transform throwPoint;
    public float projectileSpeed = 5f;

    protected override void MoveTowardsPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // 공격 범위 밖이면 이동
        if (distance > attackRange)
        {
            Vector2 dir = new Vector2(player.position.x - transform.position.x, 0).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);

            if (animator != null) animator.SetBool("IsMoving", true);
        }
        else
        {
            if (animator != null) animator.SetBool("IsMoving", false);
        }
    }

    protected override void Attack()
    {
        animator.SetTrigger("Attack");
        ThrowProjectile();
        Debug.Log($"{gameObject.name} 투사체 공격!");
    }

    private void ThrowProjectile()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
        {
            Vector2 dir = (player.position - throwPoint.position).normalized;
            rbProj.velocity = dir * projectileSpeed;
        }
    }
}
