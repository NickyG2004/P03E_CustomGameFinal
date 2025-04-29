// -----------------------------------------------------------------------------
// MusicManager.cs
// -----------------------------------------------------------------------------
// Singleton coordinating two MusicPlayer instances for seamless track transitions.
// -----------------------------------------------------------------------------
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    #region Singleton
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<MusicManager>();
                if (_instance == null)
                {
                    var go = new GameObject("MusicManager");
                    _instance = go.AddComponent<MusicManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Serialized Fields
    [Tooltip("Enable verbose logging for debugging music actions")]
    public bool debugMode = false;
    #endregion

    #region Private Fields
    private MusicPlayer _playerA;
    private MusicPlayer _playerB;
    private MusicPlayer _activePlayer;
    private AudioClip _currentClip;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Enforce singleton uniqueness
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        // Create two MusicPlayers for crossfading
        _playerA = gameObject.AddComponent<MusicPlayer>();
        _playerB = gameObject.AddComponent<MusicPlayer>();
        _activePlayer = _playerA;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Switch to a new music clip with crossfade.
    /// </summary>
    public void Play(AudioClip clip, float fadeTime)
    {
        if (clip == null || clip == _currentClip) return;
        if (debugMode) Debug.Log($"MusicManager: Playing {clip.name}");
        _activePlayer.Stop(fadeTime);
        _activePlayer = (_activePlayer == _playerA) ? _playerB : _playerA;
        _activePlayer.Play(clip, fadeTime);
        _currentClip = clip;
    }

    /// <summary>
    /// Fade out and stop current music.
    /// </summary>
    public void Stop(float fadeTime)
    {
        if (_currentClip == null) return;
        _activePlayer.Stop(fadeTime);
        _currentClip = null;
    }
    #endregion
}
