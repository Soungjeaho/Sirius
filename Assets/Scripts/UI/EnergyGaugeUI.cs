using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnergyGaugeUI : MonoBehaviour
{
    [SerializeField] private Image[] gaugeBlocks;
    [SerializeField] private Color activeColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private float fillDuration = 0.3f;

    public void UpdateUI(int currentGauge, int maxGauge)
    {
        for (int i = 0; i < gaugeBlocks.Length; i++)
        {
            if (i < currentGauge)
            {
                if (gaugeBlocks[i].color != activeColor)
                    StartCoroutine(FillCoroutine(gaugeBlocks[i]));
            }
            else
            {
                gaugeBlocks[i].color = inactiveColor;
            }
        }
    }

    private IEnumerator FillCoroutine(Image block)
    {
        if (block == null)
            yield break;  // ← null 참조 방지

        Color startColor = inactiveColor;
        Color endColor = activeColor;
        float time = 0f;

        while (time < fillDuration)
        {
            float t = time / fillDuration;
            block.color = Color.Lerp(startColor, endColor, t);
            time += Time.deltaTime;
            yield return null;
        }

        block.color = endColor;
    }
}
