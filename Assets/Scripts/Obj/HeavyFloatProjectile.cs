using Project.Combat; // IDamageable 사용
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeavyFloatProjectile : MonoBehaviour
{
    [Header("Attack VFX")] 
    [SerializeField] private GameObject hitEffectPrefab; // 이펙트 프리팹 
    [SerializeField] private float hitEffectOffset = 0.1f; // 충돌 지점 보정 거리 

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

        // 제외 태그 목록: Switch, CrackedTilemap, Player, Enemy, BalancePlatform
        string tag = collision.collider.tag;

        // ----------------------------------------------------------------------
        // 1. 특별한 상호작용이 필요한 태그 처리 (기존 로직 유지)
        // ----------------------------------------------------------------------

        if (tag == "Switch")
        {
            // ... (Switch 처리 로직 유지) ...
            IsOnSwitch = true;
            return;
        }

        if (tag == "CrackedTilemap")
        {
            // ... (CrackedTilemap 처리 로직 유지) ...
            StartCoroutine(DestroyAfterDelay(enemyDestroyDelay));
            return;
        }

        if (tag == "Player")
        {
            SafeDestroy();
            return;
        }

        if (tag == "Enemy")
        {
            HandleEnemyHit(collision.collider);
            return;
        }

        if (tag == "BalancePlatform")
        {
            if (!hasActivatedPlatform)
            {
                hasActivatedPlatform = true;
                HandleBalancePlatformCollision(collision.collider);
            }
            StartCoroutine(DestroyAfterDelay());
            return;
        }

        // ----------------------------------------------------------------------
        // 2. 기타 모든 충돌 처리 (이펙트 생성 및 파괴)
        // ----------------------------------------------------------------------

        // 기타 모든 충돌 (Ground, RB_Wall, Obstacle 등)
        // 혹은 위의 if/return 문에 해당하지 않는 모든 태그가 여기에 해당됨

        // 이펙트를 생성할 충돌 지점 계산
        Vector2 hitPoint = collision.contacts[0].point;
        Vector2 hitNormal = collision.contacts[0].normal;

        // 이펙트 생성 후 파괴
        SpawnHitEffectAndDestroy(hitPoint, hitNormal);
    }

    private void HandleEnemyHit(Collider2D enemyCol)
    {
        GameObject enemyObj = enemyCol.gameObject;

        // 이미 처리한 적이면 리턴 (중복 방지)
        if (damagedEnemies.Contains(enemyObj))
        {
            return;
        }

        damagedEnemies.Add(enemyObj);

        //  Enemy Rigidbody2D (보통 부모에 있을 수도 있으니 InParent로)
        Rigidbody2D enemyRb = enemyCol.GetComponentInParent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 dir = ((Vector2)enemyRb.position - (Vector2)transform.position).normalized;

            // 살짝 위로 튕기고 싶으면 Y를 약간 올리자
            dir.y += 0.3f;
            dir = dir.normalized;

            enemyRb.velocity = Vector2.zero;
            enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        // 데미지 주기 (EnemyHealth / EnemyBase는 건들지 않고 IDamageable만 사용)
        IDamageable damageable = enemyCol.GetComponentInParent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            Vector2 hitPoint = enemyCol.ClosestPoint(transform.position);
            Vector2 hitNormal = ((Vector2)enemyObj.transform.position - (Vector2)transform.position).normalized;

            damageable.ApplyDamage(damage, hitPoint, hitNormal, this);

            // 여기서도 "처치 시 게이지 +1"을 주고 싶으면 이 부분 유지
            if (gaugeRef != null && damageable.IsDead)
            {
                gaugeRef.AddGauge(1);
            }
        }

        StartCoroutine(DestroyAfterDelay(enemyDestroyDelay));
    }
    private void SpawnHitEffectAndDestroy(Vector2 collisionPoint, Vector2 collisionNormal)
    {
        if (hitEffectPrefab != null)
        {
            // 1. 위치 보정: 충돌 지점에서 법선 방향으로 Offset만큼 밀어냅니다.
            Vector2 spawnPosition = collisionPoint + collisionNormal * hitEffectOffset;

            // 2. 회전 설정: 법선 벡터를 사용하여 회전값(Quaternion)을 계산합니다.
            // Quaternion.LookRotation(forward, upwards)와 유사하게, 법선을 기준으로 z축 회전 계산
            Quaternion rotation = Quaternion.FromToRotation(Vector2.up, collisionNormal);

            // 3. 이펙트 생성
            GameObject effect = Instantiate(hitEffectPrefab, spawnPosition, rotation);
        }

        SafeDestroy();
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
