// -----------------------------------------------------------------------------
// Filename: MenuSystemRefactored.cs
// (Provided as Refactored Example)
// -----------------------------------------------------------------------------
// Manages transitions between different UI Canvases related to the battle:
// the main Battle UI, the Win Menu, and the Lose Menu. Also handles returning
// to the main menu scene. Coordinates with BattleSystem and ScreenFader.
// -----------------------------------------------------------------------------

using System; // Needed for Action
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Needed for SceneManager

/// <summary>
/// Controls UI Canvas visibility and transitions between the main battle UI,
/// win/lose screens, and loading the main menu scene.
/// </summary>
public class MenuSystemRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------
    [Header("UI Canvases (Assign in Inspector)")]
    [SerializeField, Tooltip("Canvas containing the primary battle interface (HUDs, action buttons).")]
    private Canvas _battleUI;

    [SerializeField, Tooltip("Canvas displayed when the player wins the battle.")]
    private Canvas _winMenu;

    [SerializeField, Tooltip("Canvas displayed when the player loses the battle.")]
    private Canvas _loseMenu;

    [Header("System References (Assign in Inspector)")]
    [SerializeField, Tooltip("Reference to the core BattleSystem controller.")]
    private BattleSystemRefactored _battleSystem;

    [Header("Transition Settings")]
    [SerializeField, Tooltip("Default duration (seconds) for fade transitions between menus.")]
    [Range(0.1f, 5f)] private float _menuFadeDuration = 1f;

    [SerializeField, Tooltip("The exact name of the Main Menu scene asset to load.")]
    private string _mainMenuSceneName = "MainMenuScene"; // Provide a default

    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    private ScreenFader _screenFader; // Optional component for smooth fades

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds optional ScreenFader and sets initial menu states (Battle UI active).
    /// Validates required references.
    /// </summary>
    private void Awake()
    {
        // Validate required references
        if (_battleUI == null) Debug.LogError("[MenuSystem] Battle UI Canvas not assigned!", this);
        if (_winMenu == null) Debug.LogError("[MenuSystem] Win Menu Canvas not assigned!", this);
        if (_loseMenu == null) Debug.LogError("[MenuSystem] Lose Menu Canvas not assigned!", this);
        if (_battleSystem == null) Debug.LogError("[MenuSystem] Battle System reference not assigned!", this);
        if (string.IsNullOrEmpty(_mainMenuSceneName)) Debug.LogError("[MenuSystem] Main Menu Scene Name is not set!", this);


        // Attempt to find the optional ScreenFader
        _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        // No warning needed here, functionality adapts if it's null

        // Set initial state: Only Battle UI should be visible at start
        SetMenuState(_battleUI, true);
        SetMenuState(_winMenu, false);
        SetMenuState(_loseMenu, false);
    }

    // -------------------------------------------------------------------------
    // Public Methods (Called by BattleSystemRefactored or UI Buttons)
    // -------------------------------------------------------------------------

    /// <summary> Shows the Win Menu canvas. </summary>
    public void ShowWinMenu() => SetMenuState(_winMenu, true);

    /// <summary> Shows the Lose Menu canvas. </summary>
    public void ShowLoseMenu() => SetMenuState(_loseMenu, true);

    /// <summary> Hides the Win Menu and transitions back to start a new battle. </summary>
    public void OnWinMenuNextBattle() => StartCoroutine(TransitionBackToBattleRoutine(_winMenu, false));

    /// <summary> Hides the Lose Menu and transitions back to restart the battle (resets progress). </summary>
    public void OnLoseMenuRetry() => StartCoroutine(TransitionBackToBattleRoutine(_loseMenu, true));

    /// <summary> Transitions from the Lose Menu back to the Main Menu (resets progress). </summary>
    public void OnLoseMenuMainMenu() => StartCoroutine(TransitionToMainMenuRoutine(true));

    /// <summary> Transitions from the Win Menu back to the Main Menu (keeps progress). </summary>
    public void OnWinMenuMainMenu() => StartCoroutine(TransitionToMainMenuRoutine(false));


    // -------------------------------------------------------------------------
    // Private Methods & Coroutines
    // -------------------------------------------------------------------------

    /// <summary>
    /// Enables or disables a specific menu Canvas and its interaction blocking (CanvasGroup).
    /// </summary>
    /// <param name="menuCanvas">The Canvas component of the menu to modify.</param>
    /// <param name="enable">True to enable (make visible and interactable), false to disable.</param>
    private void SetMenuState(Canvas menuCanvas, bool enable)
    {
        if (menuCanvas == null) return; // Ignore if canvas is null

        menuCanvas.enabled = enable;

        // Also control interaction blocking via CanvasGroup if present
        CanvasGroup canvasGroup = menuCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.interactable = enable;
            canvasGroup.blocksRaycasts = enable;
            // Optional: Control alpha for instant hide/show without needing UIFader
            // canvasGroup.alpha = enable ? 1f : 0f;
        }
    }

    /// <summary>
    /// Coroutine handles transitioning from a win/lose menu back into the battle scene setup.
    /// Uses ScreenFader if available. Optionally resets progress on retry.
    /// </summary>
    /// <param name="menuToHide">The win or lose menu Canvas to hide.</param>
    /// <param name="resetProgressOnRetry">True if progress should be reset (typically after a loss).</param>
    private IEnumerator TransitionBackToBattleRoutine(Canvas menuToHide, bool resetProgressOnRetry)
    {
        // Reset progress if specified (e.g., retrying after loss)
        if (resetProgressOnRetry)
        {
            Debug.Log("[MenuSystem] Resetting progress before retry.");
            SaveManager.ResetProgress();
        }

        // --- Transition Logic ---
        bool useFader = (_screenFader != null);
        CanvasGroup fadeCanvasGroup = useFader ? _screenFader.GetComponent<CanvasGroup>() : null;
        if (useFader && fadeCanvasGroup == null) useFader = false; // Fader exists but no CanvasGroup? Disable fade.

        if (useFader)
        {
            // --- Smooth Fade Transition ---
            _screenFader.gameObject.SetActive(true); // Ensure fader is active
            fadeCanvasGroup.blocksRaycasts = true;   // Block input

            // 1. Fade overlay IN
            yield return StartCoroutine(UIFader.FadeInCanvasGroup(fadeCanvasGroup, _menuFadeDuration));

            // 2. Swap menus and setup battle behind the overlay
            SetMenuState(menuToHide, false); // Hide Win/Lose menu
            SetMenuState(_battleUI, true);   // Show Battle UI (though overlay covers it)
            yield return StartCoroutine(_battleSystem.SetupBattleRoutine()); // Setup units, HUDs

            // 3. Fade overlay OUT
            yield return StartCoroutine(UIFader.FadeOutCanvasGroup(fadeCanvasGroup, _menuFadeDuration));

            // 4. Restore interaction
            fadeCanvasGroup.blocksRaycasts = false;
            // Do not disable _screenFader GameObject here
        }
        else
        {
            // --- Instant Transition (No Fader) ---
            SetMenuState(menuToHide, false);
            SetMenuState(_battleUI, true);
            yield return StartCoroutine(_battleSystem.SetupBattleRoutine());
        }

        // 5. Start the battle flow (turn sequence)
        yield return StartCoroutine(_battleSystem.StartBattleRoutine());
    }

    /// <summary>
    /// Coroutine handles transitioning back to the main menu scene.
    /// Uses ScreenFader if available. Optionally resets progress.
    /// </summary>
    /// <param name="resetProgress">True if progress should be reset before returning to main menu.</param>
    private IEnumerator TransitionToMainMenuRoutine(bool resetProgress)
    {
        // Reset progress if specified (e.g., quitting after loss)
        if (resetProgress)
        {
            Debug.Log("[MenuSystem] Resetting progress before returning to Main Menu.");
            SaveManager.ResetProgress();
        }

        // --- Transition Logic ---
        bool useFader = (_screenFader != null);
        // Note: Scene load will happen regardless of fader success after yield

        if (useFader)
        {
            // Use the ScreenFader's built-in scene loading
            _screenFader.StartFadeOutAndLoadScene(_mainMenuSceneName);
            yield return null; // Allow fader coroutine to start before this one ends
        }
        else
        {
            // Load instantly if no fader
            SceneManager.LoadScene(_mainMenuSceneName);
            yield break; // Exit coroutine after instant load
        }
    }
}