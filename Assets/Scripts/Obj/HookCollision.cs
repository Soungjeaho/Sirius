using UnityEngine;

public class HookCollision : MonoBehaviour
{
    private NewReelback reelback;
    private bool hasHit = false;

    public void Init(NewReelback rb)
    {
        reelback = rb;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || reelback == null)
            return;

        hasHit = true;

        Vector2 hitPos = collision.ClosestPoint(transform.position);

        //  1. Enemy 충돌
        if (collision.CompareTag("Enemy"))
        {
            //  EnemyGrapple을 찾아서 StartGrapple 실행
            EnemyGrapple enemyGrapple = reelback.GetComponent<EnemyGrapple>();
            if (enemyGrapple != null)
            {
                enemyGrapple.StartGrapple(collision.gameObject);
                Debug.Log($"[HookCollision] Enemy 감지 → Grapple 시작: {collision.name}");
            }
            else
            {
                Debug.LogWarning("[HookCollision] EnemyGrapple 컴포넌트를 찾을 수 없습니다. Player에 붙어 있는지 확인하세요!");
            }

            // 라인 연결용 위치 전달
            reelback.OnHookHit("Enemy", hitPos);

            // Hook은 제거하되, Grapple이 진행 중이므로 라인은 유지
            Destroy(gameObject, 0.05f);
            return;
        }

        //  2. 당길 수 있는 오브젝트
        else if (collision.CompareTag("Reelbackable"))
        {
            reelback.OnHookHit("Reelbackable", hitPos);
            Destroy(gameObject, 0.05f);
            return;
        }

        //  3. 릴백용 벽 (매달리기)
        else if (collision.CompareTag("RB_Wall"))
        {
            reelback.OnHookHit("RB_Wall", hitPos);
            Destroy(gameObject, 0.05f);
            return;
        }

        //  4. 일반 지면 / 장애물
        else if (collision.CompareTag("Ground") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);

            if (reelback.lr != null)
            {
                reelback.lr.enabled = false;
                reelback.lr.positionCount = 0;
            }

            return;
        }

        //  5. 그 외 충돌 (기타 오브젝트)
        else
        {
            Destroy(gameObject);
            if (reelback.lr != null)
            {
                reelback.lr.enabled = false;
                reelback.lr.positionCount = 0;
            }
        }
    }
}
