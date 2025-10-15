using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reelback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lr;

    [Header("Grapple Settings")]
    [SerializeField] public float ReelbackSpeed = 15f;
    [SerializeField] private float maxReelDistance = 10f;
    [SerializeField] private LayerMask ReelbackLayer;

    private Rigidbody2D playerRb;
    private Vector2 bobberPoint;
    private Rigidbody2D pulledEnemy;

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

    public void HandleReekback()
    {
        if (Input.GetMouseButtonDown(1))
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
                if (hit.CompareTag("Reelbackable"))
                    StartReelback(mouseWorld);
                else if (hit.CompareTag("Enemy"))
                    StartPullEnemy(hit.GetComponent<Rigidbody2D>());
            }
        }

        if (IsGrappling)
            MoveToGrapplePoint();
        else if (IsPullingEnemy)
            PullEnemyToPlayer();

        if (Input.GetMouseButtonUp(1))
        {
            StopReelback();
            StopPullEnemy();
        }

        UpdateReelLine();
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
