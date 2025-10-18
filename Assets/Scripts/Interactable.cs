using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Tooltip("�÷��̾ �������� �� ǥ���� �ؽ�Ʈ")]
    public string interactText = "E Ű�� ��ȣ�ۿ�";

    // �÷��̾ ��ȣ�ۿ� �� ȣ��
    public virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name}�� ��ȣ�ۿ��߽��ϴ�!");
        // ���⿡ ��: �� ����, NPC ��ȭ ����, ������ �ݱ� ���� ������ �߰� ����
    }

    // ����׿����� ��ȣ�ۿ� ���� ǥ��
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
