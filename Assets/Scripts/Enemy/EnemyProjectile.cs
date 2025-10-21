using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float verticalBoost = 0.5f;   // 위로 쏘는 정도 조절용

    private Vector2 startPoint;
    private Vector2 targetDirection;
    private float timer = 0f;

    public void Launch(Vector2 direction)
    {
        startPoint = transform.position;

        // 방향에 위쪽 성분 추가 후 정규화
        targetDirection = (direction + Vector2.up * verticalBoost).normalized;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timer += Time.deltaTime * speed;

        // 포물선 형태의 이동 (y축에 곡선 추가)
        Vector2 nextPos = (Vector2)transform.position + targetDirection * Time.deltaTime * speed;
        nextPos.y += Mathf.Sin(timer) * arcHeight * Time.deltaTime;

        transform.position = nextPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 피격!");
            Destroy(gameObject);
        }
    }
}
