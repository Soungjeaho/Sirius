using UnityEngine;

/// Gfx 오브젝트에 붙여서 SpriteRenderer.flipX만 제어.
/// 시작 방향을 강제로 만들지 않고, '플레이어가 어느 쪽에 있는지'를 보고 즉시 맞춥니다.
[DisallowMultipleComponent]
public class EnemyLookAtTarget : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] float deadZoneX = 0.03f;      // 너무 근접하면 깜빡임 방지
    [SerializeField] bool invert = false;          // 아트가 기본이 왼쪽을 보는 경우 체크

    Transform target;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>(true);
    }

    void OnEnable()
    {
        // Player 자동 획득
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) target = p.transform;

        // 시작 시 '플레이어 쪽'으로 바로 1회 맞춤 (오른쪽 강제 X)
        if (sr && target)
        {
            float dx = target.position.x - transform.position.x;
            if (Mathf.Abs(dx) >= deadZoneX) sr.flipX = invert ? dx > 0f : dx < 0f;
        }
    }

    void LateUpdate()
    {
        if (!sr)
            return;

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (!p) return;
            target = p.transform;
        }

        float dx = target.position.x - transform.position.x;
        if (Mathf.Abs(dx) < deadZoneX) return;

        // 오른쪽이면 flipX=false, 왼쪽이면 flipX=true (invert 옵션 고려)
        sr.flipX = invert ? dx > 0f : dx < 0f;
    }
}
