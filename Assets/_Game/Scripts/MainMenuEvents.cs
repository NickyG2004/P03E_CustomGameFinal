using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
    public bool debugMode = true;

    private UIDocument _document;

    // visual elements sub menus
    private VisualElement _rootMenu;
    private VisualElement _settingsMenu;
    private VisualElement _creditsMenu;

    private Button _startButton;
    private Button _continueButton;
    ////private Button _settingsButton;
    private Button _setingsBackButton;
    private Button _quitButton;
    private Button _creditsButton;
    private Button _creditsBackButton;

    [SerializeField] private string _starLevelName;
    [SerializeField] private AudioClip _levelSong;

    private List<Button> _menuButtons = new List<Button>();

    private AudioSource _audioSource;

    // reference to the fader in this scene
    private ScreenFader _fader;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();

        _rootMenu = _document.rootVisualElement.Q("RootMenu");
        _settingsMenu = _document.rootVisualElement.Q("SettingsMenu");
        _creditsMenu = _document.rootVisualElement.Q("CreditsMenu");

        _startButton = _document.rootVisualElement.Q("StartButton") as Button;
        _startButton.RegisterCallback<ClickEvent>(OnPlayGameClick);

        // 1) Grab the Continue button
        _continueButton = _document.rootVisualElement.Q<Button>("ContinueButton");

        // 2) Register its click and show/hide based on saved data
        if (_continueButton != null)
        {
            _continueButton.RegisterCallback<ClickEvent>(OnContinueClick);

            int savedLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            _continueButton.style.display =
                (savedLevel > 1) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        //// get the settings button and register the callback
        ////_settingsButton = _document.rootVisualElement.Q("SettingsButton") as Button;
        ////_settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);

        // get the settings back button and register the callback
        _setingsBackButton = _document.rootVisualElement.Q("SettingsBackButton") as Button;
        _setingsBackButton.RegisterCallback<ClickEvent>(OnSettingsBackButtonClick);

        // get the quit button and register the callback
        _quitButton = _document.rootVisualElement.Q("QuitButton") as Button;
        _quitButton.RegisterCallback<ClickEvent>(OnQuitButtonClick);

        // get the credits button and register the callback
        _creditsButton = _document.rootVisualElement.Q("CreditsButton") as Button;
        _creditsButton.RegisterCallback<ClickEvent>(OnCreditsButtonClick);

        // get the credits back button and register the callback
        _creditsBackButton = _document.rootVisualElement.Q("CreditsBackButton") as Button;
        _creditsBackButton.RegisterCallback<ClickEvent>(OnCreditsBackButtonClick);


        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();
        foreach (var btn in _menuButtons)
            btn.RegisterCallback<ClickEvent>(OnAllButtonsClick);

        // grab the fader once
        _fader = FindFirstObjectByType<ScreenFader>();
    }

    private void OnDisable()
    {
        _startButton.UnregisterCallback<ClickEvent>(OnPlayGameClick);
        //_settingsButton.UnregisterCallback<ClickEvent>(OnSettingsButtonClick);
        _setingsBackButton.UnregisterCallback<ClickEvent>(OnSettingsBackButtonClick);
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitButtonClick);
        _creditsButton.UnregisterCallback<ClickEvent>(OnCreditsButtonClick);
        _creditsBackButton.UnregisterCallback<ClickEvent>(OnCreditsBackButtonClick);

        // **Continue button callback**
        if (_continueButton != null)
            _continueButton.UnregisterCallback<ClickEvent>(OnContinueClick);

        // Unregister all menu buttons
        foreach (var btn in _menuButtons)
            btn.UnregisterCallback<ClickEvent>(OnAllButtonsClick);
    }

    private void OnSettingsButtonClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Settings Button Clicked");
        }
        // set the root menu to none and the settings menu to flex
        _rootMenu.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.Flex;
    }

    private void OnSettingsBackButtonClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Settings Back Button Clicked");
        }
        // set the root menu to flex and the settings menu to none
        _rootMenu.style.display = DisplayStyle.Flex;
        _settingsMenu.style.display = DisplayStyle.None;
    }

    private void OnPlayGameClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("MainMenu: Start New Game");
        // reset save
        PlayerPrefs.SetInt("PlayerLevel", 1);
        PlayerPrefs.SetInt("EnemyLevel", 1);
        PlayerPrefs.Save();

        // play your music
        MusicManager.Instance.Play(_levelSong, 3f);

        // fade then load
        if (_fader != null)
            StartCoroutine(_fader.FadeOutAndLoad(_starLevelName));
        else
            SceneManager.LoadScene(_starLevelName);
    }

    private void OnContinueClick(ClickEvent evt)
    {
        if (debugMode) Debug.Log("MainMenu: Continue Game");
        MusicManager.Instance.Play(_levelSong, 3f);

        if (_fader != null)
            StartCoroutine(_fader.FadeOutAndLoad(_starLevelName));
        else
            SceneManager.LoadScene(_starLevelName);
    }

    private void OnQuitButtonClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Quit Button Clicked");
        }
        // quit the game
        Application.Quit();
    }

    private void OnCreditsButtonClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Credits Button Clicked");
        }
        // set the root menu to none and the credits menu to flex
        _rootMenu.style.display = DisplayStyle.None;
        _creditsMenu.style.display = DisplayStyle.Flex;
    }

    private void OnCreditsBackButtonClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Credits Back Button Clicked");
        }
        // set the root menu to flex and the credits menu to none
        _rootMenu.style.display = DisplayStyle.Flex;
        _creditsMenu.style.display = DisplayStyle.None;
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        if (debugMode)
        {
            Debug.Log("MainMenuEvent: Menu Button Clicked");
        }

        _audioSource.Play();
    }
}
