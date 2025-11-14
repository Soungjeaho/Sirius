using UnityEngine;
using Project.Combat;

namespace Project.NPC
{
    public class Bomb : Projectile2D
    {
        public float explodeRadius = 1.4f;
        public float fuse = 1.2f;
        private bool lit;

        protected override void OnEnable()
        {
            base.OnEnable();
            lit = false;
            CancelInvoke();
            Invoke(nameof(Explode), fuse);
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            // Use fuse timing to explode; ignore immediate collisions
        }

        void Explode()
        {
            if (lit) return;
            lit = true;
            var arr = Physics2D.OverlapCircleAll(transform.position, explodeRadius, hitLayers);
            foreach (var c in arr)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d != null && !d.IsDead) d.ApplyDamage(damage, transform.position, Vector2.zero, this);
            }
            gameObject.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explodeRadius);
        }
    }
}
