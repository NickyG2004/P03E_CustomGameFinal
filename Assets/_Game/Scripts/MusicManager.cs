// -----------------------------------------------------------------------------
// Filename: MusicManager.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// A persistent Singleton MonoBehaviour that manages background music playback.
// Uses two child MusicPlayer components to enable crossfading between tracks.
// Provides simple Play and Stop methods accessible via MusicManager.Instance.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Singleton manager for handling background music playback with crossfade support.
/// Creates and coordinates two MusicPlayer instances attached to its own GameObject.
/// Persists across scene loads. Access via `MusicManager.Instance`.
/// </summary>
public class MusicManager : MonoBehaviour
{
    #region Singleton Implementation

    private static MusicManager _instance;

    /// <summary>
    /// Gets the singleton instance of the MusicManager.
    /// Will find or create the instance if it doesn't exist.
    /// </summary>
    public static MusicManager Instance
    {
        get
        {
            // If instance doesn't exist, try to find it
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MusicManager>();
            }
            // If still not found, create a new GameObject and add the component
            if (_instance == null)
            {
                var singletonObject = new GameObject("MusicManager (Singleton)");
                _instance = singletonObject.AddComponent<MusicManager>();
                // Note: DontDestroyOnLoad is handled in Awake to ensure it happens only once
                Debug.Log("[MusicManager] Instance created dynamically.", _instance);
            }
            return _instance;
        }
    }

    #endregion

    #region Inspector Fields

    [Header("Debug")]
    [Tooltip("Enable detailed logging for music play/stop actions.")]
    [SerializeField] private bool _debugMode = false; // Default to false

    #endregion

    #region Private Fields

    private MusicPlayer _playerA;       // First audio player instance
    private MusicPlayer _playerB;       // Second audio player instance
    private MusicPlayer _activePlayer;  // Reference to the currently playing MusicPlayer
    private AudioClip _currentClip;     // Reference to the currently playing AudioClip

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// Enforces the singleton pattern and initializes the MusicPlayer components.
    /// Marks the manager to persist across scene loads.
    /// </summary>
    private void Awake()
    {
        // Singleton Enforcement: Ensure only one instance exists
        if (_instance != null && _instance != this)
        {
            if (_debugMode) Debug.LogWarning($"[MusicManager] Duplicate instance found on '{gameObject.name}'. Destroying self.", this);
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Persist this MusicManager across scene loads
        DontDestroyOnLoad(gameObject);

        // --- Simpler MusicPlayer Creation ---
        // Ensure players exist by adding if necessary
        // Assume players are direct children or on this object
        var players = GetComponentsInChildren<MusicPlayer>();
        if (players.Length >= 2)
        {
            _playerA = players[0];
            _playerB = players[1];
            if (_debugMode) Debug.Log("[MusicManager] Found existing MusicPlayer components.", this);
        }
        else
        {
            // Add missing players
            _playerA = (players.Length > 0) ? players[0] : gameObject.AddComponent<MusicPlayer>();
            _playerB = gameObject.AddComponent<MusicPlayer>();
            if (_debugMode) Debug.Log("[MusicManager] Created MusicPlayer components.", this);
        }


        // Initialize player references
        _activePlayer = _playerA; // Start with Player A as active
        if (_debugMode) Debug.Log("[MusicManager] Singleton initialized.", this);
    }

    #endregion

    #region Public API

    /// <summary>
    /// Starts playing a new audio clip, crossfading from the currently playing clip (if any).
    /// Does nothing if the specified clip is null or already playing.
    /// </summary>
    /// <param name="clipToPlay">The AudioClip to play.</param>
    /// <param name="fadeDuration">The duration (in seconds) for the crossfade.</param>
    public void Play(AudioClip clipToPlay, float fadeDuration)
    {
        // --- Input Validation ---
        if (clipToPlay == null)
        {
            if (_debugMode) Debug.LogWarning("[MusicManager] Play called with a null AudioClip. Ignoring.", this);
            return;
        }
        // Prevent re-playing the same clip if the active player is already playing it
        if (clipToPlay == _currentClip && _activePlayer != null && _activePlayer.IsPlayingClip(clipToPlay))
        {
            if (_debugMode) Debug.Log($"[MusicManager] Clip '{clipToPlay.name}' is already the current track and playing. Ignoring.", this);
            return;
        }

        // --- Crossfade Logic ---
        if (_debugMode) Debug.Log($"[MusicManager] Playing '{clipToPlay.name}' (Fade: {fadeDuration}s). Current: '{_currentClip?.name ?? "None"}'", this);

        // Identify which player is currently active and which will fade in
        MusicPlayer playerToFadeOut = _activePlayer;
        MusicPlayer playerToFadeIn = (_activePlayer == _playerA) ? _playerB : _playerA;

        // Tell the currently active player to fade out and stop
        if (_currentClip != null && playerToFadeOut != null) // Only fade out if something was actually playing
        {
            playerToFadeOut.Stop(fadeDuration);
        }

        // Tell the inactive player to start playing the new clip and fade in
        if (playerToFadeIn != null)
        {
            playerToFadeIn.Play(clipToPlay, fadeDuration);
            // --- Update State ---
            _activePlayer = playerToFadeIn; // The fading-in player is now the active one
            _currentClip = clipToPlay;      // Store reference to the new clip
        }
        else
        {
            Debug.LogError("[MusicManager] Could not find inactive MusicPlayer component to fade in!", this);
            // Attempt recovery: try using the first player if available
            if (_playerA != null)
            {
                _playerA.Play(clipToPlay, fadeDuration);
                _activePlayer = _playerA;
                _currentClip = clipToPlay;
            }
            else if (_playerB != null)
            {
                _playerB.Play(clipToPlay, fadeDuration);
                _activePlayer = _playerB;
                _currentClip = clipToPlay;
            }
            else
            {
                Debug.LogError("[MusicManager] No MusicPlayer components available at all!", this);
            }
        }
    }

    /// <summary>
    /// Fades out and stops the currently playing music track over a specified duration.
    /// Does nothing if no music is currently playing.
    /// </summary>
    /// <param name="fadeDuration">The duration (in seconds) for the fade out.</param>
    public void Stop(float fadeDuration)
    {
        // Check if music is actually playing via the active player
        if (_currentClip == null || _activePlayer == null || !_activePlayer.IsPlaying())
        {
            if (_debugMode) Debug.Log("[MusicManager] Stop called, but no music is currently playing or active player invalid. Ignoring.", this);
            return;
        }

        if (_debugMode) Debug.Log($"[MusicManager] Stopping '{_currentClip.name}' (Fade: {fadeDuration}s).", this);

        // Tell the active player to fade out and stop
        _activePlayer.Stop(fadeDuration);

        // Clear the reference to the current clip
        _currentClip = null;
    }

    #endregion
}