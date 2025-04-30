// -----------------------------------------------------------------------------
// BattleSystemRefactored.cs
// -----------------------------------------------------------------------------
// Core controller for the turn-based battle: handles instantiation of units,
// turn sequencing, win/lose detection, level scaling, and smooth transitions
// to win/lose menus using ScreenFader & MenuSystemRefactored.
// Integrated with SaveManager for persistent player/enemy levels.
// -----------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the main battle loop: spawning units, managing turns, and handling
/// win/lose flow with UI transitions.
/// </summary>
public class BattleSystemRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------
    [Header("References")]
    [SerializeField, Tooltip("Battle actions handler")]
    private BattleActionsRefactored _battleActions;

    [Header("Prefabs & Spawn Stations")]
    [SerializeField, Tooltip("Player unit prefab")]
    private GameObject _playerPrefab;
    [SerializeField, Tooltip("Enemy unit prefab")]
    private GameObject _enemyPrefab;
    [SerializeField, Tooltip("Transform where player appears")]
    private Transform _playerBattleStation;
    [SerializeField, Tooltip("Transform where enemy appears")]
    private Transform _enemyBattleStation;

    [Header("UI References")]
    [SerializeField, Tooltip("Dialogue text for battle messages")]
    private TextMeshProUGUI _dialogueText;
    [SerializeField, Tooltip("Player HUD reference")]
    private BattleHUDRefactored _playerHUD;
    [SerializeField, Tooltip("Enemy HUD reference")]
    private BattleHUDRefactored _enemyHUD;

    [Header("Action Buttons")]
    [SerializeField, Tooltip("Attack action button")]
    private Button _attackButton;
    [SerializeField, Tooltip("Heal action button")]
    private Button _healButton;
    // TODO: [SerializeField] private Button _defendButton;        // placeholder for future defend action

    [Header("Heal Settings")]
    [SerializeField, Tooltip("Minimum heal multiplier per level")]
    private float _healMinMultiplier = 0.5f;
    [SerializeField, Tooltip("Maximum heal multiplier per level")]
    private float _healMaxMultiplier = 1.5f;

    [Header("Damage Settings")]
    [SerializeField, Tooltip("Minimum damage multiplier per level")]
    private float _damageMinMultiplier = 0.8f;
    [SerializeField, Tooltip("Maximum damage multiplier per level")]
    private float _damageMaxMultiplier = 1.2f;

    [Header("Critical Hits")]
    [SerializeField, Range(0f, 1f), Tooltip("Chance to land a critical hit")]
    private float _critChance = 0.1f;
    [SerializeField, Tooltip("Damage multiplier for critical hits")]
    private float _critMultiplier = 1.5f;

    [Header("Timing")]
    [SerializeField, Tooltip("Delay before battle begins")]
    private float _battleStartDelay = 2f;
    [SerializeField, Tooltip("Delay between turns")]
    private float _turnDelay = 2f;
    [SerializeField, Tooltip("Delay for button feedback and dialog display")]
    private float _buttonDelay = 2f;

    [Header("Level & Heal Values")]
    [SerializeField, Tooltip("Fixed heal amount (if used)")]
    private int _healAmount = 10;
    [SerializeField, Tooltip("Levels gained on win")]
    private int _levelUpAmount = 1;

    [Header("End Battle Fade")]
    [SerializeField, Tooltip("Duration to fade screens at battle end")]
    private float _endFadeDuration = 1f;

    // -------------------------------------------------------------------------
    // Public Properties
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets the current battle state.
    /// </summary>
    public BattleState State { get; set; }

    /// <summary>
    /// Gets the current player unit instance.
    /// </summary>
    public UnitRefactored PlayerUnit { get; private set; }

    /// <summary>
    /// Gets the current enemy unit instance.
    /// </summary>
    public UnitRefactored EnemyUnit { get; private set; }

    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    private MenuSystemRefactored _menuSystem;
    private ScreenFader _screenFader;
    private int _savedPlayerLevel;
    private int _savedEnemyLevel;

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------
    private void Awake()
    {
        // Load saved levels or default to 1
        _savedPlayerLevel = SaveManager.PlayerLevel;
        _savedEnemyLevel = SaveManager.EnemyLevel;
        // Cache systems, including inactive objects
        _menuSystem = FindFirstObjectByType<MenuSystemRefactored>(FindObjectsInactive.Include);
        _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        // Kick off the initial battle sequence
        StartCoroutine(InitialBattleRoutine());
    }

    // -------------------------------------------------------------------------
    // Battle Coroutines (Pass 3: Extracted Helpers)
    // -------------------------------------------------------------------------
    /// <summary>
    /// Sets up units, applies levels, and shows appearance dialog.
    /// </summary>
    private IEnumerator SetupBattleRoutine()
    {
        InstantiateAndInitPlayer();
        InstantiateAndInitEnemy();
        UpdateAllHUDs();
        yield return ShowDialogRoutine("A wild " + EnemyUnit.UnitName + " appeared!");
    }

    /// <summary>
    /// Begins the initial battle and determines turn order.
    /// </summary>
    private IEnumerator InitialBattleRoutine()
    {
        yield return SetupBattleRoutine();
        yield return StartBattleRoutine();
    }

    /// <summary>
    /// Determines turn order based on speed and starts the correct turn.
    /// </summary>
    private IEnumerator StartBattleRoutine()
    {
        State = BattleState.START;
        yield return new WaitForSeconds(_battleStartDelay);
        yield return (PlayerUnit.Speed >= EnemyUnit.Speed)
            ? PlayerTurnRoutine()
            : EnemyTurnRoutine();
    }

    /// <summary>
    /// Handles the player's turn: UI prompt and await input.
    /// </summary>
    private IEnumerator PlayerTurnRoutine()
    {
        State = BattleState.PLAYERTURN;
        _dialogueText.text = "Choose an action:";
        SetActionButtonsInteractable(true);
        yield break;
    }

    /// <summary>
    /// Handles the enemy's turn: attack and win/loss checks.
    /// </summary>
    private IEnumerator EnemyTurnRoutine()
    {
        State = BattleState.ENEMYTURN;
        SetActionButtonsInteractable(false);
        _dialogueText.text = EnemyUnit.UnitName + "'s turn!";
        yield return new WaitForSeconds(_turnDelay);
        yield return _battleActions.EnemyAttackRoutine();
        if (CheckDefeatAndProcess()) yield break;
        yield return new WaitForSeconds(_turnDelay);
        _dialogueText.text = "Your turn!";
        yield return new WaitForSeconds(_turnDelay);
        yield return PlayerTurnRoutine();
    }

    /// <summary>
    /// Ends the battle: shows result, processes outcomes, and fades to menu.
    /// </summary>
    private IEnumerator EndBattleRoutine()
    {
        ShowResultDialog();
        yield return new WaitForSeconds(_turnDelay);
        ProcessPostBattle();
        yield return FadeToMenuRoutine(GetMenuAction());
    }

    // -------------------------------------------------------------------------
    // Refactored Helper Methods
    // -------------------------------------------------------------------------
    private void InstantiateAndInitPlayer()
    {
        var playerGO = Instantiate(_playerPrefab, _playerBattleStation);
        PlayerUnit = playerGO.GetComponent<UnitRefactored>();
        PlayerUnit.InitializeLevel(_savedPlayerLevel);
    }

    private void InstantiateAndInitEnemy()
    {
        var enemyGO = Instantiate(_enemyPrefab, _enemyBattleStation);
        EnemyUnit = enemyGO.GetComponent<UnitRefactored>();
        int enemyLevel = CalculateEnemyLevel(_savedPlayerLevel);
        EnemyUnit.InitializeLevel(enemyLevel);
        SaveManager.EnemyLevel = enemyLevel;
    }

    private int CalculateEnemyLevel(int playerLevel)
    {
        int offset = UnityEngine.Random.Range(0, 4) - 1; // -1 to +2
        return Mathf.Max(1, playerLevel + offset);
    }

    private void UpdateAllHUDs()
    {
        _playerHUD.SetHUD(PlayerUnit);
        _enemyHUD.SetHUD(EnemyUnit);
    }

    private bool CheckDefeatAndProcess()
    {
        if (PlayerUnit.CurrentHP > 0) return false;
        State = BattleState.LOST;
        StartCoroutine(EndBattleRoutine());
        return true;
    }

    private void ShowResultDialog()
    {
        _dialogueText.text = State == BattleState.WON ? "You Win!" : "You Lose!";
    }

    private void ProcessPostBattle()
    {
        if (State != BattleState.WON) return;
        PlayerUnit.LevelUp(_levelUpAmount);
        _playerHUD.SetHUD(PlayerUnit);
        SaveManager.PlayerLevel = PlayerUnit.Level;
    }

    private Action GetMenuAction()
    {
        return State == BattleState.WON
            ? _menuSystem.ShowWinMenu
            : _menuSystem.ShowLoseMenu;
    }

    /// <summary>
    /// Handles screen fade to a menu and back.
    /// </summary>
    private IEnumerator FadeToMenuRoutine(Action showMenu)
    {
        if (_screenFader == null)
        {
            showMenu();
            yield break;
        }
        var cg = _screenFader.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        _screenFader.gameObject.SetActive(true);
        yield return UIFader.FadeInCanvasGroup(cg, _endFadeDuration);
        showMenu();
        yield return UIFader.FadeOutCanvasGroup(cg, _endFadeDuration);
        cg.blocksRaycasts = false;
        _screenFader.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays a message for a set duration.
    /// </summary>
    private IEnumerator ShowDialogRoutine(string message)
    {
        _dialogueText.text = message;
        yield return new WaitForSeconds(_buttonDelay);
    }

    /// <summary>
    /// Toggles the interactability of all action buttons.
    /// </summary>
    private void SetActionButtonsInteractable(bool enable)
    {
        _attackButton.interactable = enable;
        _healButton.interactable = enable;
    }
}
