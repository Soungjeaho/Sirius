using UnityEngine;

/// <summary>
/// 이펙트 생성 후 일정 시간 뒤에 오브젝트를 자동으로 파괴하는 스크립트.
/// 이펙트 Prefab에 부착하여 사용합니다.
/// </summary>
public class AutoDestroyVFX : MonoBehaviour
{
    [Tooltip("오브젝트가 자동으로 파괴될 때까지의 시간 (초)")]
    [SerializeField] private float lifetime = 1.0f;

     private void Awake()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                lifetime = clipInfo[0].clip.length; // 현재 재생 중인 클립의 길이를 가져옵니다.
            }
        }
        Destroy(gameObject, lifetime);
    }
   
}