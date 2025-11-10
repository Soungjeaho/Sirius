// IHeavyHitReceiver.cs
using UnityEngine;

public struct HeavyHitContext
{
    public Vector2 hitPoint;
    public Vector2 hitDirection;   // 찌 → 충돌체 방향(정규화)
    public float knockback;        // 필요 시 쓰라고 둠
    public GameObject projectile;  // 맞힌 찌 객체(파편 이펙트 등에서 사용)
}

public interface IHeavyHitReceiver
{
    void OnHeavyHit(HeavyHitContext ctx);
}
