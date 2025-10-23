using UnityEngine;

public class ShovelHuman : BaseMonster
{
    [Header("원거리 공격 설정")]
    public GameObject projectilePrefab;  
    public Transform throwPoint;         
    public float projectileSpeed = 6f;   
    public Animator animator;

    protected override void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        if (projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: 투사체 프리팹 또는 ThrowPoint 미지정");
            return;
        }

        
        Vector2 direction = (player.position - throwPoint.position).normalized;

        
        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
        {
            rbProj.velocity = direction * projectileSpeed;
        }

        Debug.Log($"{gameObject.name}이(가) 투사체 발사!");
    }

    protected override void MoveTowardsPlayer()
    {
        
        float distance = Vector2.Distance(transform.position, player.position);

        
        if (distance > attackRange * 1.2f)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector2 dir = (player.position - transform.position).normalized;

            if (dir.x != 0)
                transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

            rb.MovePosition(rb.position + dir * step);
        }
        
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
