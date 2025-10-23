using UnityEngine;

public class AxeHuman : BaseMonster
{
    [Header("AI")]
    public float detectedRange = 3f;
    public float stopDistance = 1f;


    [Header("���� ����")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;
    public Animator animator;

    protected override void MoveTowardsPlayer()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if(distance <= stopDistance)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        Vector2 dir = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.deltaTime);

        animator.SetBool("IsMoving", true); 
    }

    protected override void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hit != null)
        {
            Debug.Log("AxeHuman�� �÷��̾� ����!");
            // PlayerController2D ���� ���� ������ ���� �ڵ� �ֱ�
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
