using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("���� ���")]
    public Transform target;

    [Header("ī�޶� ������")]
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("�ε巯�� ���� �ӵ�")]
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        // ��ǥ ��ġ ��� (Z�� ����)
        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, Time.deltaTime * smoothSpeed);

        // 2D�� Z���� ���� ����
        transform.position = new Vector3(smoothed.x, smoothed.y, offset.z);
    }
}
