using UnityEngine;

namespace Project.Combat
{
    public class DamageDealer2D : MonoBehaviour
    {
        public int damage = 1;
        public float radius = 0.6f;
        public LayerMask targetLayers;
        public Transform origin;

        public int DealOnce()
        {
            if (!origin) origin = transform;
            int hits = 0;
            var arr = Physics2D.OverlapCircleAll(origin.position, radius, targetLayers);
            foreach (var c in arr)
            {
                var dmg = c.GetComponentInParent<IDamageable>();
                if (dmg != null && !dmg.IsDead)
                {
                    Vector2 normal = Vector2.right * (transform.localScale.x >= 0 ? 1 : -1);
                    dmg.ApplyDamage(damage, origin.position, normal, this);
                    hits++;
                }
            }
            return hits;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            var o = origin ? origin : transform;
            Gizmos.DrawWireSphere(o.position, radius);
        }
    }
}
