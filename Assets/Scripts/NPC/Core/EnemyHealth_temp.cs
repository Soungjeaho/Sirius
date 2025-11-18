using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 체력 설정")]
    public int health = 3;

    [Header("참조")]
    [SerializeField] private EnergyGauge gauge; // ✅ 게이지 참조

    private void Start()
    {
        // Inspector에서 직접 할당 안 되어 있으면 자동 탐색
        if (gauge == null)
            gauge = FindObjectOfType<EnergyGauge>();
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log($"{gameObject.name}이(가) {dmg} 피해를 입음! 현재 체력: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // ✅ 게이지 1개 회복
        if (gauge != null)
        {
            gauge.AddGauge(1);
            Debug.Log($"게이지 +1 (현재 게이지: {gauge.CurrentGauge})");
        }

        // 적 오브젝트 제거
        Destroy(gameObject);
    }
}
