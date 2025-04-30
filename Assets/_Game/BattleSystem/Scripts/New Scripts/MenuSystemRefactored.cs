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
    public IEnumerator TransitionBackToBattle(Canvas menu)
    {
        if (menu == loseMenu)
            SaveManager.ResetProgress();

        var cg = _screenFader.GetComponent<CanvasGroup>();
        _screenFader.gameObject.SetActive(true);
        cg.blocksRaycasts = true;

        // 1) Fade overlay in
        yield return UIFader.FadeInCanvasGroup(cg, menuFadeDuration);

        // 2) Swap the menus behind it
        SetMenuState(menu, false);
        SetMenuState(battleUI, true);

        // 3) ONLY do the unit & HUD setup here
        yield return StartCoroutine(battleSystem.SetUpBattle());

        // 4) Fade overlay out
        yield return UIFader.FadeOutCanvasGroup(cg, menuFadeDuration);
        cg.blocksRaycasts = false;
        _screenFader.gameObject.SetActive(false);

        // 5) **Now** start the turn logic
        yield return StartCoroutine(battleSystem.StartBattle());
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