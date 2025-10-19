using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AxeHumanController : BaseMonster
{
    [Header("Animator")]
    public Animator animator;

    protected override void MoveTowardsPlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // ���� ���� ���̸� �̵�, �����̸� ����
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
        if (animator != null) animator.SetTrigger("Attack");
        Debug.Log($"{gameObject.name} ���� ����!");
    }
}
