// -----------------------------------------------------------------------------
// MusicPlayer.cs
// -----------------------------------------------------------------------------
// Handles playing and crossfading individual AudioClips via an AudioSource.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Duration of fade transitions for music tracks")]
    [SerializeField] private float fadeDuration;
    #endregion

    #region Private Fields
    private AudioSource _audioSource;    // Local AudioSource component
    private Coroutine _fadeRoutine;      // Reference to active fade coroutine
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Cache the AudioSource and ensure looping for continuous music
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Crossfade from any currently playing track to the specified clip.
    /// </summary>
    public void Play(AudioClip clip, float fadeTime)
    {
        if (clip == null || _audioSource.clip == clip) return;
        // Stop existing fade if in progress
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        // Begin crossfade sequence
        _fadeRoutine = StartCoroutine(FadeToNewClip(clip, fadeTime));
    }

    /// <summary>
    /// Fade out and stop current track.
    /// </summary>
    public void Stop(float fadeTime)
    {
        if (_audioSource.clip == null) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutRoutine(fadeTime));
    }
    #endregion

    #region Coroutines
    private IEnumerator FadeToNewClip(AudioClip newClip, float duration)
    {
        // 1) Fade out current audio
        yield return FadeOutRoutine(duration);
        // 2) Switch clip and start playing
        _audioSource.clip = newClip;
        _audioSource.Play();
        // 3) Fade in to full volume
        yield return FadeInRoutine(duration);
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVol = _audioSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        _audioSource.volume = 0f;
        _audioSource.Stop();
    }

    private IEnumerator FadeInRoutine(float duration)
    {
        // Ensure playback is started before raising volume
        _audioSource.Play();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        _audioSource.volume = 1f;
    }
    #endregion
}