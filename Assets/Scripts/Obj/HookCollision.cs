using UnityEngine;

public class HookCollision : MonoBehaviour
{
    private Reelback owner;

    public void Init(Reelback reelback)
    {
        owner = reelback;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[HookCollision] Collision with {collision.gameObject.name}");

        if (owner == null)
        {
            return;
        }

        Vector2 hitPos = collision.GetContact(0).point;
        owner.OnHookHit(collision.gameObject, hitPos);

        // 한 번 맞으면 Hook 정지
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[HookCollision] Trigger with {other.gameObject.name}");

        if (owner == null)
        {
            return;
        }

        Vector2 hitPos = other.ClosestPoint(transform.position);
        owner.OnHookHit(other.gameObject, hitPos);

        Destroy(gameObject);
    }
}
