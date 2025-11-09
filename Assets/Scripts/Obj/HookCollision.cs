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

        //  적 충돌
        if (collision.CompareTag("Enemy"))
        {
            reelback.OnHookHit("Enemy", hitPos);
        }
        //  당길 수 있는 오브젝트
        else if (collision.CompareTag("Reelbackable"))
        {
            reelback.OnHookHit("Reelbackable", hitPos);
        }
        //  릴백용 벽 (매달리기)
        else if (collision.CompareTag("RB_Wall"))
        {
            reelback.OnHookHit("RB_Wall", hitPos);
        }
        //  일반 지면 / 장애물
        else if (collision.CompareTag("Ground") || collision.CompareTag("Obstacle"))
        {
            // Hook 삭제
            Destroy(gameObject);

            // 라인렌더러 정리
            if (reelback.lr != null)
            {
                reelback.lr.enabled = false;
                reelback.lr.positionCount = 0;
            }
        }
        //  나머지 경우 — Hook만 제거
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
