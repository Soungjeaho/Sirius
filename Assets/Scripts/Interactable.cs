using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Tooltip("플레이어가 접근했을 때 표시할 텍스트")]
    public string interactText = "E 키로 상호작용";

    // 플레이어가 상호작용 시 호출
    public virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name}와 상호작용했습니다!");
        // 여기에 예: 문 열기, NPC 대화 시작, 아이템 줍기 등의 로직을 추가 가능
    }

    // 디버그용으로 상호작용 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
