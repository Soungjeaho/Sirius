using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reelback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lr;
    [SerializeField] private GameObject hookProjectilePrefab; // ✅ 추가됨

    [Header("Grapple Settings")]
    [SerializeField] public float ReelbackSpeed = 15f;
    [SerializeField] private float maxReelDistance = 10f;
    [SerializeField] private float pullSpeed = 5f; // ✅ 추가됨
    [SerializeField] private LayerMask ReelbackLayer;
    [SerializeField] private LayerMask hookLayer; // ✅ 추가됨

    [Header("Cost")]
    [SerializeField] private EnergyGauge playerGauge;
    [SerializeField] private int gaugeCost = 3;

    private Rigidbody2D playerRb;
    private Vector2 bobberPoint;
    private Rigidbody2D pulledEnemy;

    // ✅ 추가 변수들
    private GameObject currentProjectile;
    private bool isHooked = false;
    private Vector2 hookPoint;

    public bool IsGrappling { get; private set; } = false;
    public bool IsPullingEnemy { get; private set; } = false;

    private void Start()
    {
        if (lr == null)
            lr = GetComponent<LineRenderer>();

        if (lr != null)
            lr.enabled = false;

        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleReekback();   // 기존 기능 유지
        HandleHook();       // ✅ 새로 추가된 투사체 Hook 기능
        UpdateReelLine();
    }

    // ✅ 포물선 Hook 투사체 관련
    private void HandleHook()
    {
        if (Input.GetMouseButtonDown(1) && currentProjectile == null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 direction = (mouseWorld - firePoint.position).normalized;

            currentProjectile = Instantiate(hookProjectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRb = currentProjectile.GetComponent<Rigidbody2D>();

            if (projRb != null)
            {
                projRb.velocity = direction * ReelbackSpeed; // 포물선 발사 속도
            }

            lr.enabled = true;
        }

        // E 키로 살짝 끌기
        if (isHooked && Input.GetKey(KeyCode.E))
        {
            Vector2 newPos = Vector2.MoveTowards(playerRb.position, hookPoint, pullSpeed * Time.deltaTime);
            playerRb.MovePosition(newPos);

            if (Vector2.Distance(playerRb.position, hookPoint) < 0.3f)
            {
                StopHook();
            }
        }

        // 마우스 오른쪽 떼면 해제
        if (Input.GetMouseButtonUp(1))
        {
            StopHook();
        }

        // 라인 업데이트
        if (lr.enabled)
        {
            lr.SetPosition(0, firePoint.position);
            if (isHooked)
                lr.SetPosition(1, hookPoint);
            else if (currentProjectile != null)
                lr.SetPosition(1, currentProjectile.transform.position);
        }
    }

    // ✅ HookProjectile.cs에서 호출될 함수
    public void OnHookHit(Collider2D hit, Vector2 point)
    {
        if (hit.CompareTag("HookableObj"))
        {
            isHooked = true;
            hookPoint = point;
            Debug.Log("Hook 연결됨: " + hit.name);
        }
    }

    private void StopHook()
    {
        isHooked = false;

        if (lr != null)
            lr.enabled = false;

        if (currentProjectile != null)
            Destroy(currentProjectile);

        currentProjectile = null;
    }

    // ✅ 기존 Reelback 기능 (벽이나 Enemy용)
    public void HandleReekback()
    {
        if (Input.GetMouseButtonDown(0)) // 왼쪽 클릭으로 기존 기능 사용
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            float distance = Vector2.Distance(firePoint.position, mouseWorld);
            if (distance > maxReelDistance)
            {
                Debug.Log("Reelback 실패: 최대 사거리 초과");
                return;
            }

            Collider2D hit = Physics2D.OverlapCircle(mouseWorld, 0.1f, ReelbackLayer);
            if (hit != null)
            {
                if (hit.CompareTag("RB_Wall"))
                {
                    StartReelback(mouseWorld);
                }
                else if (hit.CompareTag("Enemy"))
                {
                    if (playerGauge.UseGauge(gaugeCost))
                        StartPullEnemy(hit.GetComponent<Rigidbody2D>());
                    else
                        Debug.Log("게이지가 부족합니다!");
                }
            }
            else
            {
                Debug.Log("Reelback 실패: 대상 없음");
            }
        }

        if (IsGrappling)
            MoveToGrapplePoint();
        else if (IsPullingEnemy)
            PullEnemyToPlayer();

        if (Input.GetMouseButtonUp(0))
        {
            StopReelback();
            StopPullEnemy();
        }
    }

    private void StartReelback(Vector2 point)
    {
        bobberPoint = point;
        IsGrappling = true;
        lr.enabled = true;
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, bobberPoint);
    }

    public void StopReelback()
    {
        IsGrappling = false;
        lr.enabled = false;
    }

    private void MoveToGrapplePoint()
    {
        Vector2 newPos = Vector2.MoveTowards(playerRb.position, bobberPoint, ReelbackSpeed * Time.deltaTime);
        playerRb.MovePosition(newPos);

        if (Vector2.Distance(playerRb.position, bobberPoint) < 0.1f)
            StopReelback();
    }

    private void StartPullEnemy(Rigidbody2D enemyRb)
    {
        if (enemyRb == null) return;
        pulledEnemy = enemyRb;
        IsPullingEnemy = true;

        lr.enabled = true;
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, pulledEnemy.position);
    }

    public void StopPullEnemy()
    {
        IsPullingEnemy = false;
        pulledEnemy = null;
        lr.enabled = false;
    }

    private void PullEnemyToPlayer()
    {
        if (pulledEnemy == null)
        {
            StopPullEnemy();
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(pulledEnemy.position, playerRb.position, ReelbackSpeed * Time.deltaTime);
        pulledEnemy.MovePosition(newPos);

        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, pulledEnemy.position);

        if (Vector2.Distance(pulledEnemy.position, playerRb.position) < 0.5f)
            StopPullEnemy();
    }

    private void UpdateReelLine()
    {
        if (IsGrappling)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, bobberPoint);
        }
        else if (IsPullingEnemy && pulledEnemy != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, pulledEnemy.position);
        }
    }

    public Rigidbody2D GetPulledEnemy()
    {
        return pulledEnemy;
    }
}
