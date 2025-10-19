using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewReelback : MonoBehaviour
{
    [SerializeField] GameObject HookPrefab = null;       // Inspector에서 Hook Prefab 연결
    [SerializeField] Transform FirePoint = null;         // Player 발사 위치
    [SerializeField] SpriteRenderer playerSprite = null; // Player 스프라이트
    [SerializeField] LineRenderer lr = null;            // LineRenderer 연결
    [SerializeField] float maxDistance = 10f;           // 훅 최대 거리

    Camera m_cam = null;
    bool facingRight = true;
    GameObject currentHook = null;

    private void Start()
    {
        m_cam = Camera.main;

        if (FirePoint == null)
            Debug.LogError("FirePoint가 할당되지 않았습니다!");
        if (playerSprite == null)
            Debug.LogError("Player Sprite가 할당되지 않았습니다!");
        if (lr == null)
            Debug.LogError("LineRenderer가 할당되지 않았습니다!");
        else
            lr.positionCount = 2; // 시작과 끝점 2개
    }

    private void LookAtMouse()
    {
        if (FirePoint == null) return;

        Vector2 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x && facingRight)
            Flip();
        else if (mousePos.x > transform.position.x && !facingRight)
            Flip();

        Vector2 direction = mousePos - (Vector2)FirePoint.position;
        FirePoint.right = direction;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        playerSprite.flipX = !playerSprite.flipX;

        Vector3 fpLocalPos = FirePoint.localPosition;
        fpLocalPos.x *= -1;
        FirePoint.localPosition = fpLocalPos;
    }
    private void TryFire()
    {
        if (FirePoint == null || HookPrefab == null) return;

        if (Input.GetMouseButtonDown(1) && currentHook == null)
        {
            currentHook = Instantiate(HookPrefab, FirePoint.position, FirePoint.rotation);

            Rigidbody2D rb = currentHook.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = FirePoint.right * 10f;
            }
        }
    }


    private void UpdateLine()
    {
        if (currentHook != null)
        {
            // Player와 훅 사이 거리 체크
            float distance = Vector2.Distance(FirePoint.position, currentHook.transform.position);
            if (distance > maxDistance)
            {
                Destroy(currentHook);
                currentHook = null;
                return;
            }

            lr.enabled = true;
            lr.SetPosition(0, FirePoint.position);
            lr.SetPosition(1, currentHook.transform.position);
        }
        else
        {
            lr.enabled = false;
        }
    }

    private void Update()
    {
        LookAtMouse();
        TryFire();
        UpdateLine();
    }
}
