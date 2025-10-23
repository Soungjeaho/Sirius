using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Tooltip("플레이어가 접근했을 때 표시할 텍스트")]
    public string interactText = "E 키로 상호작용";

    
    public virtual void OnInteract()
    {
        Debug.Log($"{gameObject.name}의 상호작용했습니다.");
        
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<Collider2D>().bounds.size);
    }
}
