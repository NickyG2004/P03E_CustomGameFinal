using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Tooltip("Seconds for the initial fade-in and fade-out")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup _cg;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        // start fully opaque for fade-in
        _cg.alpha = 1f;
    }

    void Start()
    {
        StartCoroutine(UnblockAfterFade());
    }

    private IEnumerator UnblockAfterFade()
    {
        // Fade from black -> transparent
        yield return UIFader.FadeOutCanvasGroup(_cg, fadeDuration);

        // **Completely turn off the panel so it no longer exists in the UI**
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fade (transparent -> black), then load a new scene.
    /// </summary>
    public IEnumerator FadeOutAndLoad(string sceneName)
    {
        // start blocking clicks again
        gameObject.SetActive(true);
        _cg.blocksRaycasts = true;
        yield return UIFader.FadeInCanvasGroup(_cg, fadeDuration);
        SceneManager.LoadScene(sceneName);
    }

    // Helper wrappers so you don't need to reference UIFader directly
    private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
        => UIFader.FadeInCanvasGroup(cg, duration);

    private IEnumerator FadeOutCanvasGroup(CanvasGroup cg, float duration)
        => UIFader.FadeOutCanvasGroup(cg, duration);
}
