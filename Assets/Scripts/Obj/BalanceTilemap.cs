using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Tilemap))]
public class BalanceTilemap : MonoBehaviour
{
    [Header("기울기 설정")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltDuration = 0.25f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float returnDuration = 0.3f;

    private Quaternion baseRot;
    private Coroutine tiltRoutine;
    private Rigidbody2D rb;

    private void Awake()
    {
        baseRot = transform.rotation;
        rb = GetComponent<Rigidbody2D>();
    }

    // HeavyFloatProjectile에서 호출됨
    public void OnHeavyHit(Vector2 hitDir, Vector2 hitPoint)
    {
        // 이미 회전 중이면 중복 실행 방지
        if (tiltRoutine != null)
            return;

        // 좌/우 충돌 방향만 기준으로 기울이기
        float tiltSign = Mathf.Sign(hitDir.x);  //  핵심 수정
        float targetZ = tiltAngle * tiltSign;

        tiltRoutine = StartCoroutine(TiltPlatform(targetZ));
    }

    private IEnumerator TiltPlatform(float targetZ)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, targetZ);
        float t = 0f;

        // 1️⃣ 기울이기
        while (t < tiltDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / tiltDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, u);
            yield return null;
        }

        transform.rotation = endRot;
        yield return new WaitForSeconds(holdTime);

        // 2️⃣ 복귀
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / returnDuration);
            transform.rotation = Quaternion.Slerp(endRot, baseRot, u);
            yield return null;
        }

        transform.rotation = baseRot;
        tiltRoutine = null;
    }

    private void LateUpdate()
    {
        // Collider 실시간 업데이트 강제
        CompositeCollider2D comp = GetComponent<CompositeCollider2D>();
        if (comp != null)
        {
            comp.GenerateGeometry(); // Unity 2022+ 기준
        }
    }
}
