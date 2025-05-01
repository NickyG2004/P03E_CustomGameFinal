// -----------------------------------------------------------------------------
// Filename: AudioManager.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Plays a specified introductory music track via the MusicManager singleton
// when the component starts. Requires an AudioClip to be assigned.
// Note: Consider renaming if more specific audio duties are not added later.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Responsible for playing a designated introductory music clip using the
/// MusicManager when this component's Start method is called.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The music clip to play automatically when this component starts.")]
    [SerializeField] private AudioClip _introSong;

    [Tooltip("The duration (in seconds) for the intro song to fade in.")]
    [SerializeField, Range(0.1f, 10f)] private float _introFadeDuration = 3.0f;

    /// <summary>
    /// Called when the script instance is first enabled. Attempts to play the
    /// assigned intro song via the MusicManager.
    /// </summary>
    private void Start()
    {
        // Check if an intro song has been assigned in the Inspector
        if (_introSong == null)
        {
            Debug.LogWarning($"[AudioManager] No intro song assigned on {gameObject.name}. No music will be played by this component.", this);
            return; // Do nothing further if no clip is assigned
        }

        // Attempt to play the intro song using the MusicManager singleton
        // Use null-conditional ?. for safety in case the instance is somehow unavailable
        MusicManager.Instance?.Play(_introSong, _introFadeDuration);

        // Optional: Log action if debugging is needed
        // Debug.Log($"[AudioManager] Requested MusicManager to play intro song: {_introSong.name} with fade duration: {_introFadeDuration}s", this);
    }
}