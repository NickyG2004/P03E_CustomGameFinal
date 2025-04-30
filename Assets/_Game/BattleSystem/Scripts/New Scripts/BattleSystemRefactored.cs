// -----------------------------------------------------------------------------
// BattleSystemRefactored.cs
// -----------------------------------------------------------------------------
// Core controller for the turn-based battle: handles instantiation of units,
// turn sequencing (player, enemy), win/lose detection, level scaling,
// and smooth transitions to win/lose menus using ScreenFader & MenuSystemRefactored.
// Integrated with SaveManager for persistent player/enemy levels.
// -----------------------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents the state of the battle progression.
/// </summary>
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, NEXTMENU }

public class BattleSystemRefactored : MonoBehaviour
{
    #region Serialized Fields
    [Header("References to Helpers")]
    public BattleActionsRefactored battleActions;  // drag in from Inspector

    [Header("Prefabs & Stations")]
    [Tooltip("Player unit prefab")]
    [SerializeField] private GameObject playerPrefab;
    [Tooltip("Enemy unit prefab")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Transform where player appears")]
    [SerializeField] private Transform playerBattleStation;
    [Tooltip("Transform where enemy appears")]
    [SerializeField] private Transform enemyBattleStation;

    [Header("UI References")]
    [Tooltip("Dialogue text for battle messages")]
    [SerializeField] public TextMeshProUGUI dialogueText;
    [Tooltip("Player HUD reference")]
    [SerializeField] public BattleHUDRefactored playerHUD;
    [Tooltip("Enemy HUD reference")]
    [SerializeField] public BattleHUDRefactored enemyHUD;


    [Header("Player Action Buttons")]
    [SerializeField] public Button attackButton;  // drag your Attack button here
    [SerializeField] public Button healButton;    // drag your Heal button here
    // [SerializeField] public Button defendButton;     // future Defend option

    [Header("Heal Settings (per player level)")]
    [Tooltip("Minimum heal = floor(level * this)")]
    public float healMinMultiplier = 0.5f;
    [Tooltip("Maximum heal = ceil(level * this)")]
    public float healMaxMultiplier = 1.5f;

    [Header("Damage Settings (per level)")]
    public float damageMinMultiplier = 0.8f;
    public float damageMaxMultiplier = 1.2f;

    [Header("Critical Hits")]
    [Range(0, 1)]
    public float critChance = 0.1f;
    public float critMultiplier = 1.5f;

    [Header("Battle Timers & Values")]
    [Tooltip("Delay before battle starts")]
    [SerializeField] public float battleStartDelay = 2f;
    [Tooltip("Delay between turns")]
    [SerializeField] public float turnDelay = 2f;
    [Tooltip("Delay for button feedback")]
    [SerializeField] public float buttonDelay = 2f;
    [Tooltip("Amount healed on heal action")]
    [SerializeField] public int healAmount = 10;
    [Tooltip("Levels gained on win")]
    [SerializeField] public int levelUpAmount = 1;

    [Header("End-Battle Fade")]
    [Tooltip("Duration to fade to black and back when showing menus")]
    [SerializeField] private float endFadeDuration = 1f;
    #endregion

    #region Public Properties
    /// <summary>Current battle state (player/enemy turn, win, lose).</summary>
    public BattleState state { get; set; }

    /// <summary>Reference to the player Unit instance.</summary>
    public UnitRefactored playerUnit { get; private set; }

    /// <summary>Reference to the enemy Unit instance.</summary>
    public UnitRefactored enemyUnit { get; private set; }
    #endregion

    #region Private Fields
    private MenuSystemRefactored _menuSystem;
    private ScreenFader _screenFader;
    private int _savedPlayerLevel;
    private int _savedEnemyLevel;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        // Load persistent levels (default to 1)
        _savedPlayerLevel = SaveManager.PlayerLevel;
        _savedEnemyLevel = SaveManager.EnemyLevel;

        // Cache MenuSystem and ScreenFader (handles inactive)
        _menuSystem = FindFirstObjectByType<MenuSystemRefactored>(FindObjectsInactive.Include);
        _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        // Initial battle entry on scene load:
        StartCoroutine(InitialBattle());
    }
    #endregion

    #region Public API

    #endregion

    #region Coroutines
    public IEnumerator SetUpBattle()
    {
        // Instantiate and initialize player
        var pGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = pGO.GetComponent<UnitRefactored>();
        playerUnit.InitializeLevel(SaveManager.PlayerLevel);

        // Instantiate and initialize enemy
        var eGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = eGO.GetComponent<UnitRefactored>();

        // 1) Get the player’s current level
        int playerLvl = SaveManager.PlayerLevel;

        // 2) Pick a random offset of -1, 0, +1, or +2
        //    Random.Range(0,4) returns {0,1,2,3}; subtract 1 -> {-1,0,1,2}
        int diff = UnityEngine.Random.Range(0, 4) - 1;

        // 3) Compute enemy level, clamped so it never drops below 1
        int enemyLvl = Mathf.Max(1, playerLvl + diff);

        // 4) Apply it to the enemy and update the saved value
        enemyUnit.InitializeLevel(enemyLvl);
        SaveManager.EnemyLevel = enemyLvl;

        // Update HUDs and intro text
        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        yield return ShowDialog($"A wild {enemyUnit.unitName} appeared!");

    }

    private IEnumerator FadeToMenu(Action showMenu)
    {
        if (_screenFader != null)
        {
            // Enable and block input
            _screenFader.gameObject.SetActive(true);
            var cg = _screenFader.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            // Fade to black
            yield return UIFader.FadeInCanvasGroup(cg, endFadeDuration);

            // Invoke menu UI
            showMenu();

            // Fade back in
            yield return UIFader.FadeOutCanvasGroup(cg, endFadeDuration);

            // Unblock and hide fader
            cg.blocksRaycasts = false;
            _screenFader.gameObject.SetActive(false);
        }
        else
        {
            showMenu();
        }
    }
    #endregion

    #region Battle Flow Coroutines
    public IEnumerator InitialBattle()
    {
        yield return SetUpBattle();
        yield return StartCoroutine(StartBattle());
    }

    /// <summary>Initiate a new battle: resets state and begins setup coroutine.</summary>
    public IEnumerator StartBattle()
    {
        state = BattleState.START;   // if you still want that flag

        // tiny buffer so the player can read “A wild X appeared!”
        yield return new WaitForSeconds(battleStartDelay);

        if (playerUnit.Speed >= enemyUnit.Speed)
            yield return StartCoroutine(PlayerTurn());
        else
            yield return StartCoroutine(EnemyTurn());
    }

    /// <summary>/// Handles the player’s turn: enables action buttons and waits for input./// </summary>
    public IEnumerator PlayerTurn()
    {
        // 1) Set the state
        state = BattleState.PLAYERTURN;

        // 2) Update the dialog
        dialogueText.text = "Choose an action:";

        // 3) Re-enable the action buttons
        SetActionButtonsInteractable(true);

        // 4) End the coroutine immediately
        yield break;
    }

    public IEnumerator EnemyTurn()
    {
        // 1) Switch state & disable player buttons
        state = BattleState.ENEMYTURN;
        SetActionButtonsInteractable(false);

        // 2) Tell the player it’s the enemy’s turn
        dialogueText.text = $"{enemyUnit.unitName}'s turn!";
        yield return new WaitForSeconds(turnDelay);

        // 3) Delegate the actual attack
        yield return StartCoroutine(battleActions.EnemyAttack());

        // 4) Check for player defeat
        if (playerUnit.currentHP <= 0)
        {
            state = BattleState.LOST;
            yield return StartCoroutine(EndBattle());
            yield break;
        }

        // 5) Small pause before giving back control
        yield return new WaitForSeconds(turnDelay);

        // 6) Your turn prompt
        dialogueText.text = "Your turn!";
        yield return new WaitForSeconds(turnDelay);

        // 7) Hand back to player
        yield return StartCoroutine(PlayerTurn());
    }

    /// <summary>Called to end the battle: triggers fade & appropriate menu.</summary>
    public IEnumerator EndBattle()
    {
        // Decide which menu to show after fade
        Action showMenu = state == BattleState.WON
            ? _menuSystem.ShowWinMenu
            : _menuSystem.ShowLoseMenu;

        // 1) Show win/lose text
        dialogueText.text = state == BattleState.WON
            ? "You Win!"
            : "You Lose!";

        // 2) Pause so the player sees it
        yield return new WaitForSeconds(turnDelay);

        // 3) On win, bump the player’s level, update HUDs, and save
        if (state == BattleState.WON)
        {
            playerUnit.LevelUp(levelUpAmount);
            playerHUD.SetHUD(playerUnit);
            enemyHUD.SetHUD(enemyUnit);
            SaveManager.PlayerLevel = playerUnit.Level;
        }

        // 4) Fade out and then show the menu, and wait for it to finish
        yield return StartCoroutine(FadeToMenu(showMenu));

        // 5) (Optional) explicit end
        yield break;
    }

    #endregion

    #region Helper Methods
    /// <summary>
    /// Displays a dialogue message and waits a standard delay.
    /// </summary>
    private IEnumerator ShowDialog(string message)
    {
        dialogueText.text = message;
        yield return new WaitForSeconds(buttonDelay);
    }

    /// <summary>
    /// Enable/disable all action buttons at once.
    /// </summary>
    public void SetActionButtonsInteractable(bool enabled)
    {
        attackButton.interactable = enabled;
        healButton.interactable = enabled;
        //defendButton.interactable = enabled;

        // if using a list:
        // foreach (var btn in actionButtons)
        //     btn.interactable = enabled;
    }
    #endregion
}
