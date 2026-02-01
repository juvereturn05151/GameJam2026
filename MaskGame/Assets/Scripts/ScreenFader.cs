using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private SpriteRenderer overlay;
    [SerializeField] private float defaultDuration = 0.25f;

    Coroutine _co;

    void Reset()
    {
        overlay = GetComponent<SpriteRenderer>();
    }

    public void FadeTo(float targetAlpha, float duration = -1f)
    {
        if (!overlay) return;

        Debug.Log($"Fading to alpha {targetAlpha} over {duration} seconds.");

        if (duration < 0f) duration = defaultDuration;

        if (_co == null) 
        _co = StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        Debug.Log($"Fade Routine");

        Color c = overlay.color;
        float startAlpha = c.a;

        if (duration <= 0f)
        {
            c.a = targetAlpha;
            overlay.color = c;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            c.a = a;
            overlay.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        overlay.color = c;
    }

    public void SetAlpha(float a)
    {
        if (!overlay) return;
        var c = overlay.color;
        c.a = a;
        overlay.color = c;
    }
}
