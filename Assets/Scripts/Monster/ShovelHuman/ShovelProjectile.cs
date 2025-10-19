using UnityEngine;

public class ShovelProjectile : MonoBehaviour
{
    [Header("����ü ����")]
    public float lifetime = 3f;      // �� �� �� �������
    public int damage = 1;           // �÷��̾�� ���� ���ط�
    public LayerMask hitLayer;       // ���� ��� (�ַ� Player)

    private void Start()
    {
        // ���� �ð� ������ �ڵ� ����
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player Layer�� �浹���� ���� ó��
        if (((1 << collision.gameObject.layer) & hitLayer) != 0)
        {
            Debug.Log($"����ü�� {collision.gameObject.name}���� ����!");

            // PlayerController2D�� ������ �޴� �޼��尡 ������ ȣ�� ���� (��: TakeDamage)
            // var player = collision.GetComponent<PlayerController2D>();
            // if (player != null)
            // {
            //     player.TakeDamage(damage);
            // }

            Destroy(gameObject); // ���� �� ����
        }
    }
}
