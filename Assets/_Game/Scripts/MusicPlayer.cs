// -----------------------------------------------------------------------------
// Filename: MusicPlayer.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Component responsible for playing a single AudioClip via an attached AudioSource.
// Handles volume fading logic (fade in, fade out) as directed by external calls
// (typically from MusicManager). Manages fade coroutines to prevent conflicts.
// Requires an AudioSource component.
// -----------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

/// <summary>
/// Controls playback and volume fading for a single AudioSource, designed to be
/// managed by a coordinator like MusicManager for crossfading effects.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    #region Inspector Fields

    [Header("Debug")]
    [Tooltip("Enable detailed logging for this specific MusicPlayer instance.")]
    [SerializeField] private bool _debugMode = false;

    #endregion

    #region Private Fields

    private AudioSource _audioSource;    // Reference to the attached AudioSource component
    private Coroutine _activeFadeRoutine;  // Reference to the currently active fade coroutine

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Caches the required AudioSource component and sets initial configuration.
    /// </summary>
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        // Safety check, although RequireComponent should guarantee it exists
        if (_audioSource == null)
        {
            Debug.LogError($"[MusicPlayer] AudioSource component not found on {gameObject.name}! Disabling script.", this);
            this.enabled = false;
            return;
        }

        // Configure AudioSource defaults for background music
        _audioSource.loop = true;           // Music should loop
        _audioSource.playOnAwake = false;   // Prevent automatic playback
        _audioSource.volume = 0f;           // Start silent, fade in when needed
    }

    #endregion

    #region Public API called by MusicManager

    /// <summary> Checks if this player's AudioSource is currently playing. </summary>
    /// <returns>True if the AudioSource is valid and playing, false otherwise.</returns>
    public bool IsPlaying()
    {
        return _audioSource != null && _audioSource.isPlaying;
    }

    /// <summary> Checks if this player is currently playing the specified audio clip. </summary>
    /// <param name="clip">The AudioClip to check against.</param>
    /// <returns>True if the specified clip is not null and is the one currently playing, false otherwise.</returns>
    public bool IsPlayingClip(AudioClip clip)
    {
        return clip != null && _audioSource != null && _audioSource.isPlaying && _audioSource.clip == clip;
    }


    /// <summary>
    /// Starts playing the specified audio clip, fading in over the given duration.
    /// If another clip is currently playing/fading on this player, it will be interrupted.
    /// </summary>
    /// <param name="clipToPlay">The AudioClip to play.</param>
    /// <param name="fadeDuration">The duration (in seconds) for the fade-in and any preceding fade-out.</param>
    public void Play(AudioClip clipToPlay, float fadeDuration)
    {
        // --- Input Validation ---
        if (clipToPlay == null)
        {
            if (_debugMode) Debug.LogWarning($"[MusicPlayer] Play called with null clip on {gameObject.name}. Ignoring.", this);
            return;
        }
        // Avoid restarting if the exact same clip is already playing at full volume with no active fade
        if (IsPlayingClip(clipToPlay) && Mathf.Approximately(_audioSource.volume, 1f) && _activeFadeRoutine == null)
        {
            if (_debugMode) Debug.Log($"[MusicPlayer] Clip '{clipToPlay.name}' is already playing at full volume on {gameObject.name}. Ignoring.", this);
            return;
        }

        if (_debugMode) Debug.Log($"[MusicPlayer] Play called for '{clipToPlay.name}' on {gameObject.name} (Fade: {fadeDuration}s).", this);

        // --- Coroutine Management ---
        StopExistingFade(); // Stop any fade currently in progress

        // Start the new fade sequence (will fade out old if needed, then fade in new)
        _activeFadeRoutine = StartCoroutine(FadeToNewClipRoutine(clipToPlay, fadeDuration));
    }

    /// <summary>
    /// Fades out the currently playing audio clip over the specified duration and then stops playback.
    /// If no clip is playing or a fade is already in progress, it may be interrupted.
    /// </summary>
    /// <param name="fadeDuration">The duration (in seconds) for the fade-out.</param>
    public void Stop(float fadeDuration)
    {
        // --- Check if Stop is Necessary ---
        // Don't need to stop if not playing AND already silent
        if (!IsPlaying() && Mathf.Approximately(_audioSource.volume, 0f))
        {
            if (_debugMode) Debug.Log($"[MusicPlayer] Stop called on {gameObject.name}, but already stopped/silent. Ignoring.", this);
            StopExistingFade(); // Ensure any latent fade routine is cleared
            return;
        }

        if (_debugMode) Debug.Log($"[MusicPlayer] Stop called for '{_audioSource.clip?.name ?? "Nothing Playing"}' on {gameObject.name} (Fade: {fadeDuration}s).", this);

        // --- Coroutine Management ---
        StopExistingFade(); // Stop any other fade currently in progress

        // Start the fade-out sequence
        _activeFadeRoutine = StartCoroutine(FadeOutRoutine(fadeDuration));
    }

    #endregion

    #region Coroutine Helpers

    /// <summary>
    /// Stops the active fade coroutine if one is running and clears the reference.
    /// </summary>
    private void StopExistingFade()
    {
        if (_activeFadeRoutine != null)
        {
            if (_debugMode) Debug.Log($"[MusicPlayer] Stopping existing fade routine on {gameObject.name}.", this);
            StopCoroutine(_activeFadeRoutine);
            _activeFadeRoutine = null;
        }
    }

    /// <summary>
    /// Coroutine to orchestrate fading out the current clip (if playing)
    /// and then fading in the new clip.
    /// </summary>
    private IEnumerator FadeToNewClipRoutine(AudioClip newClip, float duration)
    {
        // 1. Fade out the current audio IF it's actually playing and audible
        if (IsPlaying() && _audioSource.volume > 0.01f)
        {
            if (_debugMode) Debug.Log($"[MusicPlayer] Fading out previous clip ('{_audioSource.clip?.name ?? "None"}') on {gameObject.name}.", this);
            yield return StartCoroutine(FadeOutRoutine(duration)); // Use StartCoroutine here to execute fully
        }
        else if (IsPlaying()) // If playing but silent, just stop it
        {
            _audioSource.Stop();
            _audioSource.volume = 0f;
        }


        // 2. Switch to the new clip and start playback at zero volume
        if (_debugMode) Debug.Log($"[MusicPlayer] Starting new clip '{newClip.name}' at volume 0 on {gameObject.name}.", this);
        _audioSource.clip = newClip;
        _audioSource.volume = 0f; // Ensure starting silent
        _audioSource.Play();      // Start playback

        // 3. Fade in the new clip
        if (_debugMode) Debug.Log($"[MusicPlayer] Fading in new clip '{newClip.name}' on {gameObject.name}.", this);
        yield return StartCoroutine(FadeInRoutine(duration)); // Use StartCoroutine here

        // Mark the overall fade process as complete only if this coroutine instance is still the active one
        if (_activeFadeRoutine != null && !_activeFadeRoutine.Equals(null))
        {
            // Check if the active routine is THIS instance before nulling
            // This check is complex, simpler to rely on StopExistingFade being called at start of Play/Stop
            _activeFadeRoutine = null;
        }

        if (_debugMode) Debug.Log($"[MusicPlayer] Fade to '{newClip.name}' complete on {gameObject.name}.", this);
    }

    /// <summary>
    /// Coroutine that fades the AudioSource volume down to 0 and then stops playback.
    /// </summary>
    private IEnumerator FadeOutRoutine(float duration)
    {
        float actualDuration = Mathf.Max(0.01f, duration); // Ensure positive duration
        float startVolume = _audioSource.volume;
        float timeElapsed = 0f;

        if (_debugMode && startVolume < 0.01f && IsPlaying())
            Debug.LogWarning($"[MusicPlayer] FadeOutRoutine started on {gameObject.name} when volume was already near zero ({startVolume:F3}) but still playing.", this);


        while (timeElapsed < actualDuration)
        {
            // If playback stopped externally mid-fade, exit gracefully
            if (!IsPlaying())
            {
                if (_debugMode) Debug.Log($"[MusicPlayer] Playback stopped externally during FadeOutRoutine on {gameObject.name}.", this);
                _audioSource.volume = 0f; // Ensure silent
                _activeFadeRoutine = null; // Clear the routine reference as it's now invalid
                yield break;
            }

            timeElapsed += Time.deltaTime; // Use scaled time
            float progress = Mathf.Clamp01(timeElapsed / actualDuration);
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        // Ensure final state after loop
        _audioSource.volume = 0f;
        if (IsPlaying()) // Only stop if it hasn't been stopped externally
        {
            _audioSource.Stop();
        }
        if (_debugMode) Debug.Log($"[MusicPlayer] FadeOutRoutine complete on {gameObject.name}. Playback stopped.", this);
        // Note: Do not clear _activeFadeRoutine here; the calling method (Stop or FadeToNewClip) should handle it or StopExistingFade.
        // Correction: Let the routine clear itself if it completes naturally.
        _activeFadeRoutine = null;
    }

    /// <summary>
    /// Coroutine that fades the AudioSource volume up to 1. Assumes playback
    /// has already been started by the caller.
    /// </summary>
    private IEnumerator FadeInRoutine(float duration)
    {
        // Safety check: Ensure audio is playing before fading in volume
        if (!IsPlaying())
        {
            if (_debugMode) Debug.LogWarning($"[MusicPlayer] FadeInRoutine called on {gameObject.name} but AudioSource was not playing. Calling Play().", this);
            _audioSource.Play();
            // If Play was just called, volume is likely 0, which is fine for starting Lerp
        }

        float actualDuration = Mathf.Max(0.01f, duration); // Ensure positive duration
        float startVolume = _audioSource.volume; // Should typically be 0 when called from FadeToNewClip
        float timeElapsed = 0f;

        if (_debugMode && startVolume > 0.99f)
            Debug.LogWarning($"[MusicPlayer] FadeInRoutine started on {gameObject.name} when volume was already near one ({startVolume:F3}).", this);

        while (timeElapsed < actualDuration)
        {
            // If playback stopped externally mid-fade, exit gracefully
            if (!IsPlaying())
            {
                if (_debugMode) Debug.Log($"[MusicPlayer] Playback stopped externally during FadeInRoutine on {gameObject.name}.", this);
                _audioSource.volume = 0f; // Ensure silent if stopped
                _activeFadeRoutine = null; // Clear the routine reference
                yield break;
            }

            timeElapsed += Time.deltaTime; // Use scaled time
            float progress = Mathf.Clamp01(timeElapsed / actualDuration);
            _audioSource.volume = Mathf.Lerp(startVolume, 1f, progress); // Lerp towards full volume
            yield return null;
        }

        // Ensure final state after loop only if still playing
        if (IsPlaying())
        {
            _audioSource.volume = 1f;
        }
        else
        {
            _audioSource.volume = 0f; // Ensure silent if stopped during fade
        }

        if (_debugMode) Debug.Log($"[MusicPlayer] FadeInRoutine complete on {gameObject.name}. Final volume: {_audioSource.volume:F2}", this);
        // Note: Do not clear _activeFadeRoutine here; the calling method (FadeToNewClip) should handle it or StopExistingFade.
        // Correction: Let the routine clear itself if it completes naturally.
        _activeFadeRoutine = null;
    }

    #endregion
}