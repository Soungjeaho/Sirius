using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeavyFloatProjectile : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] private int damage = 2;
    [SerializeField] private float knockbackForce = 20f;
    [SerializeField] private float enemyDestroyDelay = 0.2f;
    [SerializeField] private float remainTime = 1.0f;
    [SerializeField] private float backDeleteDistance = 1.0f;

    [Header("게이지 소모")]
    private EnergyGauge gaugeRef;
    private int gaugeCost = 0;

    private Transform firePoint;
    private Vector2 fireDir;
    private bool initialized = false;
    private bool isDying = false;
    private bool hasActivatedPlatform = false;
    public bool IsOnSwitch { get; private set; } = false;


    private Rigidbody2D rb;
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>(); // ✅ 중복 피격 방지용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDying)
            return;

        //  Switch 충돌 감지
        if (collision.collider.CompareTag("Switch"))
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.y <= 0)
            {
                Vector3 pos = transform.position;
                pos.y = collision.collider.bounds.max.y + GetComponent<Collider2D>().bounds.extents.y;
                transform.position = pos;
                rb.velocity = Vector2.zero;
            }

            IsOnSwitch = true;
            return;
        }

        // 이하 기존 코드 유지
        if (collision.collider.CompareTag("CrackedTilemap"))
        {
            var cracked = collision.collider.GetComponent<CrackedTilemap>();
            if (cracked != null)
            {
                Vector2 hitPoint = collision.contacts[0].point;
                cracked.OnHeavyHit(hitPoint);
            }
            StartCoroutine(DestroyAfterDelay(enemyDestroyDelay));
            return;
        }

        if (collision.collider.CompareTag("Player"))
        {
            SafeDestroy();
            return;
        }

        if (collision.collider.CompareTag("Enemy"))
        {
            if (gaugeRef != null)
                gaugeRef.UseGauge(gaugeCost);
            HandleEnemyHit(collision.collider);
            return;
        }

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

        if (collision.collider.CompareTag("Ground") ||
            collision.collider.CompareTag("RB_Wall") ||
            collision.collider.CompareTag("Obstacle"))
        {
            SafeDestroy();
        }
    }

    private void HandleEnemyHit(Collider2D enemyCol)
    {
        GameObject enemyObj = enemyCol.gameObject;

        //  이미 처리한 적이면 리턴 (중복 방지)
        if (damagedEnemies.Contains(enemyObj))
            return;

        damagedEnemies.Add(enemyObj);

        EnemyHealth enemy = enemyObj.GetComponent<EnemyHealth>();
        if (enemy != null)
            enemy.TakeDamage(damage);

        Rigidbody2D enemyRb = enemyObj.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 dir = (enemyObj.transform.position - transform.position).normalized;
            enemyRb.velocity = Vector2.zero; // 기존 속도 초기화
            enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
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

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.down * 3f, ForceMode2D.Impulse);
        }

        StartCoroutine(DestroyAfterDelay(0.6f));
    }

    public void SetGaugeReference(EnergyGauge gauge, int cost)
    {
        gaugeRef = gauge;
        gaugeCost = cost;
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
