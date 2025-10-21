using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class EnergyGauge : MonoBehaviour
{
    [SerializeField] private int maxGauge = 10;
    [SerializeField] private int currentGauge = 0;

    [Header("UI References")]
    [SerializeField] private EnergyGaugeUI gaugeUI;
    [SerializeField] private Image[] gaugeBlocks;

    private void Start()
    {
        gaugeUI.UpdateUI(currentGauge, maxGauge);
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
        int prev = currentGauge;
        currentGauge = Mathf.Clamp(currentGauge + amount, 0, maxGauge);
        gaugeUI.UpdateUI(currentGauge, maxGauge);
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge < amount)
            return false;

        currentGauge -= amount;
        gaugeUI.UpdateUI(currentGauge, maxGauge);
        return true;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < gaugeBlocks.Length; i++)
        {
            gaugeBlocks[i].enabled = (i < currentGauge);
        }
    }

    public bool IsFull()
    {
        return currentGauge >= maxGauge;
    }

    public void TestGauge()
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
