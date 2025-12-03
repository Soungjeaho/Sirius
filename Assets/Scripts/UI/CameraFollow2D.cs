using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("따라갈 대상")]
    [SerializeField] private Transform target = null;   // 보통 Player

    [Header("카메라 위치 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

    [Header("부드러운 따라가기 정도")]
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // 목표 위치 = 플레이어 위치 + 오프셋
        Vector3 desiredPosition = target.position + offset;

        // 부드럽게 따라가기 (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // Z축은 그대로 유지하고 싶으면 이렇게 고정해도 됨
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }
}
