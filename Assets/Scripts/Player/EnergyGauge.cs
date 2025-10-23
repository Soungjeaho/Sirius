using UnityEngine;
using UnityEngine.UI;

public class EnergyGauge : MonoBehaviour
{
    [SerializeField] private int maxGauge = 10;
    [SerializeField] private int currentGauge = 0;

    [Header("UI References")]
    [SerializeField] private Image[] gaugeBlocks;

    private void Start()
    {
        UpdateUI();

        Debug.Log("1번: Gauge + 1");
        Debug.Log("2번: Gauge - 1");
        Debug.Log("3번: Gauge 풀 충전");
    }

    private void Update()
    {
        TestGauge();
    }

    public void AddGauge(int amount)
    {
        currentGauge = Mathf.Clamp(currentGauge + amount, 0, maxGauge);
        UpdateUI();
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge < amount)
            return false;

        currentGauge -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < gaugeBlocks.Length; i++)
        {
            Color c = gaugeBlocks[i].color;

            // 현재 게이지 이하일 경우 완전 불투명, 초과 시 반투명 처리
            if (i < currentGauge)
                c.a = 255f / 255f;
            else
                c.a = 50f / 255f;

            gaugeBlocks[i].color = c;
        }
    }

    public bool IsFull()
    {
        return currentGauge >= maxGauge;
    }

    private void TestGauge()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddGauge(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseGauge(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddGauge(10);
        }
    }
}
