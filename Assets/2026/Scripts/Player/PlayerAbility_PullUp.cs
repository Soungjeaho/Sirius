using UnityEngine;
using System.Collections;

namespace ProjectConductor
{
    public class PlayerAbility_PullUp : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private float pullRange = 2.5f;    // P1을 감지할 아래쪽 거리
        [SerializeField] private float pullSpeed = 5f;      // 끌어올리는 속도
        [SerializeField] private LayerMask playerLayer;    // P1 레이어 (Player)

        private PlayerController controller;
        private Rigidbody2D rb;
        private bool isPulling = false;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            // 여캐(P2)가 지면에 있고, 아래 방향키 + 상호작용 키를 누를 때 실행 
            if (controller.playerID == 2 && controller.isGrounded && !isPulling)
            {
                if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(controller.interactionKey))
                {
                    CheckAndPull();
                }
            }
        }

        private void CheckAndPull()
        {
            // 1. PlayerController에 등록된 빈 오브젝트(facingDirectionObject)의 위치를 가져옴
            if (controller.facingDirectionObject == null)
            {
                Debug.LogWarning("PlayerController에 FacingDirectionObject가 연결되지 않았습니다.");
                return;
            }

            // 레이캐스트 시작점: 빈 오브젝트의 현재 월드 위치
            Vector3 rayStartPos = controller.facingDirectionObject.position;

            // 2. 해당 위치에서 아래로 레이를 발사
            RaycastHit2D hit = Physics2D.Raycast(rayStartPos, Vector2.down, pullRange, playerLayer);

            // 디버그용: 레이가 실제로 어디서 나가는지 확인
            Debug.DrawRay(rayStartPos, Vector2.down * pullRange, Color.red, 0.5f);

            if (hit.collider != null && hit.collider.CompareTag("Player1"))
            {
                // P1이 P2의 약간 뒤나 옆에 서도록 설정 (겹침 방지)
                // P2 위치에서 facingDirection 반대 방향으로 0.6f만큼 떨어진 곳
                float offset = controller.facingDirection * -0.6f;
                Vector3 finalTargetPos = new Vector3(transform.position.x + offset, transform.position.y, 0);

                StartCoroutine(PullRoutine(hit.collider.gameObject, finalTargetPos));
            }
        }

        private IEnumerator PullRoutine(GameObject targetPlayer, Vector3 destination)
        {
            isPulling = true;
            Rigidbody2D targetRb = targetPlayer.GetComponent<Rigidbody2D>();
            Collider2D targetCollider = targetPlayer.GetComponent<Collider2D>();
            Collider2D p2Collider = GetComponent<Collider2D>();

            // 1. 상승 시작 시 P1과 P2의 충돌을 잠시 무시 (겹침 방지)
            Physics2D.IgnoreCollision(targetCollider, p2Collider, true);
            targetRb.isKinematic = true;
            targetRb.velocity = Vector2.zero;

            Vector3 startPos = targetPlayer.transform.position;
            // P2의 머리 위가 아니라 가슴~어깨 높이 정도로 곡선 정점 조절
            Vector3 controlPoint = new Vector3(startPos.x, destination.y + 0.8f, 0);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * (pullSpeed / 5f);

                // 2. 절반 이상(P2 발 근처) 올라왔을 때 충돌 감지 재활성화
                if (t > 0.7f)
                {
                    Physics2D.IgnoreCollision(targetCollider, p2Collider, false);
                }

                // 3. 거의 다 올라왔을 때(약 90%) 물리 엔진을 다시 켬
                if (t > 0.9f && targetRb.isKinematic)
                {
                    targetRb.isKinematic = false;
                    // P2 방향으로 살짝 밀어넣는 초기 속도 부여
                    float pushDir = (destination.x - targetPlayer.transform.position.x);
                    targetRb.velocity = new Vector2(pushDir * 2f, 2f);
                }

                if (targetRb.isKinematic)
                {
                    Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
                    Vector3 m2 = Vector3.Lerp(controlPoint, destination, t);
                    targetPlayer.transform.position = Vector3.Lerp(m1, m2, t);
                }

                yield return null;
            }

            // 최종 정리
            targetRb.isKinematic = false;
            Physics2D.IgnoreCollision(targetCollider, p2Collider, false);

            // 도착 후 P2를 살짝 밀어내기 (P2가 미끄러지듯 뒤로 감)
            Vector2 pushForce = new Vector2(controller.facingDirection * -1.5f, 0);
            rb.AddForce(pushForce, ForceMode2D.Impulse);

            isPulling = false;
        }
        // 에디터 뷰에서 감지 범위를 시각적으로 확인
        private void OnDrawGizmosSelected()
        {
            if (controller != null && controller.facingDirectionObject != null)
            {
                Gizmos.color = Color.blue;
                // 빈 오브젝트 위치에서 아래로 파란 선을 그림
                Gizmos.DrawRay(controller.facingDirectionObject.position, Vector2.down * pullRange);
                // 안착 지점을 작은 구체로 표시
                Gizmos.DrawWireSphere(new Vector3(controller.facingDirectionObject.position.x, transform.position.y, 0), 0.2f);
            }
        }
    }
}