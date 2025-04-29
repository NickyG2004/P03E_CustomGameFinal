// -----------------------------------------------------------------------------
// UIFader.cs
// -----------------------------------------------------------------------------
// Static utility for fading arbitrary CanvasGroups in or out.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

public static class UIFader
{
    /// <summary>Fade a CanvasGroup from current alpha to 1 (opaque).</summary>
    public static IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    /// <summary>Fade a CanvasGroup from current alpha to 0 (transparent).</summary>
    public static IEnumerator FadeOutCanvasGroup(CanvasGroup cg, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }
}