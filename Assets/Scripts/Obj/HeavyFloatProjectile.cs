using UnityEngine;
using System.Collections;

public class HeavyFloatProjectile : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private int damage = 2;                // 피해량
    [SerializeField] private float knockbackForce = 8f;     // 넉백 힘
    [SerializeField] private float reboundForce = 5f;       // 반동 세기
    [SerializeField] private float upwardBias = 0.5f;       // 튕김 시 위쪽 비율 (0.5면 위로 50%)
    [SerializeField] private float destroyDelay = 0.2f;     // 삭제 전 대기 시간

    private bool hasCollided = false;
    private bool reboundStarted = false;
    private Transform player;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasCollided) return;

        if (collision.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision);
        }
        else if (collision.CompareTag("RB_Wall") || collision.CompareTag("Ground"))
        {
            HandleRebound();
        }
        else
        {
            return;
        }

        hasCollided = true;
    }

    private void HandleEnemyHit(Collider2D enemyCol)
    {
        EnemyHealth enemy = enemyCol.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            Rigidbody2D enemyRb = enemyCol.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 dir = (enemyCol.transform.position - transform.position).normalized;
                enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        StartCoroutine(ReboundAndDestroy());
    }

    private void HandleRebound()
    {
        StartCoroutine(ReboundAndDestroy());
    }

    private IEnumerator ReboundAndDestroy()
    {
        if (reboundStarted) yield break;
        reboundStarted = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 현재 이동 방향의 반대 + 위쪽 성분 추가
            Vector2 reboundDir = (-rb.velocity.normalized + Vector2.up * upwardBias).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(reboundDir * reboundForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
