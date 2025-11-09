using UnityEngine;

namespace Project.Combat
{
    public abstract class Projectile2D : MonoBehaviour
    {
        public int damage = 1;
        public float speed = 8f;
        public float life = 5f;
        public LayerMask hitLayers;

        protected Vector2 dir;

        protected virtual void OnEnable()
        {
            CancelInvoke();
            Invoke(nameof(Despawn), life);
        }

        public void Fire(Vector2 origin, Vector2 direction)
        {
            transform.position = origin;
            dir = direction.normalized;
        }

        protected virtual void Update()
        {
            transform.position += (Vector3)(dir * speed * Time.deltaTime);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & hitLayers) == 0) return;
            var d = other.GetComponentInParent<IDamageable>();
            if (d != null) d.ApplyDamage(damage, transform.position, -dir, this);
            Despawn();
        }

        protected void Despawn()
        {
            gameObject.SetActive(false);
        }
    }
}
