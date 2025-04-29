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

/// <summary>
/// Represents the state of the battle progression.
/// </summary>
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, NEXTMENU }

public class BattleSystemRefactored : MonoBehaviour
{
    #region Serialized Fields
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
        // Begin the battle flow
        StartBattle();
    }
    #endregion

    #region Public API
    /// <summary>Initiate a new battle: resets state and begins setup coroutine.</summary>
    public void StartBattle()
    {
        state = BattleState.START;
        StartCoroutine(SetUpBattle());
    }

    /// <summary> Precents text on the players.</summary>
    public void PlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        dialogueText.text = "Choose an action:";
    }


    /// <summary>Called to end the battle: triggers fade & appropriate menu.</summary>
    public void EndBattle()
    {
        // Decide which menu to show
        Action showMenu = state == BattleState.WON
            ? _menuSystem.ShowWinMenu
            : _menuSystem.ShowLoseMenu;

        // Update persistent level for player win
        if (state == BattleState.WON)
        {
            SaveManager.PlayerLevel = playerUnit.unitLevel;
            SaveManager.EnemyLevel = enemyUnit.unitLevel;
        }

        // Start fade sequence then menu display
        StartCoroutine(FadeToMenu(showMenu));
    }
    #endregion

    #region Coroutines
    private IEnumerator SetUpBattle()
    {
        // Instantiate and initialize player
        var pGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = pGO.GetComponent<UnitRefactored>();
        playerUnit.InitializeLevel(_savedPlayerLevel);

        // Instantiate and initialize enemy
        var eGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = eGO.GetComponent<UnitRefactored>();
        enemyUnit.InitializeLevel(_savedEnemyLevel);

        // Update HUDs and intro text
        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);
        yield return ShowDialog($"A wild {enemyUnit.unitName} appeared!");

        // Begin player turn
        state = BattleState.PLAYERTURN;
        ShowDialog("Choose an action:");
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
    /// <summary>Handles the player attack action and transitions.</summary>
    public IEnumerator PlayerAttack()
    {
        // Damage enemy
        bool enemyDead = enemyUnit.TakeDamage(playerUnit.damage);
        enemyHUD.SetHP(enemyUnit.currentHP);
        yield return ShowDialog("Attack successful!");

        if (enemyDead)
        {
            state = BattleState.WON;
            yield return new WaitForSeconds(turnDelay);
            EndBattle();
        }
        else
        {
            state = BattleState.ENEMYTURN;
            yield return EnemyTurn();
        }
    }

    /// <summary>Handles the player heal action and transitions.</summary>
    public IEnumerator PlayerHeal()
    {
        yield return ShowDialog("Healing...");
        playerUnit.Heal(healAmount);
        playerHUD.SetHP(playerUnit.currentHP);
        yield return ShowDialog($"Healed for {healAmount} HP!");
        state = BattleState.ENEMYTURN;
        yield return EnemyTurn();
    }

    /// <summary>Enemy's automated turn behavior.</summary>
    private IEnumerator EnemyTurn()
    {
        yield return ShowDialog($"{enemyUnit.unitName} attacks!");
        bool playerDead = playerUnit.TakeDamage(enemyUnit.damage);
        playerHUD.SetHP(playerUnit.currentHP);

        if (playerDead)
        {
            state = BattleState.LOST;
            yield return new WaitForSeconds(turnDelay);
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            yield return ShowDialog("Your turn!");
        }
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
    #endregion
}
