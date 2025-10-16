using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactText = "Open";

    public virtual void OnInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);
        // 예: 도어 열기, 상자 열기, NPC 대화 시작 등
    }
}
