using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

    [Header("BattleSystem Ref")]
    public BattleSystemRefactored battleSystem;

    void Start()
    {
        // hide both menus at start
        if (winMenu != null) winMenu.enabled = false;
        if (loseMenu != null) loseMenu.enabled = false;
        if (battleUI != null) battleUI.enabled = true;

    }

    public void ShowWinMenu()
    {
        if (battleSystem.state != BattleState.WON) return;

        if (winMenu != null)
        {
            winMenu.enabled = true;
            StartCoroutine(UIFader.FadeInCanvasGroup(winMenu.GetComponent<CanvasGroup>(), winMenuFadeTime));
        }
        if (battleUI != null)
        {
            StartCoroutine(UIFader.FadeOutCanvasGroup(battleUI.GetComponent<CanvasGroup>(), battleUiFadeTime));
            battleUI.enabled = false;
        }
    }

    public void HideWinMenu()
    {
        if (debugMode) Debug.Log("MenuSystemRefactored: Hiding WinMenu");
        if (winMenu != null)
        {
            StartCoroutine(UIFader.FadeOutCanvasGroup(winMenu.GetComponent<CanvasGroup>(), winMenuFadeTime));
            winMenu.enabled = false;
        }
        if (battleUI != null)
        {
            battleUI.enabled = true;
            StartCoroutine(UIFader.FadeInCanvasGroup(battleUI.GetComponent<CanvasGroup>(), battleUiFadeTime));
            battleSystem.StartBattle();
        }
    }

    // Function to save the game and quit to the main menu.
    public void SaveAndQuit()
    {
        // Save both levels
        PlayerPrefs.SetInt("PlayerLevel", battleSystem.playerUnit.unitLevel);
        PlayerPrefs.SetInt("EnemyLevel", battleSystem.enemyUnit.unitLevel);
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // SHOW LOSE
    public void ShowLoseMenu()
    {
        if (battleSystem.state != BattleState.LOST) return;

        if (loseMenu != null)
        {
            loseMenu.enabled = true;
            StartCoroutine(UIFader.FadeInCanvasGroup(loseMenu.GetComponent<CanvasGroup>(), loseMenuFadeTime));
        }
        if (battleUI != null)
        {
            StartCoroutine(UIFader.FadeOutCanvasGroup(battleUI.GetComponent<CanvasGroup>(), battleUiFadeTime));
            battleUI.enabled = false;
        }

        // update high score
        int best = PlayerPrefs.GetInt("BestLevel", 1);
        int current = battleSystem.playerUnit.unitLevel;
        if (current > best)
            PlayerPrefs.SetInt("BestLevel", current);
    }

    // HIDE LOSE (Retry)
    public void HideLoseMenu()
    {
        if (loseMenu != null)
        {
            StartCoroutine(UIFader.FadeOutCanvasGroup(loseMenu.GetComponent<CanvasGroup>(), loseMenuFadeTime));
            loseMenu.enabled = false;
        }
        if (battleUI != null)
        {
            battleUI.enabled = true;
            StartCoroutine(UIFader.FadeInCanvasGroup(battleUI.GetComponent<CanvasGroup>(), battleUiFadeTime));
        }
        battleSystem.StartBattle();
    }

    // Function to return to main menu.
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Called by the LoseMenu “Main Menu” button.
    /// Clears the auto-save and then loads the main menu.
    /// </summary>
    public void ReturnToMainMenuFromLose()
    {
        PlayerPrefs.DeleteKey("PlayerLevel");
        PlayerPrefs.DeleteKey("EnemyLevel");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
