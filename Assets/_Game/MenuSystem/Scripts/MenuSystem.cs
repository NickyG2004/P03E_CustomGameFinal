using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
{
    // This script contains the menu system logic for the game.
    // It handles menu navigation, input processing, and UI updates.

    // debug settings.
    [Header("Debug Settings")]
    public bool debugMode = false; // Enable or disable debug mode

    [Header("Menu Settings")]
    public string mainMenuSceneName = "MainMenu";
    public Canvas battleUI; // Reference to the Battle UI Canvas
    public Canvas WinMenu;
    // public Canvas LoseMenu;
    // public Canvas PauseMenu;

    [Header("Fade Time Settings")]
    [Tooltip("Time in seconds to fade in the win menu.")]
    [SerializeField] private float winMenuFadeTime = 1f;
    [SerializeField] private float battleUiFadeTime = 1f;

    // Get the BattleSystem instance if needed
    [Header("BattleSystem Referance")]
    [SerializeField] private BattleSystem battleSystem;

    void Start()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("MenuSystem: Menu System Initialized");
        }

        // Turn the win menu opacity to 0 at the start
        if (WinMenu != null)
        {
            WinMenu.enabled = false; // Disable the win menu initially
            // Optionally, you can set its alpha to 0 if using a CanvasGroup
            CanvasGroup canvasGroup = WinMenu.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            } 
        }
        else
        {
            Debug.LogWarning("Menu System: WinMenu is not assigned in the MenuSystem.");
        }

        // check if the BattleSystem is assigned
        if (battleSystem == null)
        {
            if (battleSystem == null)
            {
                Debug.LogError("MenuSystem: BattleSystem instance not found in the scene.");
            }
        }
    }

    // Function to fade in the win menu over time
    public void ShowWinMenu()
    {
        // check if current battlestate is WON
        if (battleSystem == null || battleSystem.GetBattleState() != BattleState.WON)
        {
            Debug.LogWarning("MenuSystem: Cannot show WinMenu because the current battle state is not WON.");
            return; // Exit if the battle state is not WON
        }

        // re-enable the win menu
        if (WinMenu != null)
        {
            WinMenu.enabled = true; // Enable the win menu
            // Optionally, you can use a CanvasGroup to fade in
            CanvasGroup canvasGroup = WinMenu.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInCanvasGroup(canvasGroup, winMenuFadeTime)); // Fade in over time
            }
            else
            {
                Debug.LogWarning("MenuSystem: WinMenu does not have a CanvasGroup component.");
            }
        }
        else
        {
            Debug.LogWarning("MenuSystem: WinMenu is not assigned in the MenuSystem.");
        }

        // Hide the battle UI and fade the alpha to 0;
        if (battleUI != null)
        {
            battleUI.enabled = false; // Disable the battle UI
            CanvasGroup battleCanvasGroup = battleUI.GetComponent<CanvasGroup>();
            if (battleCanvasGroup != null)
            {
                StartCoroutine(FadeOutCanvasGroup(battleCanvasGroup, battleUiFadeTime)); // Fade out over time
            }
            else
            {
                Debug.LogWarning("MenuSystem: BattleUI does not have a CanvasGroup component.");
            }
        }
        else
        {
            Debug.LogWarning("MenuSystem: BattleUI is not assigned in the MenuSystem.");
        }
    }

    // Function to fade out the win menu over time and fade in the battle UI
    public void HideWinMenu()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("MenuSystem: Hiding Win Menu");
        }
        // Hide the win menu and fade out
        if (WinMenu != null)
        {
            WinMenu.enabled = false; // Disable the battle UI
            CanvasGroup canvasGroup = WinMenu.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOutCanvasGroup(canvasGroup, winMenuFadeTime)); // Fade out over time
            }
            else
            {
                Debug.LogWarning("MenuSystem: WinMenu does not have a CanvasGroup component.");
            }
        }
        else
        {
            Debug.LogWarning("MenuSystem: WinMenu is not assigned in the MenuSystem.");
        }
        // Show the battle UI and fade in
        if (battleUI != null)
        {
            battleUI.enabled = true; // Enable the battle UI
            CanvasGroup battleCanvasGroup = battleUI.GetComponent<CanvasGroup>();
            if (battleCanvasGroup != null)
            {
                StartCoroutine(FadeInCanvasGroup(battleCanvasGroup, battleUiFadeTime)); // Fade in over time
            }
            else
            {
                Debug.LogWarning("MenuSystem: BattleUI does not have a CanvasGroup component.");
            }

            // set battle state to START
            if (battleSystem != null)
            {
                battleSystem.StartBattle(); // Start the battle if BattleSystem is assigned
            }
            else
            {
                Debug.LogError("MenuSystem: BattleSystem instance not found in the scene.");
            }
        }
        else
        {
            Debug.LogWarning("MenuSystem: BattleUI is not assigned in the MenuSystem.");
        }
    }

    // Coroutine to fade in a CanvasGroup
    private IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null; // Wait for the next frame
        }
        canvasGroup.alpha = 1f; // Ensure it ends at fully visible
    }

    // Coroutine to fade out a CanvasGroup
    private IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / duration));
            yield return null; // Wait for the next frame
        }
        canvasGroup.alpha = 0f; // Ensure it ends at fully invisible
        // canvasGroup.gameObject.SetActive(false); // Optionally disable the game object
    }


    // Function to return to the main menu
    public void ReturnToMainMenu()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("MenuSystem: Returning to Main Menu");
        }

        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
}
