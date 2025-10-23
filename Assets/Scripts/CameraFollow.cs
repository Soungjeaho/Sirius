using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target;

    [Header("카메라 오프셋")]
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("부드러운 추적 속도")]
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        
        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, Time.deltaTime * smoothSpeed);

        
        transform.position = new Vector3(smoothed.x, smoothed.y, offset.z);
    }
}
