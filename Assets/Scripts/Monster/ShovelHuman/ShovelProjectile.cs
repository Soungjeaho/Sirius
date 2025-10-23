using UnityEngine;

public class ShovelProjectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float lifetime = 3f;      // 몇 초 뒤 사라질지
    public int damage = 1;           // 플레이어에게 입힐 피해량
    public LayerMask hitLayer;       // 맞을 대상 (주로 Player)

    private void Start()
    {
        // 일정 시간 지나면 자동 삭제
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player Layer와 충돌했을 때만 처리
        if (((1 << collision.gameObject.layer) & hitLayer) != 0)
        {
            Debug.Log($"투사체가 {collision.gameObject.name}에게 명중!");

            // PlayerController2D가 데미지 받는 메서드가 있으면 호출 가능 (예: TakeDamage)
            // var player = collision.GetComponent<PlayerController2D>();
            // if (player != null)
            // {
            //     player.TakeDamage(damage);
            // }

            Destroy(gameObject); // 명중 후 제거
        }
    }
}
