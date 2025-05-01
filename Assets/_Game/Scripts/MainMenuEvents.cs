// -----------------------------------------------------------------------------
// Filename: MainMenuEvents.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Handles UI logic and event binding for the main menu using UI Toolkit.
// Manages navigation between main menu, submenus (Credits, Settings - Placeholder),
// starting/continuing the game, playing music/SFX via MusicManager/AudioSource,
// interacting with SaveManager, and transitioning scenes via ScreenFader.
// Requires UIDocument and AudioSource components.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System; // Required for Action<ClickEvent>

/// <summary>
/// Handles UI logic and event binding for the main menu using UI Toolkit.
/// Manages navigation, game start/continue, music/SFX, and quitting.
/// </summary>
[RequireComponent(typeof(UIDocument), typeof(AudioSource))]
public class MainMenuEvents : MonoBehaviour
{
    #region Inspector Fields

    [Header("Configuration")]
    [Tooltip("Name of the battle scene asset to load.")]
    [SerializeField] private string _battleSceneName = "BattleScene"; // Sensible default

    [Tooltip("Default audio clip to play when starting/continuing the battle scene transition.")]
    [SerializeField] private AudioClip _levelSong;

    [Tooltip("Background music clip for the main menu.")]
    [SerializeField] private AudioClip _menuMusicClip;

    [Tooltip("Duration (in seconds) for menu music fade-in.")]
    [SerializeField, Range(0.1f, 5f)] private float _menuMusicFadeTime = 1.0f;

    [Header("Debug")]
    [Tooltip("Enable detailed logs for menu actions.")]
    [SerializeField] private bool _debugMode = false; // Default false is usually better

    #endregion

    #region Private Fields

    // Component References (Set in Awake/OnEnable)
    private UIDocument _document;
    private AudioSource _audioSource; // Used for button SFX
    private ScreenFader _screenFader; // Optional component for transitions

    // UI Toolkit Element References (Queried in OnEnable)
    private VisualElement _rootVisualElement;
    private VisualElement _rootMenu;
    private VisualElement _settingsMenu; // Placeholder for future
    private VisualElement _creditsMenu;

    // Button References (Cached for UnregisterCallback)
    private Button _startButton;
    private Button _continueButton;
    // private Button _settingsButton; // TODO: Uncomment and implement when settings menu is ready
    private Button _settingsBackButton;
    private Button _quitButton;
    private Button _creditsButton;
    private Button _creditsBackButton;

    // List for managing universal SFX binding/unbinding
    private readonly List<Button> _allBoundButtons = new List<Button>();

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        // Cache required components attached to this GameObject
        _document = GetComponent<UIDocument>();
        _audioSource = GetComponent<AudioSource>();

        // Attempt to find the optional ScreenFader anywhere in the scene
        _screenFader = FindFirstObjectByType<ScreenFader>();
        if (_screenFader == null && _debugMode)
        {
            Debug.Log("[MainMenuEvents] ScreenFader component not found in scene. Scene transitions will be instant.", this);
        }
    }

    private void OnEnable()
    {
        // Query UI elements and bind events *after* Awake ensures UIDocument is ready
        _rootVisualElement = _document?.rootVisualElement;
        if (_rootVisualElement == null)
        {
            Debug.LogError("[MainMenuEvents] Could not find RootVisualElement in UIDocument! Disabling script.", this);
            this.enabled = false; // Prevent further errors
            return;
        }

        QueryUIElements();      // Find all relevant VisualElements and Buttons
        BindButtonCallbacks();  // Register onClick events
        SetupInitialState();    // Set initial visibility (e.g., Continue button)

        PlayMenuMusic();        // Start menu background music
    }

    private void OnDisable()
    {
        // Crucial to unregister callbacks to prevent errors and memory leaks
        UnbindButtonCallbacks();
        _allBoundButtons.Clear(); // Clear the cache
    }

    #endregion

    #region UI Setup and Binding Helpers

    /// <summary> Queries and caches references to essential visual elements and buttons. </summary>
    private void QueryUIElements()
    {
        if (_rootVisualElement == null) return; // Already logged error in OnEnable

        // Query menu containers
        _rootMenu = _rootVisualElement.Q<VisualElement>("RootMenu");
        _settingsMenu = _rootVisualElement.Q<VisualElement>("SettingsMenu"); // May be null if not implemented
        _creditsMenu = _rootVisualElement.Q<VisualElement>("CreditsMenu"); // May be null

        // Validate essential container
        if (_rootMenu == null) Debug.LogError("[MainMenuEvents] Required 'RootMenu' VisualElement not found in UXML!", this);

        // Query specific buttons
        _startButton = _rootVisualElement.Q<Button>("StartButton");
        _continueButton = _rootVisualElement.Q<Button>("ContinueButton");
        // _settingsButton = _rootVisualElement.Q<Button>("SettingsButton"); // TODO
        _settingsBackButton = _rootVisualElement.Q<Button>("SettingsBackButton");
        _quitButton = _rootVisualElement.Q<Button>("QuitButton");
        _creditsButton = _rootVisualElement.Q<Button>("CreditsButton");
        _creditsBackButton = _rootVisualElement.Q<Button>("CreditsBackButton");

        // Log warnings for missing standard buttons (helps UI debugging)
        if (_startButton == null) Debug.LogWarning("[MainMenuEvents] 'StartButton' not found in UXML.", this);
        if (_quitButton == null) Debug.LogWarning("[MainMenuEvents] 'QuitButton' not found in UXML.", this);
    }

    /// <summary> Registers callbacks for specific buttons and a universal SFX callback for all buttons. </summary>
    private void BindButtonCallbacks()
    {
        // Bind specific actions using the helper
        BindButtonAction(_startButton, OnPlayGameClick);
        BindButtonAction(_continueButton, OnContinueClick);
        // BindButtonAction(_settingsButton, OnSettingsButtonClick); // TODO
        BindButtonAction(_settingsBackButton, OnSettingsBackButtonClick);
        BindButtonAction(_quitButton, OnQuitButtonClick);
        BindButtonAction(_creditsButton, OnCreditsButtonClick);
        BindButtonAction(_creditsBackButton, OnCreditsBackButtonClick);

        // Bind universal SFX to all buttons found and bound above
        if (_audioSource != null && _audioSource.clip != null) // Check source AND clip
        {
            foreach (var btn in _allBoundButtons)
            {
                // Avoid double-registering if a button somehow existed without being bound specifically
                // (Though Unregister takes care of this)
                btn?.RegisterCallback<ClickEvent>(OnAnyButtonClickSFX);
            }
        }
        else if (_debugMode)
        {
            if (_audioSource == null) Debug.LogWarning("[MainMenuEvents] AudioSource component missing, universal button click SFX disabled.", this);
            else if (_audioSource.clip == null) Debug.LogWarning("[MainMenuEvents] AudioSource component has no AudioClip assigned, universal button click SFX disabled.", this);
        }
    }

    /// <summary> Unregisters all specific actions and universal SFX callbacks. </summary>
    private void UnbindButtonCallbacks()
    {
        // Use the cached list to unregister everything
        foreach (var btn in _allBoundButtons)
        {
            if (btn == null) continue; // Safety check

            // Unregister specific callbacks (safe even if not registered to this specific callback)
            // Note: A button should ideally only have ONE action callback + SFX callback.
            // This is slightly overkill but ensures cleanup if structure changes.
            btn.UnregisterCallback<ClickEvent>(OnPlayGameClick);
            btn.UnregisterCallback<ClickEvent>(OnContinueClick);
            // btn.UnregisterCallback<ClickEvent>(OnSettingsButtonClick); // TODO
            btn.UnregisterCallback<ClickEvent>(OnSettingsBackButtonClick);
            btn.UnregisterCallback<ClickEvent>(OnQuitButtonClick);
            btn.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);
            btn.UnregisterCallback<ClickEvent>(OnCreditsBackButtonClick);

            // Unregister universal SFX
            if (_audioSource != null) // Only unregister if it was potentially registered
            {
                btn.UnregisterCallback<ClickEvent>(OnAnyButtonClickSFX);
            }
        }
    }

    /// <summary> Sets the initial visibility state of menus and the Continue button. </summary>
    private void SetupInitialState()
    {
        ShowRootMenu(); // Ensure main menu is visible first

        // Configure Continue button visibility based on saved progress
        if (_continueButton != null)
        {
            int savedLevel = SaveManager.PlayerLevel; // Uses static property
            bool canContinue = savedLevel > 1;
            SetElementVisibility(_continueButton, canContinue);

            if (_debugMode) Debug.Log($"[MainMenuEvents] Continue button {(canContinue ? "enabled" : "disabled")} (PlayerLevel={savedLevel})", this);
        }
    }

    /// <summary> Helper: Binds a specific action callback to a button and adds it to the tracking list. </summary>
    // *** CORRECTION: Change parameter type from Action<> to EventCallback<> ***
    private void BindButtonAction(Button button, EventCallback<ClickEvent> callback)
    {
        if (button != null)
        {
            // Now the 'callback' parameter is the correct type for RegisterCallback
            button.RegisterCallback<ClickEvent>(callback);

            // Add to list for easy unbinding and universal SFX binding
            if (!_allBoundButtons.Contains(button))
            {
                _allBoundButtons.Add(button);
            }
        }
        // No warning here, QueryUIElements already warns if buttons are null
    }

    #endregion

    #region UI Navigation Helpers

    /// <summary> Shows the main menu container and hides known submenus. </summary>
    private void ShowRootMenu()
    {
        SetElementVisibility(_rootMenu, true);
        SetElementVisibility(_settingsMenu, false); // Hide settings if it exists
        SetElementVisibility(_creditsMenu, false);  // Hide credits if it exists
    }

    /// <summary> Shows a specific submenu container and hides the main menu. </summary>
    private void ShowSubMenu(VisualElement subMenuToShow)
    {
        if (subMenuToShow == null)
        {
            Debug.LogError("[MainMenuEvents] Attempted to show a null submenu container!", this);
            ShowRootMenu(); // Default to showing root menu as a fallback
            return;
        }
        SetElementVisibility(_rootMenu, false); // Hide main menu

        // Show only the target submenu
        SetElementVisibility(_settingsMenu, subMenuToShow == _settingsMenu);
        SetElementVisibility(_creditsMenu, subMenuToShow == _creditsMenu);
    }

    /// <summary> Safely sets the display style of a VisualElement (Flex for visible, None for hidden). </summary>
    private void SetElementVisibility(VisualElement element, bool visible)
    {
        if (element != null)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    #endregion

    #region Button Event Handlers

    /// <summary> Handles the "Start New Game" button click: Resets progress, plays music, loads battle scene. </summary>
    private void OnPlayGameClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log("[MainMenuEvents] 'Start New Game' button clicked.", this);

        // Reset progress using SaveManager static class
        SaveManager.PlayerLevel = 4;
        SaveManager.EnemyLevel = 1;
        // Note: SaveManager properties handle PlayerPrefs.Save() internally

        PlayLevelMusic();           // Start transition music
        StartSceneLoad(_battleSceneName); // Load the scene (with fade if possible)
    }

    /// <summary> Handles the "Continue" button click: Plays music, loads battle scene with existing progress. </summary>
    private void OnContinueClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log($"[MainMenuEvents] 'Continue' button clicked. Loading level {SaveManager.PlayerLevel}.", this);

        // Progress is already saved, just transition
        PlayLevelMusic();
        StartSceneLoad(_battleSceneName);
    }

    // TODO: Implement Settings Menu button action when feature is added
    // private void OnSettingsButtonClick(ClickEvent evt)
    // {
    //     if (_debugMode) Debug.Log("[MainMenuEvents] 'Settings' button clicked.", this);
    //     ShowSubMenu(_settingsMenu);
    // }

    /// <summary> Handles the "Back" button from the Settings menu. </summary>
    private void OnSettingsBackButtonClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log("[MainMenuEvents] 'Settings Back' button clicked.", this);
        ShowRootMenu(); // Return to main menu
    }

    /// <summary> Handles the "Quit Game" button click. </summary>
    private void OnQuitButtonClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log("[MainMenuEvents] 'Quit' button clicked.", this);

        // Standard way to close the application
        Application.Quit();

        // Add a specific way to stop play mode in the Unity Editor for convenience
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary> Handles the "Credits" button click. </summary>
    private void OnCreditsButtonClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log("[MainMenuEvents] 'Credits' button clicked.", this);
        ShowSubMenu(_creditsMenu); // Show the credits panel
    }

    /// <summary> Handles the "Back" button from the Credits menu. </summary>
    private void OnCreditsBackButtonClick(ClickEvent evt)
    {
        if (_debugMode) Debug.Log("[MainMenuEvents] 'Credits Back' button clicked.", this);
        ShowRootMenu(); // Return to main menu
    }

    /// <summary> Universal callback for playing SFX on any button click that had an action bound. </summary>
    private void OnAnyButtonClickSFX(ClickEvent evt)
    {
        // Use PlayOneShot for SFX to allow overlapping sounds if clicked quickly
        if (_audioSource != null && _audioSource.clip != null)
        {
            _audioSource.PlayOneShot(_audioSource.clip);
        }
        // No debug log by default for SFX, can be noisy. Add if needed.
    }

    #endregion

    #region Music and Scene Loading Logic

    /// <summary> Plays the assigned menu music using the MusicManager singleton. </summary>
    private void PlayMenuMusic()
    {
        if (_menuMusicClip != null)
        {
            // Access singleton safely using ?. in case it hasn't initialized or was destroyed
            MusicManager.Instance?.Play(_menuMusicClip, _menuMusicFadeTime);
        }
        else if (_debugMode)
        {
            Debug.LogWarning("[MainMenuEvents] Menu Music Clip (_menuMusicClip) is not assigned in the inspector. No menu music will play.", this);
        }
    }

    /// <summary> Plays the assigned level transition music using the MusicManager singleton. </summary>
    private void PlayLevelMusic()
    {
        if (_levelSong != null)
        {
            // Assuming a standard 3-second fade for level transitions, adjust if needed
            MusicManager.Instance?.Play(_levelSong, 3f);
        }
        else if (_debugMode)
        {
            Debug.LogWarning("[MainMenuEvents] Level transition song (_levelSong) is not assigned in the inspector. No transition music will play.", this);
        }
    }

    /// <summary> Initiates loading the specified scene, using the ScreenFader coroutine if available, otherwise loads directly. </summary>
    private void StartSceneLoad(string sceneName)
    {
        // Validate scene name before attempting to load
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[MainMenuEvents] Cannot load scene: Scene name is null or empty! Check Configuration in Inspector.", this);
            return;
        }

        if (_screenFader != null)
        {
            // Use the ScreenFader component for a smooth transition
            _screenFader.StartFadeOutAndLoadScene(sceneName); // Use the void method
        }
        else
        {
            // Fallback to immediate scene loading if no fader is found
            SceneManager.LoadScene(sceneName);
        }
    }

    #endregion
}