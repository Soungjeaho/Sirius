using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Move")]
        public float moveSpeed = 5f;

        [Header("Jump")]
        public Transform groundCheck;
        public float groundRadius = 0.1f;
        public LayerMask groundLayers;
        public float jumpForce = 7f;

        private Rigidbody2D rb;
        private bool isGrounded;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            // --- 좌우 이동 (A/D or ←/→)
            float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
            rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

            // --- 바라보는 방향에 따라 Flip
            if (h != 0)
            {
                var sr = GetComponentInChildren<SpriteRenderer>();
                if (sr)
                    sr.flipX = (h < 0);
            }

            // --- 땅 체크
            if (groundCheck)
            {
                isGrounded = Physics2D.OverlapCircle(
                    groundCheck.position,
                    groundRadius,
                    groundLayers
                );
            }
            else
            {
                isGrounded = true;
            }

            // --- 점프 (Space)
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
}
