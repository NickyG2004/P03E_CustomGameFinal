// -----------------------------------------------------------------------------
// MainMenuEvents.cs
// -----------------------------------------------------------------------------
// Handles the main menu UI logic: Start, Continue, (Settings), Credits, and Quit.
// Binds UI Toolkit buttons to their corresponding callbacks and plays click SFX.
// Integrates with SaveManager for persistent level data and ScreenFader for smooth
// scene transitions.
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuEvents : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Enable debug logs for menu actions")]
    public bool debugMode = true;

    [Header("Battle Scene Settings")]
    [Tooltip("Name of the battle scene to load")]
    [SerializeField] private string _battleSceneName;
    [Tooltip("Audio clip to play when entering battle")]
    [SerializeField] private AudioClip _levelSong;

    [Header("Menu Music")]
    [Tooltip("Background music to play in the main menu")]
    [SerializeField] private AudioClip _menuMusicClip;
    [Tooltip("Fade time for menu music fade-in")]
    [SerializeField] private float _menuMusicFadeTime = 1f;

    // References to UI Document and AudioSource components
    private UIDocument _document;
    private AudioSource _audioSource;
    private ScreenFader _fader;

    // Root visual containers for different sub-menus
    private VisualElement _rootMenu;
    private VisualElement _settingsMenu;
    private VisualElement _creditsMenu;

    // Individual button references
    private Button _startButton;
    private Button _continueButton;
    // private Button _settingsButton;    // Uncomment when settings menu entry is needed
    private Button _settingsBackButton;
    private Button _quitButton;
    private Button _creditsButton;
    private Button _creditsBackButton;

    // List to hold all buttons for universal click SFX binding
    private readonly List<Button> _allButtons = new List<Button>();

    /// <summary>
    /// Called once when the script instance wakes up.
    /// Grabs necessary components and binds all menu buttons.
    /// </summary>
    private void Awake()
    {
        // Cache component references
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();
        _fader = FindFirstObjectByType<ScreenFader>();

        // Start menu music fade-in concurrently with screen fade
        if (_menuMusicClip != null)
        {
            MusicManager.Instance.Play(_menuMusicClip, _menuMusicFadeTime);
        }

        // Fetch root visual elements by name defined in UXML
        var root = _document.rootVisualElement;
        _rootMenu = root.Q<VisualElement>("RootMenu");
        _settingsMenu = root.Q<VisualElement>("SettingsMenu");
        _creditsMenu = root.Q<VisualElement>("CreditsMenu");

        // Bind Start button: resets save data and enters battle
        _startButton = root.Q<Button>("StartButton");
        _startButton.RegisterCallback<ClickEvent>(OnPlayGameClick);

        // Bind Continue button: only visible if a saved level exists
        _continueButton = root.Q<Button>("ContinueButton");
        if (_continueButton != null)
        {
            int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            // Show button only when level > 1
            _continueButton.style.display =
                (savedLevel > 1) ? DisplayStyle.Flex : DisplayStyle.None;
            _continueButton.RegisterCallback<ClickEvent>(OnContinueClick);
        }

        //--- Settings menu entry (commented out)
        // _settingsButton = root.Q<Button>("SettingsButton");
        // _settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);

        // Bind Settings back button: returns to main menu
        _settingsBackButton = root.Q<Button>("SettingsBackButton");
        _settingsBackButton.RegisterCallback<ClickEvent>(OnSettingsBackButtonClick);

        // Bind Quit button: exits the application
        _quitButton = root.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClick);

        // Bind Credits entry button
        _creditsButton = root.Q<Button>("CreditsButton");
        _creditsButton.RegisterCallback<ClickEvent>(OnCreditsButtonClick);

        // Bind Credits back button: returns to main menu
        _creditsBackButton = root.Q<Button>("CreditsBackButton");
        _creditsBackButton.RegisterCallback<ClickEvent>(OnCreditsBackButtonClick);

        // Universal click SFX: bind to all buttons in the visual tree
        _allButtons.AddRange(root.Query<Button>().ToList());
        foreach (var btn in _allButtons)
            btn.RegisterCallback<ClickEvent>(OnAllButtonsClick);
    }

    /// <summary>
    /// Unregisters all button callbacks when this component is disabled.
    /// </summary>
    private void OnDisable()
    {
        _startButton.UnregisterCallback<ClickEvent>(OnPlayGameClick);
        _continueButton?.UnregisterCallback<ClickEvent>(OnContinueClick);
        // _settingsButton?.UnregisterCallback<ClickEvent>(OnSettingsButtonClick);
        _settingsBackButton.UnregisterCallback<ClickEvent>(OnSettingsBackButtonClick);
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitButtonClick);
        _creditsButton.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);
        _creditsBackButton.UnregisterCallback<ClickEvent>(OnCreditsBackButtonClick);
        foreach (var btn in _allButtons)
            btn.UnregisterCallback<ClickEvent>(OnAllButtonsClick);
    }

    /// <summary>
    /// Handler for the Start Game button click.
    /// Resets player/enemy levels, plays music, and transitions to battle.
    /// </summary>
    private void OnPlayGameClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("MainMenu: Start New Game");
        PlayerPrefs.SetInt("PlayerLevel", 1);
        PlayerPrefs.SetInt("EnemyLevel", 1);
        PlayerPrefs.Save();

        MusicManager.Instance.Play(_levelSong, 3f);
        if (_fader != null)
            StartCoroutine(_fader.FadeOutAndLoad(_battleSceneName));
        else
            SceneManager.LoadScene(_battleSceneName);
    }

    /// <summary>
    /// Handler for the Continue button click.
    /// Loads battle at the saved player/enemy levels.
    /// </summary>
    private void OnContinueClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("MainMenu: Continue Game");
        MusicManager.Instance.Play(_levelSong, 3f);
        if (_fader != null)
            StartCoroutine(_fader.FadeOutAndLoad(_battleSceneName));
        else
            SceneManager.LoadScene(_battleSceneName);
    }

    //--- Settings entry (commented out)
    // private void OnSettingsButtonClick(ClickEvent evt)
    // {
    //     if (debugMode) Debug.Log("Settings Button Clicked");
    //     _rootMenu.style.display     = DisplayStyle.None;
    //     _settingsMenu.style.display = DisplayStyle.Flex;
    // }

    /// <summary>
    /// Handler for the Settings Back button click.
    /// Returns to the main menu from the settings submenu.
    /// </summary>
    private void OnSettingsBackButtonClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("Settings Back Button Clicked");
        _rootMenu.style.display = DisplayStyle.Flex;
        _settingsMenu.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Handler for the Quit button click.
    /// Exits the application.
    /// </summary>
    private void OnQuitButtonClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("Quit Button Clicked");
        Application.Quit();
    }

    /// <summary>
    /// Handler for the Credits button click.
    /// Opens the credits submenu.
    /// </summary>
    private void OnCreditsButtonClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("Credits Button Clicked");
        _rootMenu.style.display = DisplayStyle.None;
        _creditsMenu.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Handler for the Credits Back button click.
    /// Returns to the main menu from the credits submenu.
    /// </summary>
    private void OnCreditsBackButtonClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("Credits Back Button Clicked");
        _rootMenu.style.display = DisplayStyle.Flex;
        _creditsMenu.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Universal click SFX callback bound to every button.
    /// Plays the AudioSource's assigned clip.
    /// </summary>
    private void OnAllButtonsClick(ClickEvent evt)
    {
        _audioSource.Play();
    }
}
