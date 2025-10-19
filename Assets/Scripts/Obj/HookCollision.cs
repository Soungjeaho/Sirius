using UnityEngine;

public class HookCollision : MonoBehaviour
{
    private NewReelback reelback;

    public void Init(NewReelback rb)
    {
        reelback = rb;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (reelback == null) return;

        Vector2 hitPos = collision.ClosestPoint(transform.position); // 충돌 지점 계산

        if (collision.CompareTag("Enemy"))
        {
            reelback.OnHookHit(collision.tag, hitPos);
        }
        else if (collision.CompareTag("Reelbackable"))
        {
            reelback.OnHookHit(collision.tag, collision.ClosestPoint(transform.position));
        }
        else if (collision.CompareTag("RB_Wall"))
        {
            reelback.OnHookHit(collision.tag, hitPos); // 태그 + 위치 전달
        }
    }
}
