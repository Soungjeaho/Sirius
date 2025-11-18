using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HookModeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image hookIcon;
    [SerializeField] private Image frame;

    [Header("Hook Sprites")]
    [SerializeField] private Sprite normalHookSprite;
    [SerializeField] private Sprite heavyHookSprite;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color heavyColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private float switchAnimTime = 0.2f;

    private Coroutine fadeRoutine;

    public void UpdateUI(int mode)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(SwitchModeUI(mode));
    }

    private IEnumerator SwitchModeUI(int mode)
    {
        float t = 0f;
        Color startColor = hookIcon.color;
        Color endColor = (mode == 1) ? normalColor : heavyColor;
        Sprite nextSprite = (mode == 1) ? normalHookSprite : heavyHookSprite;

        // Fade-out
        while (t < switchAnimTime)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / switchAnimTime);
            hookIcon.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Sprite 교체
        hookIcon.sprite = nextSprite;
        frame.color = endColor; // 프레임도 같은 색상으로 변화

        // Fade-in
        t = 0f;
        while (t < switchAnimTime)
        {
            t += Time.deltaTime;
            float alpha = t / switchAnimTime;
            hookIcon.color = new Color(endColor.r, endColor.g, endColor.b, alpha);
            yield return null;
        }
    }
}
