// -----------------------------------------------------------------------------
// MainMenuEvents.cs
// -----------------------------------------------------------------------------
// Manages UI Toolkit main menu flow: Start, Continue, Quit.
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Linq;

public class MainMenuEvents : MonoBehaviour
{
    #region Serialized Fields
    [Header("Menu Settings")]
    [Tooltip("Scene name for the main battle")] public string battleSceneName;
    [Tooltip("Music to play when entering battle")] public AudioClip levelSong;
    #endregion

    #region Private Fields
    private UIDocument _uiDoc;
    private AudioSource _buttonAudio;
    private ScreenFader _screenFader;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Cache components and bind callbacks
        _uiDoc = GetComponent<UIDocument>();
        _buttonAudio = GetComponent<AudioSource>();
        _screenFader = FindFirstObjectByType<ScreenFader>();
        BindMenuButtons();
    }
    #endregion

    #region Button Binding
    private void BindMenuButtons()
    {
        var root = _uiDoc.rootVisualElement;
        var startBtn = root.Q<Button>("StartButton");
        var continueBtn = root.Q<Button>("ContinueButton");
        var quitBtn = root.Q<Button>("QuitButton");

        // Start New Game
        startBtn.RegisterCallback<ClickEvent>(evt => {
            SaveManager.PlayerLevel = 1;
            SaveManager.EnemyLevel = 1;
            OnEnterBattle();
        });

        // Continue if saved data exists
        if (SaveManager.PlayerLevel > 1)
        {
            continueBtn.style.display = DisplayStyle.Flex;
            continueBtn.RegisterCallback<ClickEvent>(evt => OnEnterBattle());
        }

        // Quit Application
        quitBtn.RegisterCallback<ClickEvent>(evt => Application.Quit());

        // Play click SFX via AudioManager
        var buttons = root.Query<Button>().Build();  // now returns List<Button>
        foreach (var btn in buttons)
            btn.RegisterCallback<ClickEvent>(evt =>
                AudioManager.Instance.PlaySFX(_buttonAudio.clip));
    }
    #endregion

    #region Helpers
    private void OnEnterBattle()
    {
        MusicManager.Instance.Play(levelSong, 3f);
        if (_screenFader != null)
            StartCoroutine(_screenFader.FadeOutAndLoad(battleSceneName));
        else
            SceneManager.LoadScene(battleSceneName);
    }
    #endregion
}