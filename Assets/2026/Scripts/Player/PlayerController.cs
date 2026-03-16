using UnityEngine;

namespace ProjectConductor
{
    public class PlayerController : MonoBehaviour
    {
        [Header("플레이어 설정")]
        public int playerID = 1;
        public float moveSpeed = 7f;
        public float jumpForce = 12f;
        public float boostMultiplier = 1.5f;

        [Header("지면 체크")]
        public Transform groundCheck;
        public float checkRadius = 0.2f;
        public LayerMask groundLayer;
        public bool isGrounded;

        [Header("방향 및 위치 설정")]
        public Transform facingDirectionObject;

        public float facingDirection { get; private set; } = 1f;

        private Rigidbody2D rb;
        private float moveInput;
        private KeyCode jumpKey;
        public KeyCode interactionKey;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // 캐릭터 Tag 설정 필수 (가이드 사항)
            if (playerID == 1) gameObject.tag = "Player1";
            else if (playerID == 2) gameObject.tag = "Player2";

            // ID별 조작키 할당
            if (playerID == 1)
            {
                jumpKey = KeyCode.W;
            }
            else
            {
                jumpKey = KeyCode.UpArrow;
            }
        }

        void Update()
        {
            moveInput = Input.GetAxisRaw($"Horizontal_P{playerID}");

            // 방향 전환 및 빈 오브젝트 위치 조정
            if (moveInput > 0)
            {
                facingDirection = 1f;
                // 빈 오브젝트의 로컬 위치를 캐릭터 앞쪽으로 유지
                facingDirectionObject.localPosition = new Vector3(Mathf.Abs(facingDirectionObject.localPosition.x), facingDirectionObject.localPosition.y, 0);
            }
            else if (moveInput < 0)
            {
                facingDirection = -1f;
                facingDirectionObject.localPosition = new Vector3(-Mathf.Abs(facingDirectionObject.localPosition.x), facingDirectionObject.localPosition.y, 0);
            }
            // 1. OverlapCircleAll을 사용하여 범위 내 모든 콜라이더를 가져옴
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, checkRadius, groundLayer);

            isGrounded = false;
            Collider2D steppedObject = null;

            foreach (var hit in hits)
            {
                // 2. 자기 자신(P2)은 무시하고 다른 물체(지면, P1)만 체크
                if (hit.gameObject != gameObject)
                {
                    isGrounded = true;
                    steppedObject = hit;
                    break;
                }
            }

            // 3. 점프 로직
            if (Input.GetKeyDown(jumpKey) && isGrounded)
            {
                float finalJumpForce = jumpForce;

                if (playerID == 2 && steppedObject != null && steppedObject.CompareTag("Player1"))
                {
                    finalJumpForce *= boostMultiplier;
                    Debug.Log("<color=cyan>고점프 발동!</color>");
                }

                // AddForce 대신 velocity 직접 대입으로 변경 (일관된 점프 높이 보장)
                rb.velocity = new Vector2(rb.velocity.x, finalJumpForce);
            }

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