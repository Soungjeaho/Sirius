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
        if (isPullingEnemy || enemy == null)
            return;

        if (!gauge.UseGauge(gaugeCost))
        {
            Debug.Log("Energy 부족!");
            return;
        }

        EnemyMovement em = enemy.GetComponent<EnemyMovement>();
        if (em != null) em.enabled = false;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        isPullingEnemy = true;
        reelback.isEnemyBeingGrappled = true;

        StartCoroutine(PullEnemyCoroutine(enemy));
    }

    private IEnumerator PullEnemyCoroutine(GameObject enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        Transform playerTransform = reelback.FirePoint;

        if (reelback.lr != null)
        {
            reelback.lr.enabled = true;
            reelback.lr.positionCount = 2;
        }

        if (enemyRb != null)
        {
            enemyRb.isKinematic = false;
            enemyRb.gravityScale = 0f;
            enemyRb.velocity = Vector2.zero;
        }

        while (!HasTouchedPlayer(enemy.transform))
        {
            if (enemyRb != null)
            {
                // 플레이어와 Enemy의 거리 방향 벡터 계산
                Vector2 dir = ((Vector2)playerTransform.position - enemyRb.position).normalized;
                enemyRb.velocity = dir * grappleSpeed;  // 속도를 직접 적용
            }
            else
            {
                enemy.transform.position = Vector2.MoveTowards(
                    enemy.transform.position,
                    playerTransform.position,
                    grappleSpeed * Time.deltaTime
                );
            }

            if (reelback.lr != null)
            {
                reelback.lr.SetPosition(0, playerTransform.position);
                reelback.lr.SetPosition(1, enemy.transform.position);
            }

            yield return new WaitForFixedUpdate();
        }

        // 닿은 후 잠시 유지
        yield return new WaitForSeconds(disappearDelay);

        gauge.AddGauge(1);
        Destroy(enemy);

        // Grapple 종료 처리
        if (reelback.lr != null)
        {
            reelback.lr.enabled = false;
            reelback.lr.positionCount = 0;
        }

        reelback.ResetHookState();
        reelback.StopAllCoroutines();

        reelback.isEnemyBeingGrappled = false;
        isPullingEnemy = false;
    }

    private bool HasTouchedPlayer(Transform enemy)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return false;

        Collider2D enemyCol = enemy.GetComponent<Collider2D>();
        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (enemyCol == null || playerCol == null) return false;

        return enemyCol.IsTouching(playerCol);
    }
}
