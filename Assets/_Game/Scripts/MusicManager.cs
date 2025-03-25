using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public bool debugMode = false;
    public bool debugWarningMode = true;

    private MusicPlayer _musicPlayer_01;
    private MusicPlayer _musicPlayer_02;
    private MusicPlayer _activeMusicPlayer = null;

    private float _volume = 1f;
    public float Volume
    {
        get => _volume;
        private set
        {
            value = Mathf.Clamp(value, 0, 1);
            _volume = value;
        }
    }

    private AudioClip _activeMusicTrack = null;

    // Singleton pattern
    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            // laxy instantiation
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<MusicManager>();
                if (_instance == null)
                {
                    GameObject singletonGO = new GameObject("MusicManager_Singleton");
                    _instance = singletonGO.AddComponent<MusicManager>();
                    DontDestroyOnLoad(singletonGO);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // This is singleton instance
        SetupMusicPlayers();

    }

    public void Play(AudioClip musicTrack, float fadeTime)
    {
        // guard clause
        if (musicTrack == null)
        {
            if (debugWarningMode)
                Debug.LogWarning("MusicManager: music track is null");
            return;
        }
        if (musicTrack == _activeMusicTrack)
        {
            if (debugWarningMode)
                Debug.LogWarning("MusicManager: music track is already playing");
            return;
        }

        // stoping the current music player
        if (_activeMusicTrack != null)
        { 
            _activeMusicPlayer.Stop(fadeTime);
        }

        // switch to new player while previous one is fading out
        SwitchActiveMusicPlayer();
        _activeMusicTrack = musicTrack;

        // play the new music track (with fade up)
        _activeMusicPlayer.Play(musicTrack, fadeTime);

    }

    public void Stop(float fadeTime)
    {
        // dont stop if no music is playing
        if (_activeMusicTrack == null)
        {
            if (debugWarningMode)
                Debug.LogWarning("MusicManager: no music is playing");
            return;
        }

        // clear out active track and stop the player
        _activeMusicTrack = null;
        _activeMusicPlayer.Stop(fadeTime);
    }

    private void SetupMusicPlayers()
    {
        _musicPlayer_01 = gameObject.AddComponent<MusicPlayer>();
        _musicPlayer_02 = gameObject.AddComponent<MusicPlayer>();
        // choose a starting 'active' music player 
        _activeMusicPlayer = _musicPlayer_01;
    }

    private void SwitchActiveMusicPlayer()
    {
        if (_activeMusicPlayer == _musicPlayer_01)
        {
            _activeMusicPlayer = _musicPlayer_02;
        }
        else if (_activeMusicPlayer == _musicPlayer_02)
        {
            _activeMusicPlayer = _musicPlayer_01;
        }
    }
}
