using UnityEngine;
using System.Collections;

namespace ProjectConductor
{
    public class PlayerAbility_Throw : MonoBehaviour
    {
        [Header("던지기 각도 및 힘 (Inspector에서 설정)")]
        [Range(0, 90)] public float upThrowAngle = 80f;      // 위로 던지기 각도
        [Range(0, 90)] public float forwardThrowAngle = 35f; // 옆으로 던지기 각도
        public float throwPower = 15f;                       // 던지는 총 세기

        [Header("감지 설정")]
        [SerializeField] private Transform headPos;
        [SerializeField] private float detectionRadius = 0.6f;
        [SerializeField] private LayerMask playerLayer;

        private PlayerController controller;
        private bool isCharging = false;

        void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        void Update()
        {
            // P1(남캐)이 지면에 있을 때만 실행
            if (controller.playerID != 1 || !controller.isGrounded) return;

            HandleThrowInput();
        }

        private void HandleThrowInput()
        {
            if (Input.GetKey(controller.interactionKey))
            {
                Collider2D p2Collider = Physics2D.OverlapCircle(headPos.position, detectionRadius, playerLayer);

                if (p2Collider != null && p2Collider.CompareTag("Player2"))
                {
                    isCharging = true;
                    controller.SetMoveLock(true); // P1 이동 봉쇄

                    // 실시간 방향 전환
                    float horizontalInput = Input.GetAxisRaw($"Horizontal_P{controller.playerID}");
                    if (horizontalInput != 0)
                    {
                        controller.UpdateFacingDirection(horizontalInput);
                    }

                    // 던지기 실행
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        ExecuteThrow(p2Collider.gameObject, upThrowAngle);
                    }
                    else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                    {
                        ExecuteThrow(p2Collider.gameObject, forwardThrowAngle);
                    }
                }
            }

            if (Input.GetKeyUp(controller.interactionKey))
            {
                isCharging = false;
                controller.SetMoveLock(false);
            }
        }

        private void ExecuteThrow(GameObject p2, float angle)
        {
            Rigidbody2D p2Rb = p2.GetComponent<Rigidbody2D>();
            PlayerController p2Controller = p2.GetComponent<PlayerController>();

            if (p2Controller != null) p2Controller.SetMoveLock(true);

            p2Rb.velocity = Vector2.zero;

            // [중요] 위치 보정: P1과 겹치지 않게 충분히 띄움
            p2.transform.position += new Vector3(0, 0.4f, 0);

            float radian = angle * Mathf.Deg2Rad;
            float fDir = controller.facingDirection;
            Vector2 throwDir = new Vector2(Mathf.Cos(radian) * fDir, Mathf.Sin(radian));

            p2Rb.AddForce(throwDir * throwPower, ForceMode2D.Impulse);

            // 정리된 착지 감지 코루틴 시작
            StartCoroutine(UnlockP2OnLanding(p2Controller));
        }

        private IEnumerator UnlockP2OnLanding(PlayerController p2Controller)
        {
            if (p2Controller == null) yield break;

            Rigidbody2D p2Rb = p2Controller.GetComponent<Rigidbody2D>();

            // 1단계: 강제 대기 (발사 직후 즉시 착지 판정 방지)
            // 이 시간 동안은 포물선이 무조건 보장됩니다.
            yield return new WaitForSeconds(0.2f);

            // 2단계: 다시 땅에 닿을 때까지 대기
            // p2Controller.isGrounded가 다시 true가 될 때까지 포물선 이동 유지
            while (!p2Controller.isGrounded)
            {
                yield return null;
            }

            // 3단계: 착지 시 수평 속도 리셋 (미끄러짐 방지)
            if (p2Rb != null)
            {
                p2Rb.velocity = new Vector2(0, p2Rb.velocity.y);
            }

            // 4단계: 제어권 복구
            p2Controller.SetMoveLock(false);

            Debug.Log("<color=green>P2 안전 착지: 제어권 복구 완료</color>");
        }

        private void OnDrawGizmos()
        {
            if (headPos == null || controller == null) return;

            Gizmos.color = Color.green;
            float fDir = controller.facingDirection;

            float forwardRad = forwardThrowAngle * Mathf.Deg2Rad;
            Vector3 forwardVec = new Vector3(Mathf.Cos(forwardRad) * fDir, Mathf.Sin(forwardRad), 0);
            Gizmos.DrawRay(headPos.position, forwardVec * 2f);

            float upRad = upThrowAngle * Mathf.Deg2Rad;
            Vector3 upVec = new Vector3(Mathf.Cos(upRad) * fDir, Mathf.Sin(upRad), 0);
            Gizmos.DrawRay(headPos.position, upVec * 2f);
        }
    }
}