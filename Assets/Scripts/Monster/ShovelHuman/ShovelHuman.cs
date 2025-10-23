using UnityEngine;

public class ShovelHuman : BaseMonster
{
    [Header("���Ÿ� ���� ����")]
    public GameObject projectilePrefab;  // ���� ����ü ������
    public Transform throwPoint;         // ������ ��ġ
    public float projectileSpeed = 6f;   // ����ü �ӵ�
    public Animator animator;

    protected override void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        if (projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: ����ü ������ �Ǵ� ThrowPoint ������");
            return;
        }

        // �÷��̾� ���� ���
        Vector2 direction = (player.position - throwPoint.position).normalized;

        // ����ü ���� �� �߻�
        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
        {
            rbProj.velocity = direction * projectileSpeed;
        }

        Debug.Log($"{gameObject.name}��(��) ����ü �߻�!");
    }

    protected override void MoveTowardsPlayer()
    {
        // ShovelHuman�� �������� ���� �ʰ�, ��Ÿ� ���� ���� �ٰ���
        float distance = Vector2.Distance(transform.position, player.position);

        // ��Ÿ����� �ָ� õõ�� ����
        if (distance > attackRange * 1.2f)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector2 dir = (player.position - transform.position).normalized;

            if (dir.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

            rb.MovePosition(rb.position + dir * step);
        }
        // ��Ÿ� �����̸� ���ڸ� ��� (���ݸ� ����)
    }

    private void OnDrawGizmosSelected()
    {
        if (throwPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(throwPoint.position, 0.1f);
        }
    }
}
