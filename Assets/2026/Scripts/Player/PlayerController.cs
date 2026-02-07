using UnityEngine;

namespace ProjectConductor
{
    public class PlayerController : MonoBehaviour
    {
        [Header("플레이어 설정")]
        public int playerID = 1;
        public float moveSpeed = 7f;
        public float jumpForce = 12f;

        [Header("지면 체크")]
        public Transform groundCheck;
        public float checkRadius = 0.2f;
        public LayerMask groundLayer;

        private Rigidbody2D rb;
        private bool isGrounded;
        private float moveInput;
        private KeyCode interactionKey;
        private KeyCode jumpKey;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // 캐릭터 Tag 설정 필수 (가이드 사항)
            if (!CompareTag("Player")) gameObject.tag = "Player";

            // ID별 조작키 할당
            if (playerID == 1)
            {
                interactionKey = KeyCode.E;
                jumpKey = KeyCode.W;
            }
            else
            {
                interactionKey = KeyCode.L;
                jumpKey = KeyCode.UpArrow;
            }
        }

        void Update()
        {
            // 좌우 이동 입력 (Input Manager 설정에 따라 Horizontal_P1/P2 사용)
            moveInput = Input.GetAxisRaw($"Horizontal_P{playerID}");

            // 점프 로직
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            if (Input.GetKeyDown(jumpKey) && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }

            // 상호작용 시 이름 출력 테스트
            if (Input.GetKeyDown(interactionKey))
            {
                CheckInteraction();
            }
        }

        void FixedUpdate()
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        private void CheckInteraction()
        {
            // 반경 1.5 내의 오브젝트 감지
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // 회전 오브젝트 혹은 WireSegment가 있다면 상호작용 실행
                if (hit.TryGetComponent(out RotatingCurrent currentObj))
                {
                    currentObj.OnInteracted(playerID);
                }
            }
        }
    }
}