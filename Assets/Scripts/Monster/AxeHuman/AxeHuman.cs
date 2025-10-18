using UnityEngine;

public class AxeHuman : BaseMonster
{
    [Header("���� �ֹ� ���� ����")]
    public float attackMoveDistance = 1f;
    public float knockbackDelay = 0.5f;
    public Animator animator;

    private bool isAttacking = false;

    protected override void Attack()
    {
        if (isAttacking) return;
        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        Debug.Log("���� �ֹ��� �����մϴ�!");

        // �÷��̾� �������� �ణ ����
        Vector2 dir = (player.position - transform.position).normalized;
        rb.AddForce(dir * attackMoveDistance, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDelay);
        isAttacking = false;
    }
}
