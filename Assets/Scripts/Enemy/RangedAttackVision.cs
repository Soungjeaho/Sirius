using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttackVision : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackDelay = 2.5f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    private Transform playerInSight = null;
    private float attackTimer = 0f;

    [Header("Vision Settings")]
    [SerializeField] private PolygonCollider2D visionCollider; // 자식 오브젝트 Trigger

    void Update()
    {
        // 플레이어가 감지되어 있으면 공격
        if (playerInSight != null)
        {
            float distance = Vector2.Distance(transform.position, playerInSight.position);
            if (distance <= attackRange && attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackDelay;
            }

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }
    }

    void Attack()
    {
        if (playerInSight == null)
            return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Vector2 direction = (playerInSight.position - firePoint.position).normalized;

        EnemyProjectile proj = projectile.GetComponent<EnemyProjectile>();
        if (proj != null)
            proj.Launch(direction);
    }

    // --- 시야 Trigger 감지 ---
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = null;
        }
    }

    // --- Scene 시각화 ---
    private void OnDrawGizmosSelected()
    {
        if (visionCollider == null)
            return;

        Gizmos.color = Color.yellow;
        Vector2[] points = visionCollider.points;
        Vector3 prev = transform.TransformPoint(points[0]);
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 curr = transform.TransformPoint(points[i]);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
        Gizmos.DrawLine(prev, transform.TransformPoint(points[0]));
    }
}
