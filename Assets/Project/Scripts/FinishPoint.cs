using UnityEngine;

namespace ProjectConductor
{
    public class FinishPoint : MonoBehaviour
    {
        public int totalCurrentsToClear = 3;
        private int arrivedCount = 0;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ElectricCurrent current = collision.GetComponent<ElectricCurrent>();

            if (current != null)
            {
                // [수정] Destroy 전, 마지막으로 밟고 있던 세그먼트에서 자신을 제거해야 함
                // 그래야 OverloadManager가 개수를 줄여서 인지함
                current.RemoveFromLastSegment();

                arrivedCount++;
                Destroy(collision.gameObject);

                Debug.Log($"전류 도착! 현재: {arrivedCount} / 목표: {totalCurrentsToClear}");

                if (arrivedCount >= totalCurrentsToClear)
                {
                    Debug.Log("★ 스테이지 클리어! ★");
                    // 여기서 과부하 체크 중단을 명령할 수도 있음
                }
            }
        }
    }
}