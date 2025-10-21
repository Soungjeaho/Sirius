using System.Collections;
using UnityEngine;

public class EnemyGrapple : MonoBehaviour
{
    [SerializeField] private NewReelback reelback;
    [SerializeField] private EnergyGauge gauge;
    [SerializeField] private float grappleSpeed = 5f;
    [SerializeField] private float disappearDelay = 0.5f;
    [SerializeField] private int gaugeCost = 4;

    private bool isPullingEnemy = false;

    public void StartGrapple(GameObject enemy)
    {
        if (isPullingEnemy || enemy == null) return;

        // 에너지 확인
        if (!gauge.UseGauge(gaugeCost))
        {
            Debug.Log("Energy 부족!");
            return;
        }

        // Enemy의 행동 멈추기 (모든 움직임 관련 스크립트 비활성화)
        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        if (em != null)
            em.enabled = false;

        RangedAttackVision ra = enemy.GetComponent<RangedAttackVision>();
        if (ra != null)
            ra.enabled = false;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;  // 물리 속도 제거
            rb.isKinematic = true;       // 물리 반응 정지
        }

        isPullingEnemy = true;
        reelback.isEnemyBeingGrappled = true; // NewReelback에서 LR 영향 방지

        StartCoroutine(PullEnemyCoroutine(enemy));
    }

    private IEnumerator PullEnemyCoroutine(GameObject enemy)
    {
        Transform enemyTransform = enemy.transform;
        Transform playerTransform = reelback.FirePoint;

        if (reelback.lr != null)
        {
            reelback.lr.enabled = true;
            reelback.lr.positionCount = 2;
        }

        while (!HasReachedFirePoint(enemyTransform, playerTransform))
        {
            enemyTransform.position = Vector2.MoveTowards(enemyTransform.position, playerTransform.position, grappleSpeed * Time.deltaTime);

            if (reelback.lr != null)
            {
                reelback.lr.positionCount = 2; // 항상 2
                reelback.lr.SetPosition(0, playerTransform.position);
                reelback.lr.SetPosition(1, enemyTransform.position);
            }

            yield return null;
        }

        yield return new WaitForSeconds(disappearDelay);
        Destroy(enemy);

        gauge.AddGauge(1);

        if (reelback.lr != null)
        {
            reelback.lr.enabled = false;
            reelback.lr.positionCount = 0;
        }

        reelback.isEnemyBeingGrappled = false;
        isPullingEnemy = false;
    }


    private bool HasReachedFirePoint(Transform enemy, Transform firePoint)
    {
        // Enemy의 Collider가 FirePoint 주변에 닿으면 true
        float distance = Vector2.Distance(enemy.position, firePoint.position);
        return distance <= 0.3f; // 닿았다고 판단할 거리 (필요시 조정)
    }
}
