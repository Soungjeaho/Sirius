using UnityEngine;
using System.Collections;

public class BalancePlatform : MonoBehaviour
{
    [Header("기울기 설정")]
    [SerializeField] private float tiltAngle = 15f;     // 최대 회전 각도
    [SerializeField] private float tiltDuration = 0.25f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float returnDuration = 0.3f;

    private Quaternion baseRot;
    private Coroutine tiltRoutine;

    private void Awake()
    {
        baseRot = transform.rotation;
    }

    // HeavyFloatProjectile에서 호출
    public void OnHeavyHit(Vector2 hitDir, Vector2 hitPoint)
    {
        // 플랫폼 기준 상대 위치 계산
        Vector2 localHit = transform.InverseTransformPoint(hitPoint);

        float directionX = Mathf.Sign(hitDir.x);   // 좌/우 충돌
        float directionY = Mathf.Sign(localHit.y); // 위/아래 충돌 (플랫폼 위인지 아래인지)

        // 기본적으로 좌/우 기울이기 + 위/아래에 따라 반대 효과
        // 아래를 치면 위로, 위를 치면 아래로 기울도록
        float tiltSign = directionX * directionY;

        float targetZ = tiltAngle * tiltSign;

        if (tiltRoutine != null)
            StopCoroutine(tiltRoutine);

        tiltRoutine = StartCoroutine(TiltPlatform(targetZ));
    }

    private IEnumerator TiltPlatform(float targetZ)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, targetZ);
        float t = 0f;

        // 1. 기울이기
        while (t < tiltDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / tiltDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, u);
            yield return null;
        }

        transform.rotation = endRot;
        yield return new WaitForSeconds(holdTime);

        // 2. 복귀
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
}
