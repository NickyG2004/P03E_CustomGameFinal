// -----------------------------------------------------------------------------
// MenuSystemRefactored.cs
// -----------------------------------------------------------------------------
// Manages transition between battle UI, win menu, lose menu, and main menu.
// -----------------------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;

public class MenuSystemRefactored : MonoBehaviour
{
    #region Serialized Fields
    public Canvas battleUI;
    public Canvas winMenu;
    public Canvas loseMenu;
    [Tooltip("Fade duration for menu transitions")] public float menuFadeDuration = 1f;
    [Tooltip("Main menu scene name")] public string mainMenuSceneName;
    public BattleSystemRefactored battleSystem;
    #endregion

    #region Private Fields
    private ScreenFader _screenFader;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        SetMenuState(battleUI, true);
        SetMenuState(winMenu, false);
        SetMenuState(loseMenu, false);
    }
    #endregion

    #region Public API
    public void ShowWinMenu() => SetMenuState(winMenu, true);
    public void ShowLoseMenu() => SetMenuState(loseMenu, true);
    public void HideWinMenu() => StartCoroutine(TransitionBackToBattle(winMenu));
    public void HideLoseMenu() => StartCoroutine(TransitionBackToBattle(loseMenu));

    public void OnLoseMenuQuit()
    {
        StartCoroutine(TransitionToMainMenu(wasLoss: true));
    }

    public void OnWinMenuSaveAndQuit()
    {
        StartCoroutine(TransitionToMainMenu(wasLoss: false));
    }
    #endregion

    #region Helpers
    private void SetMenuState(Canvas c, bool enabled)
    {
        if (c == null) return;
        c.enabled = enabled;
        var cg = c.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = enabled;
    }

    /// <summary>
    /// Handles re-entry to battle after win or loss.
    /// </summary>
    private IEnumerator TransitionBackToBattle(Canvas menu)
    {
        // If we're retrying after a loss, clear out any saved levels
        if (menu == loseMenu)
            SaveManager.ResetProgress();

        if (_screenFader != null)
        {
            var cg = _screenFader.GetComponent<CanvasGroup>();
            _screenFader.gameObject.SetActive(true);
            cg.blocksRaycasts = true;
            yield return UIFader.FadeInCanvasGroup(cg, menuFadeDuration);

            SetMenuState(menu, false);
            SetMenuState(battleUI, true);
            battleSystem.StartBattle();

            yield return UIFader.FadeOutCanvasGroup(cg, menuFadeDuration);
            cg.blocksRaycasts = false;
            _screenFader.gameObject.SetActive(false);
        }
        else
        {
            SetMenuState(menu, false);
            SetMenuState(battleUI, true);
            battleSystem.StartBattle();
        }
    }

    private IEnumerator TransitionToMainMenu(bool wasLoss)
    {
        // If we're returning to the main menu after a loss, clear out any saved levels
        if (wasLoss)
            SaveManager.ResetProgress();


        if (_screenFader != null)
        {
            var cg = _screenFader.GetComponent<CanvasGroup>();
            _screenFader.gameObject.SetActive(true);
            cg.blocksRaycasts = true;
            yield return UIFader.FadeInCanvasGroup(cg, menuFadeDuration);
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }
    #endregion
}