using UnityEngine;
using System.Collections;

public class JavelinBomberController : BaseMonster
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;       // Inspector에서 Bomb Prefab 할당
    public Transform dropPoint;         // Inspector에서 폭탄 떨어뜨릴 위치 지정
    public float jumpHeight = 3f;       // 점프 높이
    public float jumpDuration = 0.5f;   // 점프 시간
    public float bombDelay = 0.1f;      // 폭탄 떨어뜨리는 딜레이
    public float BombattackDelay = 2f;

    private float lastAttackTime = 0f;

    protected override void Attack()
    {
        if (bombPrefab == null || dropPoint == null)
        {
            Debug.LogWarning("Bomb Prefab 또는 DropPoint가 할당되지 않았습니다!");
            return;
        }

        // 콘솔 출력
        Debug.Log($"{name} 점프 공격 준비!");

        StartCoroutine(JumpAndDropBomb());
    }

    IEnumerator JumpAndDropBomb()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = new Vector2(player.position.x, player.position.y);

        float elapsed = 0f;

        // 점프 이동
        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight; // 포물선
            transform.position = Vector2.Lerp(startPos, endPos, t) + Vector2.up * height;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos; // 정확히 착지

        yield return new WaitForSeconds(bombDelay);

        // 폭탄 생성
        if (bombPrefab != null && dropPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
            if (bomb != null)
            {
                Debug.Log($"{name} 폭탄 투하!");
            }
        }

        lastAttackTime = Time.time;
    }

    protected override void MoveTowardsPlayer()
    {
        // 점프 공격 중에는 이동 금지
        if (Time.time - lastAttackTime < BombattackDelay) return;

        base.MoveTowardsPlayer();
    }
}
