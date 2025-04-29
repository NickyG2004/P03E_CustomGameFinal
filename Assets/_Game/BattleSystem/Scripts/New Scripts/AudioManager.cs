// -----------------------------------------------------------------------------
// AudioManager.cs
// -----------------------------------------------------------------------------
// Singleton for playing one-shot sound effects with global volume control.
// -----------------------------------------------------------------------------
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Serialized Fields
    [Tooltip("Master volume for all SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    #endregion

    #region Public API
    /// <summary>
    /// Play a one-shot AudioClip at camera position, auto-destroying source.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        var go = new GameObject("SFX_" + clip.name);
        var src = go.AddComponent<AudioSource>();
        src.volume = sfxVolume;
        src.PlayOneShot(clip);
        Object.Destroy(go, clip.length + 0.1f);
    }
    #endregion
}