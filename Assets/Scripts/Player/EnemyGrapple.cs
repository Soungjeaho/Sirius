// EnemyGrapple.cs
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

        if (!gauge.UseGauge(gaugeCost))
        {
            Debug.Log("Energy 부족!");
            return;
        }

        // Enemy 멈추기
        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        if (em != null)
            em.moveSpeed = 0f;

        isPullingEnemy = true;
        reelback.isEnemyBeingGrappled = true; // LR을 NewReelback에서 건드리지 않도록
        StartCoroutine(PullEnemyCoroutine(enemy));
    }

    private IEnumerator PullEnemyCoroutine(GameObject enemy)
    {
        Transform enemyTransform = enemy.transform;
        Transform playerTransform = reelback.FirePoint;

        // LineRenderer 켜기
        if (reelback.lr != null)
        {
            reelback.lr.enabled = true;
            reelback.lr.positionCount = 2;
        }

        while (Vector2.Distance(enemyTransform.position, playerTransform.position) > 0.1f)
        {
            enemyTransform.position = Vector2.MoveTowards(
                enemyTransform.position,
                playerTransform.position,
                grappleSpeed * Time.deltaTime
            );

            // LR 갱신
            if (reelback.lr != null)
            {
                reelback.lr.SetPosition(0, playerTransform.position);
                reelback.lr.SetPosition(1, enemyTransform.position);
            }

            yield return null;
        }

        // Player 도착 후 0.5초 대기
        yield return new WaitForSeconds(disappearDelay);
        Destroy(enemy);

        // LR 완전 초기화
        if (reelback.lr != null)
        {
            reelback.lr.enabled = false;
            reelback.lr.positionCount = 0;
        }

        reelback.isEnemyBeingGrappled = false;
        isPullingEnemy = false;
    }
}
