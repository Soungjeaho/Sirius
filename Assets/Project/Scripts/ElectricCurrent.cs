using UnityEngine;
using System.Collections.Generic;

namespace ProjectConductor
{
    public class ElectricCurrent : MonoBehaviour
    {
        public enum CurrentType { A, B, C }
        public CurrentType type;
        public float moveSpeed;

        [Header("이탈 감지 설정")]
        public float detectionRadius = 3.0f;
        public float failDelay = 3.0f;
        private float currentFailTimer = 0f;

        [Header("시각적 피드백")]
        public SpriteRenderer spriteRenderer; // 전류의 SpriteRenderer 연결
        private Color originalColor;

        private List<WireSegment> pathSegments;
        private int currentTargetIndex = 0;
        private WireSegment lastSegment;
        private bool isInitialized = false;

        public void Initialize(CurrentType newType, List<WireSegment> segments)
        {
            type = newType;
            pathSegments = new List<WireSegment>(segments);

            switch (type)
            {
                case CurrentType.A: moveSpeed = 1.0f; break;
                case CurrentType.B: moveSpeed = 2.5f; break;
                case CurrentType.C: moveSpeed = 4.5f; break;
            }

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

            isInitialized = true;
        }

        void Update()
        {
            if (!isInitialized || pathSegments == null || currentTargetIndex >= pathSegments.Count) return;

            WireSegment targetSegment = pathSegments[currentTargetIndex];

            // 단절 구간 대기 로직
            if (targetSegment.isRequiredPlayer && !targetSegment.isConnected)
            {
                CheckPlayerDistance();
                return;
            }

            // 정상 주행 시 상태 초기화
            ResetFailureState();

            if (lastSegment != targetSegment)
            {
                if (lastSegment != null) lastSegment.ExitCurrent(this);
                targetSegment.EnterCurrent(this);
                lastSegment = targetSegment;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetSegment.transform.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetSegment.transform.position) < 0.01f)
            {
                currentTargetIndex++;
            }
        }

        private void CheckPlayerDistance()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            bool isAnyPlayerNear = false;

            foreach (GameObject p in players)
            {
                if (Vector3.Distance(transform.position, p.transform.position) <= detectionRadius)
                {
                    isAnyPlayerNear = true;
                    break;
                }
            }

            if (!isAnyPlayerNear)
            {
                currentFailTimer += Time.deltaTime;

                // 시각적 경고: 시간이 갈수록 빨간색으로 변함
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(originalColor, Color.red, currentFailTimer / failDelay);
                }

                if (currentFailTimer >= failDelay)
                {
                    OverloadManager.Instance.RestartStage();
                }
            }
            else
            {
                ResetFailureState();
            }
        }

        private void ResetFailureState()
        {
            currentFailTimer = 0f;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }

        public void RemoveFromLastSegment()
        {
            if (lastSegment != null)
            {
                lastSegment.ExitCurrent(this);
                lastSegment = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}