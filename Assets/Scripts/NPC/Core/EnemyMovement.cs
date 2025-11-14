using UnityEngine;

namespace Project.NPC
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMovement : MonoBehaviour
    {
        public float maxFall = -20f;
        private Rigidbody2D rb;
        void Awake() { rb = GetComponent<Rigidbody2D>(); }
        public void RunTowards(Vector2 target, float speed)
        {
            Vector2 dir = (target - (Vector2)transform.position); dir.y = 0;
            rb.velocity = new Vector2(Mathf.Sign(dir.x) * speed, Mathf.Max(rb.velocity.y, maxFall));
        }
        public void HaltX() { rb.velocity = new Vector2(0, rb.velocity.y); }
    }
}
