using UnityEngine;
using System.Collections;

namespace ProjectConductor
{
    public class RotatingCurrent : MonoBehaviour
    {
        [Header("시스템 연동")]
        public bool isConnected = false;

        private bool isRotating = false; // 회전 중인지 확인하는 변수

        // 플레이어 컨트롤러에서 호출할 함수
        public void OnInteracted(int playerID)
        {
            // 이미 회전 중이라면 입력을 무시
            if (isRotating) return;

            Debug.Log(gameObject.name + "이(가) Player " + playerID + "에 의해 회전합니다.");

            // 코루틴 실행 (0.5초 동안 회전)
            StartCoroutine(SmoothRotate(0.5f));

            // 상호작용 시 연결 상태 true (가이드 준수)
            isConnected = true;
        }

        private IEnumerator SmoothRotate(float duration)
        {
            isRotating = true;

            float elapsed = 0f;
            Quaternion startRotation = transform.rotation;
            // 현재 회전값에 Z축 90도를 더함
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 0, 90f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 구버전에서도 안정적인 Slerp(구면 선형 보간) 사용
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);

                yield return null;
            }

            // 회전 종료 후 정확한 각도로 고정
            transform.rotation = targetRotation;
            isRotating = false;
        }
    }
}