// -----------------------------------------------------------------------------
// ScreenFader.cs
// -----------------------------------------------------------------------------
// Controls a fullscreen black CanvasGroup for scene transition fades.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Speed of fade in/out transitions")]
    [SerializeField] private float fadeDuration = 1f;
    #endregion

    #region Private Fields
    private CanvasGroup _canvasGroup;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Begin fully opaque (screen covered)
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;
    }

    private void Start()
    {
        // Fade into scene automatically on load
        StartCoroutine(FadeTo(0f));
    }
    #endregion

    #region Public API
    /// <summary>
    /// Fade to black then load a new scene.
    /// </summary>
    public IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return FadeTo(1f);
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    #region Helpers
    private IEnumerator FadeTo(float targetAlpha)
    {
        // Block raycasts while fading
        _canvasGroup.blocksRaycasts = true;
        float startAlpha = _canvasGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = targetAlpha;
        // If transparent, disable blocking and hide
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
    #endregion
}
