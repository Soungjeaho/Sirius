using UnityEngine;

public class ShovelHuman : BaseMonster
{
    public float rangedAttackRange = 5f;
    public GameObject dirtProjectilePrefab;
    public Transform throwPoint;

    protected override void Attack()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange && distance <= rangedAttackRange)
        {
            Debug.Log("삽 주민이 흙을 던집니다!");
            if (dirtProjectilePrefab != null && throwPoint != null)
            {
                GameObject dirt = Instantiate(dirtProjectilePrefab, throwPoint.position, Quaternion.identity);
                Rigidbody2D rb = dirt.GetComponent<Rigidbody2D>();
                rb.velocity = (player.position - throwPoint.position).normalized * 4f;
            }
        }
        else
        {
            Debug.Log("삽 주민이 근접 공격!");
            // 근접 공격 로직 (충돌 체크 등)
        }
    }
}
