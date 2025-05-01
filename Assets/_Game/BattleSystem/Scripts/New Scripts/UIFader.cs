// -----------------------------------------------------------------------------
// Filename: UIFader.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Provides static coroutine methods for smoothly fading CanvasGroup components
// between transparent (alpha 0) and opaque (alpha 1).
// Includes checks for null CanvasGroup and non-positive duration.
// -----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

/// <summary>
/// A static utility class containing coroutine methods to fade CanvasGroups.
/// Used for fading UI elements like menus or overlays.
/// </summary>
public static class UIFader
{
    /// <summary>
    /// Coroutine to fade a CanvasGroup from its current alpha to 1 (opaque) over a specified duration.
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup component to fade. Logs error if null.</param>
    /// <param name="duration">The time in seconds the fade should take. Clamped to be >= 0.01f.</param>
    /// <returns>An IEnumerator suitable for StartCoroutine.</returns>
    public static IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        // Validate input: Check if the CanvasGroup exists
        if (canvasGroup == null)
        {
            Debug.LogError("[UIFader] Cannot FadeInCanvasGroup: Provided CanvasGroup is null.");
            yield break; // Exit the coroutine immediately
        }

        // Ensure duration is valid to prevent issues
        float actualDuration = Mathf.Max(0.01f, duration); // Use a small minimum duration

        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;

        // Loop until the elapsed time reaches the duration
        while (timeElapsed < actualDuration)
        {
            // Increment time using scaled time (affected by Time.timeScale)
            timeElapsed += Time.deltaTime;
            // Calculate fade progress, clamping between 0 and 1
            float progress = Mathf.Clamp01(timeElapsed / actualDuration);
            // Linearly interpolate alpha towards 1 (opaque)
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // Ensure the final alpha value is exactly 1 after the loop finishes
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Coroutine to fade a CanvasGroup from its current alpha to 0 (transparent) over a specified duration.
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup component to fade. Logs error if null.</param>
    /// <param name="duration">The time in seconds the fade should take. Clamped to be >= 0.01f.</param>
    /// <returns>An IEnumerator suitable for StartCoroutine.</returns>
    public static IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        // Validate input: Check if the CanvasGroup exists
        if (canvasGroup == null)
        {
            Debug.LogError("[UIFader] Cannot FadeOutCanvasGroup: Provided CanvasGroup is null.");
            yield break; // Exit the coroutine immediately
        }

        // Ensure duration is valid to prevent issues
        float actualDuration = Mathf.Max(0.01f, duration); // Use a small minimum duration

        float startAlpha = canvasGroup.alpha;
        float timeElapsed = 0f;

        // Loop until the elapsed time reaches the duration
        while (timeElapsed < actualDuration)
        {
            // Increment time using scaled time
            timeElapsed += Time.deltaTime;
            // Calculate fade progress, clamping between 0 and 1
            float progress = Mathf.Clamp01(timeElapsed / actualDuration);
            // Linearly interpolate alpha towards 0 (transparent)
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            // Wait for the next frame
            yield return null;
        }

        // Ensure the final alpha value is exactly 0 after the loop finishes
        canvasGroup.alpha = 0f;
    }
}