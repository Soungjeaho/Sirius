using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactText = "Open";

    public virtual void OnInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);
        // ��: ���� ����, ���� ����, NPC ��ȭ ���� ��
    }
}
