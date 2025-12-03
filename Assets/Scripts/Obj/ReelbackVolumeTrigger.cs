using UnityEngine;

public class ReelbackVolumeTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 끌려오는 블록이 "Reelbackable" 태그일 때만 처리
        if (other.CompareTag("Reelbackable"))
        {
            // 태그 변경
            other.tag = "Obstacle";

            // Rigidbody2D를 Static으로 고정하고 싶다면
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }

            Debug.Log("Block이 Volume에 도달: 태그를 Obstacle로 변경 + Static으로 고정");
        }
    }
}
