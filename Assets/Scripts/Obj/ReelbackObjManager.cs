using UnityEngine;

public class ReelBackObjManager : MonoBehaviour
{
    [Header("관리 대상")]
    [SerializeField] private Transform block = null;   // 당겨지는 Block
    [SerializeField] private Transform volume = null;  // 목표 위치

    [Header("설정")]
    [SerializeField] private float reachThreshold = 0.1f; // Volume과 가까워지면 도달로 판단

    private bool tagChanged = false;

    private void Update()
    {
        if (tagChanged) return; // 이미 태그 변경 완료하면 더 이상 체크 안 함

        if (block == null || volume == null) return;

        // Block이 Volume 위치에 도달했는지 확인
        if (Vector2.Distance(block.position, volume.position) <= reachThreshold)
        {
            block.gameObject.tag = "Obstacle";
            // Collider를 static으로 변경(isTrigger 비활성화)
            Collider2D col = block.GetComponent<Collider2D>();

            if (col != null)
            {
                col.isTrigger = false;
            }

            // Rigidbody가 있다면 움직임 고정
            Rigidbody2D rb = block.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }
            tagChanged = true;
        }
    }
}
