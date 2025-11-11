using UnityEngine;
using System.Collections;

public class HeavyFloatProjectile : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float enemyDestroyDelay = 0.2f;
    [SerializeField] private float remainTime = 1.0f;
    [SerializeField] private float backDeleteDistance = 1.0f;

    private Transform firePoint;
    private Vector2 fireDir;
    private bool initialized = false;
    private bool isDying = false;
    private bool hasActivatedPlatform = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = 1.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if (initialized && firePoint != null)
            CheckBehindPlayer();
    }

    private void CheckBehindPlayer()
    {
        Vector2 currentDir = ((Vector2)transform.position - (Vector2)firePoint.position).normalized;
        float dot = Vector2.Dot(fireDir, currentDir);
        float distance = Vector2.Distance(transform.position, firePoint.position);

        if (dot < 0f && distance > backDeleteDistance)
            SafeDestroy();
    }

    public void SetFirePoint(Transform point)
    {
        firePoint = point;
        StartCoroutine(InitializeFireDirection());
    }

    private IEnumerator InitializeFireDirection()
    {
        yield return null;
        if (firePoint != null)
        {
            fireDir = ((Vector2)transform.position - (Vector2)firePoint.position).normalized;
            initialized = true;
        }
    }

    //  충돌 방식 변경 (Trigger → Collision)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying)
            return;

        // Player 즉시 제거
        if (collision.collider.CompareTag("Player"))
        {
            SafeDestroy();
            return;
        }

        // Enemy 피격
        if (collision.collider.CompareTag("Enemy"))
        {
            HandleEnemyHit(collision.collider);
            return;
        }

        // BalancePlatform: 최초 1회만 작동
        if (collision.collider.CompareTag("BalancePlatform"))
        {
            if (!hasActivatedPlatform)
            {
                hasActivatedPlatform = true;
                HandleBalancePlatformCollision(collision.collider);
            }
            StartCoroutine(DestroyAfterDelay());
            return;
        }

        // BalanceTilemap
        //if (collision.collider.CompareTag("BalanceTilemap"))
        //{
        //    var tilemap = collision.collider.GetComponent<BalanceTilemap>();
        //    if (tilemap != null)
        //    {
        //        Vector2 hitPoint = collision.contacts[0].point;
        //        Vector2 hitDir = ((Vector2)collision.collider.transform.position - (Vector2)transform.position).normalized;
        //        tilemap.OnHeavyHit(hitDir, hitPoint);
        //    }
        //    StartCoroutine(DestroyAfterDelay());
        //    return;
        //}

        // Ground / RB_Wall / Obstacle
        if (collision.collider.CompareTag("Ground") ||
            collision.collider.CompareTag("RB_Wall") ||
            collision.collider.CompareTag("Obstacle"))
        {
            SafeDestroy();
        }
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

        StartCoroutine(DestroyAfterDelay(enemyDestroyDelay));
    }

    private void HandleBalancePlatformCollision(Collider2D col)
    {
        BalancePlatform platform = col.GetComponent<BalancePlatform>();
        if (platform == null)
            return;

        Vector2 hitPoint = col.ClosestPoint(transform.position);
        Vector2 hitDir = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;

        platform.OnHeavyHit(hitDir, hitPoint);

        //  살짝 아래로 반발시켜 멈춤 방지
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 충돌 순간 정지 방지용
            rb.AddForce(Vector2.down * 3f, ForceMode2D.Impulse); // 살짝 떨어지도록
        }

        //  짧은 시간 후 자동 제거
        StartCoroutine(DestroyAfterDelay(0.6f));
    }


    private IEnumerator DestroyAfterDelay(float delay = -1f)
    {
        if (delay < 0)
            delay = remainTime;

        yield return new WaitForSeconds(delay);
        SafeDestroy();
    }

    private void SafeDestroy()
    {
        if (isDying)
            return;

        isDying = true;
        Destroy(gameObject);
    }
}
