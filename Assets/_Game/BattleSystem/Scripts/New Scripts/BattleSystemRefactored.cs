// -----------------------------------------------------------------------------
// Filename: BattleSystemRefactored.cs
// (Provided as Refactored Example)
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
    [SerializeField, Tooltip("Reference to the BattleActions component containing action logic.")]
    private BattleActionsRefactored _battleActions;
    [SerializeField, Tooltip("Reference to the MenuSystem that handles win/lose menus.")]
    private MenuSystemRefactored _menuSystem; // Added explicit reference
    [SerializeField, Tooltip("Optional reference to the ScreenFader for transitions.")]
    private ScreenFader _screenFader; // Added explicit reference


    [Header("Prefabs & Spawn Stations")]
    [SerializeField, Tooltip("Player unit prefab to instantiate.")]
    private GameObject _playerPrefab;
    [SerializeField, Tooltip("Enemy unit prefab to instantiate.")]
    private GameObject _enemyPrefab;
    [SerializeField, Tooltip("Transform where the player unit should be spawned.")]
    private Transform _playerBattleStation;
    [SerializeField, Tooltip("Transform where the enemy unit should be spawned.")]
    private Transform _enemyBattleStation;

    [Header("UI References")]
    [SerializeField, Tooltip("TextMeshPro UI element for displaying battle dialogue and messages.")]
    private TextMeshProUGUI _dialogueText;
    [SerializeField, Tooltip("Reference to the Player's BattleHUD component.")]
    private BattleHUDRefactored _playerHUD;
    [SerializeField, Tooltip("Reference to the Enemy's BattleHUD component.")]
    private BattleHUDRefactored _enemyHUD;

    [Header("Action Buttons")]
    [SerializeField, Tooltip("Reference to the Attack action button.")]
    private Button _attackButton;
    [SerializeField, Tooltip("Reference to the Heal action button.")]
    private Button _healButton;
    // TODO: [SerializeField] private Button _defendButton; // placeholder for future defend action

    [Header("Gameplay Settings - Healing")]
    [SerializeField, Tooltip("Minimum healing amount multiplier (based on Player Level).")]
    private float _healMinMultiplier = 0.5f;
    [SerializeField, Tooltip("Maximum healing amount multiplier (based on Player Level).")]
    private float _healMaxMultiplier = 1.5f;
    [SerializeField, Tooltip("Fixed heal amount (if multipliers are not used or for specific skills - currently unused by default heal).")]
    private int _baseHealAmount = 10; // Renamed from _healAmount for clarity

    [Header("Gameplay Settings - Damage & Crits")]
    [SerializeField, Tooltip("Minimum damage roll multiplier (applied to unit's BaseDamage).")]
    private float _damageMinMultiplier = 0.8f;
    [SerializeField, Tooltip("Maximum damage roll multiplier (applied to unit's BaseDamage).")]
    private float _damageMaxMultiplier = 1.2f;
    [SerializeField, Range(0f, 1f), Tooltip("Chance (0 to 1) for any attack to be a critical hit.")]
    private float _critChance = 0.1f;
    [SerializeField, Tooltip("Damage multiplier applied on a critical hit (e.g., 1.5 for 50% extra damage).")]
    [Min(1f)] private float _critMultiplier = 1.5f;

    [Header("Gameplay Settings - Leveling")]
    [SerializeField, Tooltip("Number of levels the player gains upon winning a battle.")]
    [Min(0)] private int _levelUpAmount = 1;
    [SerializeField, Tooltip("Minimum level offset for enemy relative to player (e.g., -1 means enemy can be 1 level lower).")]
    private int _enemyLevelMinOffset = -1;
    [SerializeField, Tooltip("Maximum level offset for enemy relative to player (e.g., 2 means enemy can be 2 levels higher).")]
    private int _enemyLevelMaxOffset = 2;

    [Header("Timing & Delays")]
    [SerializeField, Tooltip("Delay (seconds) after setup before the first turn begins.")]
    [Range(0f, 5f)] private float _battleStartDelay = 2f;
    [SerializeField, Tooltip("Standard delay (seconds) between turns or major actions.")]
    [Range(0f, 5f)] private float _turnDelay = 2f;
    [SerializeField, Tooltip("Short delay (seconds) used for UI feedback like button presses or brief messages.")]
    [Range(0f, 5f)] private float _feedbackDelay = 1.0f; // Renamed from _buttonDelay

    [Header("Transitions")]
    [SerializeField, Tooltip("Duration (seconds) for fade transitions when showing win/lose menus.")]
    [Range(0.1f, 5f)] private float _endMenuFadeDuration = 1f; // Renamed from _endFadeDuration

    // -------------------------------------------------------------------------
    // Public Properties (Read-only Access)
    // -------------------------------------------------------------------------

    /// <summary> Gets the current state of the battle (e.g., PLAYERTURN, ENEMYTURN). </summary>
    public BattleState State { get; private set; } // Changed setter to private

    /// <summary> Gets the instantiated Player Unit component. </summary>
    public UnitRefactored PlayerUnit { get; private set; }

    /// <summary> Gets the instantiated Enemy Unit component. </summary>
    public UnitRefactored EnemyUnit { get; private set; }

    // --- Exposing Components for BattleActions ---
    /// <summary> Gets the dialogue text UI component. </summary>
    public TextMeshProUGUI DialogueText => _dialogueText;
    /// <summary> Gets the player's HUD component. </summary>
    public BattleHUDRefactored PlayerHUD => _playerHUD;
    /// <summary> Gets the enemy's HUD component. </summary>
    public BattleHUDRefactored EnemyHUD => _enemyHUD;
    // --- Exposing Settings for BattleActions ---
    public float DamageMinMultiplier => _damageMinMultiplier;
    public float DamageMaxMultiplier => _damageMaxMultiplier;
    public float CritChance => _critChance;
    public float CritMultiplier => _critMultiplier;
    public float HealMinMultiplier => _healMinMultiplier;
    public float HealMaxMultiplier => _healMaxMultiplier;
    public float TurnDelay => _turnDelay;
    public float FeedbackDelay => _feedbackDelay;


    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    // References found/set in Awake/Start are now serialized fields above (_menuSystem, _screenFader)
    private int _currentPlayerLevel; // Use current level for calculations
    private int _currentEnemyLevel; // Use current level

    // Coroutine reference for the main battle flow
    private Coroutine _battleFlowCoroutine;


    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------
    private void Awake()
    {
        // Basic validation of critical references set in Inspector
        if (_battleActions == null) Debug.LogError("[BattleSystem] BattleActions reference not set!", this);
        if (_menuSystem == null) Debug.LogError("[BattleSystem] MenuSystem reference not set!", this);
        if (_playerPrefab == null || _enemyPrefab == null) Debug.LogError("[BattleSystem] Player or Enemy Prefab not set!", this);
        if (_playerBattleStation == null || _enemyBattleStation == null) Debug.LogError("[BattleSystem] Player or Enemy Battle Station transform not set!", this);
        if (_dialogueText == null) Debug.LogError("[BattleSystem] Dialogue Text UI reference not set!", this);
        if (_playerHUD == null || _enemyHUD == null) Debug.LogError("[BattleSystem] Player or Enemy HUD reference not set!", this);
        if (_attackButton == null || _healButton == null) Debug.LogError("[BattleSystem] Action Button(s) not set!", this);

        // ScreenFader is optional, check found by FindFirstObjectByType (now serialized field)
        // _screenFader = FindFirstObjectByType<ScreenFader>(FindObjectsInactive.Include);
        if (_screenFader == null) Debug.LogWarning("[BattleSystem] Optional ScreenFader not found or assigned.", this);

        // Load initial levels from SaveManager
        _currentPlayerLevel = SaveManager.PlayerLevel;
        _currentEnemyLevel = SaveManager.EnemyLevel; // May be overridden by calculation later
    }

    private void Start()
    {
        // Ensure previous battle flow is stopped if scene reloads somehow
        if (_battleFlowCoroutine != null)
        {
            StopCoroutine(_battleFlowCoroutine);
        }
        // Kick off the initial battle sequence
        _battleFlowCoroutine = StartCoroutine(InitialBattleRoutine());
    }

    // -------------------------------------------------------------------------
    // Core Battle Flow Coroutines
    // -------------------------------------------------------------------------

    /// <summary>
    /// Orchestrates the entire initial setup and start of the battle.
    /// </summary>
    private IEnumerator InitialBattleRoutine()
    {
        State = BattleState.START;
        Debug.Log("[BattleSystem] Starting Initial Battle Routine...");
        SetActionButtonsInteractable(false); // Ensure buttons off during setup
        yield return SetupBattleRoutine();   // Spawn units, set levels/HUDs
        yield return StartBattleRoutine();   // Determine turn order and start first turn
        _battleFlowCoroutine = null; // Mark main flow as complete (turns handle themselves now)
        Debug.Log("[BattleSystem] Initial Battle Routine Complete. Handing off to turn system.");
    }


    /// <summary>
    /// Handles spawning units, setting their levels, and updating the HUDs.
    /// </summary>
    public IEnumerator SetupBattleRoutine()
    {
        Debug.Log("[BattleSystem] Setting up battle...");
        // Clear previous units if any (for potential restarts without scene reload)
        ClearExistingUnits();

        // Instantiate and initialize player
        PlayerUnit = InstantiateAndInitUnit(_playerPrefab, _playerBattleStation, _currentPlayerLevel);
        if (PlayerUnit == null) yield break; // Stop if instantiation failed

        // Calculate enemy level based on player level and offsets
        _currentEnemyLevel = CalculateEnemyLevel(_currentPlayerLevel);
        SaveManager.EnemyLevel = _currentEnemyLevel; // Save the calculated level for potential continuation

        // Instantiate and initialize enemy
        EnemyUnit = InstantiateAndInitUnit(_enemyPrefab, _enemyBattleStation, _currentEnemyLevel);
        if (EnemyUnit == null) yield break; // Stop if instantiation failed

        // Update HUDs with initial unit data
        UpdateAllHUDs();

        // Display initial message
        yield return ShowDialogRoutine($"A wild {EnemyUnit.UnitName} (Lvl {EnemyUnit.Level}) appeared!");

        Debug.Log($"[BattleSystem] Setup complete. Player Lvl: {PlayerUnit.Level}, Enemy Lvl: {EnemyUnit.Level}");
    }

    /// <summary>
    /// Determines the turn order based on unit speed and starts the appropriate turn routine.
    /// </summary>
    public IEnumerator StartBattleRoutine()
    {
        Debug.Log("[BattleSystem] Determining turn order...");
        yield return new WaitForSeconds(_battleStartDelay); // Wait before first turn

        if (PlayerUnit == null || EnemyUnit == null)
        {
            Debug.LogError("[BattleSystem] Cannot start battle, units not initialized properly!");
            yield break;
        }

        // Compare speeds to determine who goes first
        if (PlayerUnit.Speed >= EnemyUnit.Speed)
        {
            Debug.Log("[BattleSystem] Player has higher or equal speed. Starting Player Turn.");
            yield return StartCoroutine(PlayerTurnRoutine()); // Start player turn
        }
        else
        {
            Debug.Log("[BattleSystem] Enemy has higher speed. Starting Enemy Turn.");
            yield return StartCoroutine(EnemyTurnRoutine()); // Start enemy turn
        }
    }

    /// <summary>
    /// Sets the state to PLAYERTURN, updates dialogue, and enables action buttons.
    /// Control is then handed off to BattleInputRefactored via button clicks.
    /// </summary>
    public IEnumerator PlayerTurnRoutine() // Made public for BattleActions
    {
        State = BattleState.PLAYERTURN;
        yield return ShowDialogRoutine("Choose an action:"); // Update dialogue
        SetActionButtonsInteractable(true); // Enable player input
        // Execution now waits for a button press handled by BattleInputRefactored
        yield break; // End this coroutine, wait for input
    }

    /// <summary>
    /// Handles the enemy's turn sequence: display message, perform action, check for player defeat.
    /// If the player survives, transitions back to the player's turn.
    /// </summary>
    public IEnumerator EnemyTurnRoutine() // Made public for BattleActions
    {
        State = BattleState.ENEMYTURN;
        SetActionButtonsInteractable(false); // Disable player input during enemy turn
        yield return ShowDialogRoutine($"{EnemyUnit.UnitName}'s turn!");
        yield return new WaitForSeconds(_turnDelay); // Pause before enemy acts

        // Enemy performs its action (currently only attack)
        if (_battleActions != null)
        {
            yield return StartCoroutine(_battleActions.EnemyAttackRoutine());
        }
        else
        {
            Debug.LogError("[BattleSystem] BattleActions reference is missing, enemy cannot act!", this);
            // Potential fallback or state change needed here? For now, just log error.
        }


        // Check if the player was defeated
        if (PlayerUnit.CurrentHP <= 0)
        {
            State = BattleState.LOST;
            yield return StartCoroutine(EndBattleRoutine(false)); // Trigger end sequence (loss)
            yield break; // Stop further turn logic
        }

        // If player survived, transition back to player's turn
        // yield return new WaitForSeconds(_turnDelay); // Delay before message? (Already delayed in EnemyAttackRoutine)
        yield return ShowDialogRoutine("Your turn!");
        // yield return new WaitForSeconds(_turnDelay); // Delay before enabling buttons?
        yield return StartCoroutine(PlayerTurnRoutine()); // Start next player turn
    }

    /// <summary>
    /// Handles the end of the battle sequence: shows result, processes outcomes (level up, save),
    /// and fades to the appropriate win/lose menu.
    /// </summary>
    /// <param name="playerWon">True if the player won, false if they lost.</param>
    private IEnumerator EndBattleRoutine(bool playerWon)
    {
        SetActionButtonsInteractable(false); // Ensure buttons are off
        ShowResultDialog(playerWon);         // Display Win/Lose message
        yield return new WaitForSeconds(_turnDelay); // Pause on message

        ProcessPostBattle(playerWon);        // Handle level up / save logic

        // Fade to the correct menu (Win or Lose) using the MenuSystem
        if (_menuSystem != null)
        {
            yield return StartCoroutine(FadeToMenuRoutine(playerWon ? _menuSystem.ShowWinMenu : _menuSystem.ShowLoseMenu));
        }
        else
        {
            Debug.LogError("[BattleSystem] MenuSystem reference is missing, cannot transition to end menu!", this);
        }

    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------

    /// <summary> Destroys existing player/enemy GameObjects under the battle stations. </summary>
    private void ClearExistingUnits()
    {
        if (_playerBattleStation != null)
        {
            foreach (Transform child in _playerBattleStation) { Destroy(child.gameObject); }
        }
        if (_enemyBattleStation != null)
        {
            foreach (Transform child in _enemyBattleStation) { Destroy(child.gameObject); }
        }
        PlayerUnit = null;
        EnemyUnit = null;
    }


    /// <summary> Instantiates a unit prefab, initializes its level, and returns the Unit component. </summary>
    private UnitRefactored InstantiateAndInitUnit(GameObject prefab, Transform parentStation, int level)
    {
        if (prefab == null || parentStation == null)
        {
            Debug.LogError($"[BattleSystem] Cannot instantiate unit: Prefab or Battle Station is null!", this);
            return null;
        }

        GameObject unitGO = Instantiate(prefab, parentStation);
        UnitRefactored unitComponent = unitGO.GetComponent<UnitRefactored>();

        if (unitComponent == null)
        {
            Debug.LogError($"[BattleSystem] Unit Prefab '{prefab.name}' is missing the UnitRefactored component!", unitGO);
            Destroy(unitGO); // Clean up invalid instance
            return null;
        }

        unitComponent.InitializeLevel(level);
        return unitComponent;
    }


    /// <summary> Calculates the enemy's level based on the player's level and configured offsets. </summary>
    private int CalculateEnemyLevel(int playerLevel)
    {
        // Ensure min offset isn't greater than max offset
        int minOffset = Mathf.Min(_enemyLevelMinOffset, _enemyLevelMaxOffset);
        int maxOffset = Mathf.Max(_enemyLevelMinOffset, _enemyLevelMaxOffset);

        int offset = UnityEngine.Random.Range(minOffset, maxOffset + 1); // Max is inclusive
        int calculatedLevel = playerLevel + offset;

        // Ensure the final level is at least 1
        return Mathf.Max(1, calculatedLevel);
    }

    /// <summary> Updates both Player and Enemy HUDs if they exist. </summary>
    private void UpdateAllHUDs()
    {
        if (PlayerUnit != null && _playerHUD != null) _playerHUD.SetHUD(PlayerUnit);
        if (EnemyUnit != null && _enemyHUD != null) _enemyHUD.SetHUD(EnemyUnit);
    }

    /// <summary> Checks if the enemy is defeated and triggers the end battle sequence if so. </summary>
    /// <returns>True if the enemy was defeated and EndBattleRoutine started, false otherwise.</returns>
    public bool CheckEnemyDefeatAndProcess() // Public for BattleActions
    {
        if (EnemyUnit.CurrentHP <= 0)
        {
            State = BattleState.WON;
            StartCoroutine(EndBattleRoutine(true)); // Trigger end sequence (win)
            return true;
        }
        return false;
    }

    /// <summary> Checks if the player is defeated and triggers the end battle sequence if so. </summary>
    /// <returns>True if the player was defeated and EndBattleRoutine started, false otherwise.</returns>
    public bool CheckPlayerDefeatAndProcess() // Public for BattleActions potentially
    {
        if (PlayerUnit.CurrentHP <= 0)
        {
            State = BattleState.LOST;
            StartCoroutine(EndBattleRoutine(false)); // Trigger end sequence (loss)
            return true;
        }
        return false;
    }


    /// <summary> Displays "You Win!" or "You Lose!" in the dialogue text. </summary>
    private void ShowResultDialog(bool playerWon)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = playerWon ? "You Win!" : "You Lose!";
        }
    }

    /// <summary> Handles post-battle logic: player level up and saving progress on win. </summary>
    private void ProcessPostBattle(bool playerWon)
    {
        if (!playerWon) return; // Only process if player won

        if (PlayerUnit != null)
        {
            Debug.Log($"[BattleSystem] Player won! Leveling up from {PlayerUnit.Level} by {_levelUpAmount}.");
            PlayerUnit.LevelUp(_levelUpAmount);
            if (_playerHUD != null) _playerHUD.SetHUD(PlayerUnit); // Update HUD with new level
                                                                   // Save the player's new level
            SaveManager.PlayerLevel = PlayerUnit.Level;
            // Optionally update BestLevel
            if (PlayerUnit.Level > SaveManager.BestLevel)
            {
                SaveManager.BestLevel = PlayerUnit.Level;
                Debug.Log($"[BattleSystem] New Best Level saved: {SaveManager.BestLevel}");
            }
        }
    }

    /// <summary> Gets the appropriate menu display action from MenuSystem based on win/loss. </summary>
    private Action GetMenuAction(bool playerWon)
    {
        if (_menuSystem == null) return null;
        return playerWon ? _menuSystem.ShowWinMenu : _menuSystem.ShowLoseMenu;
    }

    /// <summary>
    /// Coroutine for handling screen fade to a menu using ScreenFader and UIFader.
    /// </summary>
    /// <param name="showMenuAction">The Action (method reference) to call on the MenuSystem to show the correct menu.</param>
    private IEnumerator FadeToMenuRoutine(Action showMenuAction)
    {
        if (showMenuAction == null) yield break; // Nothing to show

        // If no screen fader is available, just show the menu instantly
        if (_screenFader == null)
        {
            showMenuAction();
            yield break;
        }

        // Use ScreenFader and UIFader for smooth transition
        CanvasGroup fadeCanvasGroup = _screenFader.GetComponent<CanvasGroup>();
        if (fadeCanvasGroup == null)
        { // Should not happen if ScreenFader requires it
            showMenuAction();
            yield break;
        }

        _screenFader.gameObject.SetActive(true); // Ensure fader object is active
        fadeCanvasGroup.blocksRaycasts = true;   // Block interaction during fade

        // 1. Fade overlay IN (to black)
        yield return StartCoroutine(UIFader.FadeInCanvasGroup(fadeCanvasGroup, _endMenuFadeDuration));

        // 2. Swap the menus behind the overlay
        showMenuAction();

        // 3. Fade overlay OUT (to transparent)
        yield return StartCoroutine(UIFader.FadeOutCanvasGroup(fadeCanvasGroup, _endMenuFadeDuration));

        // 4. Clean up fader state
        fadeCanvasGroup.blocksRaycasts = false;
        // Do not disable the ScreenFader GameObject here - let it handle its own state or be persistent
        // _screenFader.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays a message in the dialogue UI for a specified duration.
    /// </summary>
    /// <param name="message">The text message to display.</param>
    private IEnumerator ShowDialogRoutine(string message)
    {
        if (_dialogueText != null)
        {
            _dialogueText.text = message;
            // Use the shorter feedback delay for messages
            if (_feedbackDelay > 0) yield return new WaitForSeconds(_feedbackDelay);
        }
        else
        {
            // Log if dialogue text is missing but message was intended
            Debug.LogWarning($"[BattleSystem] Dialogue Text UI is null, cannot display message: '{message}'", this);
        }
    }


    /// <summary>
    /// Toggles the interactable state of the player action buttons.
    /// </summary>
    /// <param name="enable">True to enable buttons, false to disable.</param>
    public void SetActionButtonsInteractable(bool enable) // Public for BattleActions potentially
    {
        if (_attackButton != null) _attackButton.interactable = enable;
        if (_healButton != null) _healButton.interactable = enable;
        // if (_defendButton != null) _defendButton.interactable = enable; // TODO
    }
}

/// <summary>Represents the possible states during a battle.</summary>
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }