using UnityEngine;

public class AxeHuman : BaseMonster
{
    [Header("도끼 주민 전용 설정")]
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
        Debug.Log("도끼 주민이 공격합니다!");

        // 플레이어 방향으로 약간 돌진
        Vector2 dir = (player.position - transform.position).normalized;
        rb.AddForce(dir * attackMoveDistance, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDelay);
        isAttacking = false;
    }
}
