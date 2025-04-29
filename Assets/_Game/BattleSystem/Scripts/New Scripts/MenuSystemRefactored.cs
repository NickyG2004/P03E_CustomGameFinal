using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;            // for Action

public class MenuSystemRefactored : MonoBehaviour
{
    [Header("Debug")]
    public bool debugMode = false;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public Canvas battleUI;
    public Canvas winMenu;
    public Canvas loseMenu;


    [Header("Fade Times")]
    public float winMenuFadeTime = 1f;
    public float battleUiFadeTime = 1f;
    public float loseMenuFadeTime = 1f;

    [Header("End Battle Fade")]
    [SerializeField] private float endFadeDuration = 1f;
    private ScreenFader _screenFader;

    [Header("BattleSystem Ref")]
    public BattleSystemRefactored battleSystem;

    void Start()
    {
        // hide both menus at start
        if (winMenu != null) winMenu.enabled = false;
        if (loseMenu != null) loseMenu.enabled = false;
        if (battleUI != null) battleUI.enabled = true;

    }

    private void Awake()
    {
        // Grab the fader (even if it's initially inactive)
        _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
    }

    public void ShowWinMenu()
    {
        // 1) disable the battle visuals right away
        battleUI.enabled = false;
        var battleCG = battleUI.GetComponent<CanvasGroup>();
        if (battleCG != null)
            battleCG.blocksRaycasts = false;

        // 2) enable the win menu canvas so it's ready as soon as the screen fades back in
        winMenu.enabled = true;
        var winCG = winMenu.GetComponent<CanvasGroup>();
        if (winCG != null)
        {
            winCG.alpha = 1f;
            winCG.blocksRaycasts = true;
        }
    }


    /// <summary>
    /// Called by the WinMenu “Battle Again” button.
    /// Immediately hides the win menu, re-shows the battle UI and restarts the fight.
    /// </summary>
    public void HideWinMenu()
    {
        // Kick off the fade to battle process:
        StartCoroutine(FadeToBattle(() =>
        {
            // This action runs while the screen is black:

            // 1) Turn off the win menu
            winMenu.enabled = false;
            var winCg = winMenu.GetComponent<CanvasGroup>();
            if (winCg != null) winCg.blocksRaycasts = false;

            // 2) Re-enable battle UI
            battleUI.enabled = true;
            var battleCg = battleUI.GetComponent<CanvasGroup>();
            if (battleCg != null)
            {
                battleCg.alpha = 1f;
                battleCg.blocksRaycasts = true;
            }

            // 3) Restart the battle
            battleSystem.StartBattle();
        }));
    }

    // Function to save the game and quit to the main menu.
    public void SaveAndQuit()
    {
        // Save both levels
        PlayerPrefs.SetInt("PlayerLevel", battleSystem.playerUnit.unitLevel);
        PlayerPrefs.SetInt("EnemyLevel", battleSystem.enemyUnit.unitLevel);
        PlayerPrefs.Save();

        StartCoroutine(DoFadeAndLoad(mainMenuSceneName));
    }

    // SHOW LOSE
    public void ShowLoseMenu()
    {
        battleUI.enabled = false;
        var battleCG = battleUI.GetComponent<CanvasGroup>();
        if (battleCG != null)
            battleCG.blocksRaycasts = false;

        loseMenu.enabled = true;
        var loseCG = loseMenu.GetComponent<CanvasGroup>();
        if (loseCG != null)
        {
            loseCG.alpha = 1f;
            loseCG.blocksRaycasts = true;
        }

        // update high score
        int best = PlayerPrefs.GetInt("BestLevel", 1);
        int current = battleSystem.playerUnit.unitLevel;
        if (current > best)
            PlayerPrefs.SetInt("BestLevel", current);
    }

    /// <summary>
    /// Called by the LoseMenu “Retry” button.
    /// Immediately hides the lose menu, re shows the battle UI and restarts the fight.
    /// </summary>
    public void HideLoseMenu()
    {
        StartCoroutine(FadeToBattle(() =>
        {
            // While black:

            // a) Hide the lose menu
            loseMenu.enabled = false;
            var loseCg = loseMenu.GetComponent<CanvasGroup>();
            if (loseCg != null) loseCg.blocksRaycasts = false;

            // b) Show the battle UI
            battleUI.enabled = true;
            var battleCg = battleUI.GetComponent<CanvasGroup>();
            if (battleCg != null)
            {
                battleCg.alpha = 1f;
                battleCg.blocksRaycasts = true;
            }

            // c) Restart the battle
            battleSystem.StartBattle();
        }));
    }

    // Function to return to main menu.
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Called by the LoseMenu “Main Menu” button.
    /// Clears the auto save and then loads the main menu.
    /// </summary>
    public void ReturnToMainMenuFromLose()
    {
        PlayerPrefs.DeleteKey("PlayerLevel");
        PlayerPrefs.DeleteKey("EnemyLevel");

        StartCoroutine(DoFadeAndLoad(mainMenuSceneName));
    }

    /// <summary>
    /// Shared helper to drive the fade and scene-change.
    /// </summary>
    private IEnumerator DoFadeAndLoad(string sceneName)
    {
        // this will search even inactive objects
        ScreenFader fader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        if (fader != null)
        {
            yield return fader.FadeOutAndLoad(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Fade to black, invoke the swap back to battle action,
    /// then fade back to transparent.
    /// </summary>
    private IEnumerator FadeToBattle(Action hideMenuAndRestart)
    {
        if (_screenFader != null)
        {
            // grab the CanvasGroup on the fader
            CanvasGroup cg = _screenFader.GetComponent<CanvasGroup>();

            // 1) enable and block
            _screenFader.gameObject.SetActive(true);
            cg.blocksRaycasts = true;

            // 2) fade to black
            yield return UIFader.FadeInCanvasGroup(cg, endFadeDuration);

            // 3) swap UI: hide the menu, show the battle canvas, restart the battle
            hideMenuAndRestart();

            // 4) fade back to transparent
            yield return UIFader.FadeOutCanvasGroup(cg, endFadeDuration);

            // 5) unblock and disable
            cg.blocksRaycasts = false;
            _screenFader.gameObject.SetActive(false);
        }
        else
        {
            // fallback: just do it immediately
            hideMenuAndRestart();
        }
    }
}
