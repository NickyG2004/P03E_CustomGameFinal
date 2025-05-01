// -----------------------------------------------------------------------------
// Filename: ScreenFader.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Controls a fullscreen CanvasGroup for smooth scene transition fades.
// Manages fade coroutines to prevent overlaps and provides simple methods
// for fading in, fading out, and fading out before loading a new scene.
// Requires a CanvasGroup component on the same GameObject.
// -----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages a CanvasGroup to create fullscreen fade effects for scene transitions.
/// Automatically fades in when the scene starts. Provides methods for manual
/// fades and fading before scene loads. Ensures only one fade runs at a time.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    #region Inspector Fields

    [Header("Configuration")]
    [Tooltip("Duration (in seconds) for the fade in/out animations.")]
    [SerializeField, Range(0.1f, 5f)] private float _fadeDuration = 1.0f;

    [Header("Debug")]
    [Tooltip("Enable detailed logs for fade actions.")]
    [SerializeField] private bool _debugMode = false;

    #endregion

    #region Private Fields

    private CanvasGroup _canvasGroup;
    private Coroutine _activeFadeCoroutine; // Tracks the currently running fade

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// Caches the CanvasGroup component and ensures it starts fully opaque.
    /// </summary>
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) // Should not happen due to RequireComponent, but safe check
        {
            Debug.LogError("[ScreenFader] CanvasGroup component not found! Disabling script.", this);
            this.enabled = false;
            return;
        }
        // Initialize fully faded out (black/opaque)
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true; // Block interaction initially
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Automatically starts the fade-in transition when the scene begins.
    /// </summary>
    private void Start()
    {
        // Start the fade-in process (fade to transparent)
        FadeIn();
    }

    #endregion

    #region Public API - Fade Controls

    /// <summary>
    /// Starts fading the screen from opaque (black) to transparent.
    /// Stops any currently active fade. Ensures the fader GameObject is active.
    /// </summary>
    /// <returns>The Coroutine handling the fade, allowing external yielding if needed.</returns>
    public Coroutine FadeIn()
    {
        if (_debugMode) Debug.Log($"[ScreenFader] Starting Fade In (Duration: {_fadeDuration}s).", this);
        gameObject.SetActive(true); // Ensure active state
        // No need to reset alpha here, StartFadeInternal handles Lerp from current
        return StartFadeInternal(0f); // Fade to transparent
    }

    /// <summary>
    /// Starts fading the screen from transparent to opaque (black).
    /// Stops any currently active fade. Ensures the fader GameObject is active.
    /// </summary>
    /// <returns>The Coroutine handling the fade, allowing external yielding if needed.</returns>
    public Coroutine FadeOut()
    {
        if (_debugMode) Debug.Log($"[ScreenFader] Starting Fade Out (Duration: {_fadeDuration}s).", this);
        gameObject.SetActive(true); // Ensure active state
                                    // No need to reset alpha here, StartFadeInternal handles Lerp from current
        return StartFadeInternal(1f); // Fade to opaque
    }

    /// <summary>
    /// Initiates a fade out to opaque (black), and then loads the specified scene.
    /// Stops any currently active fade. Ensures the fader GameObject is active.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load after fading out.</param>
    public void StartFadeOutAndLoadScene(string sceneName)
    {
        // Validate scene name before starting coroutine
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ScreenFader] Cannot fade and load: Scene name is null or empty!", this);
            return;
        }

        if (_debugMode) Debug.Log($"[ScreenFader] Starting Fade Out and Load Scene '{sceneName}' (Fade Duration: {_fadeDuration}s).", this);

        // Ensure only one scene load routine runs
        StopExistingFade(); // Stop any simple fade first

        // Start the overarching routine that handles fading then loading
        _activeFadeCoroutine = StartCoroutine(FadeOutAndLoadSceneRoutine(sceneName));
    }

    #endregion

    #region Private Coroutines & Helpers

    /// <summary>
    /// Stops the active fade coroutine if one is running and clears the reference.
    /// </summary>
    private void StopExistingFade()
    {
        if (_activeFadeCoroutine != null)
        {
            if (_debugMode) Debug.Log($"[ScreenFader] Stopping existing fade routine on {gameObject.name}.", this);
            StopCoroutine(_activeFadeCoroutine);
            _activeFadeCoroutine = null;
        }
    }

    /// <summary>
    /// Manages the starting of fade coroutines, ensuring only one runs at a time.
    /// </summary>
    /// <param name="targetAlpha">The target alpha value (0 for transparent, 1 for opaque).</param>
    /// <returns>The newly started Coroutine reference.</returns>
    private Coroutine StartFadeInternal(float targetAlpha)
    {
        StopExistingFade(); // Stop the previous fade if it's still running

        // Start the new fade coroutine
        _activeFadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(targetAlpha));
        return _activeFadeCoroutine;
    }

    /// <summary>
    /// Coroutine that handles the actual alpha interpolation over time.
    /// Manages CanvasGroup properties (alpha, interactable, blocksRaycasts).
    /// </summary>
    /// <param name="targetAlpha">The target alpha value (0 or 1).</param>
    private IEnumerator FadeCanvasGroupRoutine(float targetAlpha)
    {
        if (_canvasGroup == null) yield break; // Safety check

        // Prevent interaction during the fade animation
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = false; // Disable interaction for duration

        float startAlpha = _canvasGroup.alpha;
        float timeElapsed = 0f;
        // Use a minimum duration to prevent division by zero or instant fades
        float actualDuration = Mathf.Max(0.01f, _fadeDuration);

        if (_debugMode) Debug.Log($"[ScreenFader] Fading from alpha {startAlpha:F2} to {targetAlpha:F2} over {actualDuration:F2}s.", this);

        while (timeElapsed < actualDuration)
        {
            timeElapsed += Time.deltaTime;
            // Calculate progress, ensuring it stays between 0 and 1
            float progress = Mathf.Clamp01(timeElapsed / actualDuration);
            // Interpolate alpha value
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // Ensure the final target alpha is set precisely
        _canvasGroup.alpha = targetAlpha;

        // Determine final state based on whether we faded in (transparent) or out (opaque)
        bool isFadedOut = Mathf.Approximately(targetAlpha, 1f); // Is opaque?
        _canvasGroup.blocksRaycasts = isFadedOut; // Block rays only when opaque
        _canvasGroup.interactable = isFadedOut;   // Allow interaction only when opaque (prevents blocking underlying UI when faded in)

        if (_debugMode) Debug.Log($"[ScreenFader] Fade complete. Final Alpha: {_canvasGroup.alpha:F2}, BlocksRaycasts: {_canvasGroup.blocksRaycasts}", this);

        // Mark the coroutine as finished *if* this was the one running
        // Avoid nulling if a larger sequence (like scene load) is still in progress
        // Correction: Simpler to always null here if it's the active one. Larger sequences restart it anyway.
        _activeFadeCoroutine = null;
    }

    /// <summary>
    /// Wrapper coroutine that first fades out, then loads the scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    private IEnumerator FadeOutAndLoadSceneRoutine(string sceneName)
    {
        // Ensure fader object is active before starting fade
        gameObject.SetActive(true);

        // Start the fade out (to alpha 1) and wait for it to complete
        // Use StartCoroutine directly on the fade logic
        yield return StartCoroutine(FadeCanvasGroupRoutine(1f));

        // If the fade was interrupted externally, _activeFadeCoroutine might be null now.
        // We should only load the scene if the fade completed fully (implied by reaching here)

        // Once fade out is complete, load the target scene
        if (_debugMode) Debug.Log($"[ScreenFader] Fade out complete. Loading scene: {sceneName}", this);
        SceneManager.LoadScene(sceneName);

        // Coroutine ends here. _activeFadeCoroutine was nulled by FadeCanvasGroupRoutine.
    }

    #endregion
}