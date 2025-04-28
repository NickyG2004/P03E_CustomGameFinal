// Summary of Script and functions


using UnityEngine;
using System.Collections;
using TMPro;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, NEXTMENU }

public class BattleSystemRefactored : MonoBehaviour
{
    [Header("Debug")]
    public bool debugMode = false; // Needs to stay public

    [Header("Prefabs & Stations")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerBattleStation;
    public Transform enemyBattleStation;

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public BattleHUDRefactored playerHUD;
    public BattleHUDRefactored enemyHUD;

    [Header("Battle Settings")]
    public float battleStartDelay = 2f;
    public float turnDelay = 2f;
    public float buttonDelay = 2f;

    [Header("Heal & Level-Up")]
    public int healAmount = 10;
    public int levelUpAmount = 1;

    [Header("Menus")]
    public MenuSystemRefactored menuSystem;

    // Public so BattleActions can access:
    public BattleState state; // Needs to stay public
    public UnitRefactored playerUnit; // Needs to stay public
    public UnitRefactored enemyUnit; // Needs to stay public

    // Private variables for saving player and enemy levels
    private int _savedPlayerLevel = 1;
    private int _savedEnemyLevel = 1;

    void Awake()
    {
        // Load whatever was last saved (defaults to 1)
        _savedPlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        _savedEnemyLevel = PlayerPrefs.GetInt("EnemyLevel", 1);
    }

    void Start()
    {
        if (debugMode) Debug.Log("BattleSystemRefactored: Initialized");

        // Initialize the battle system
        StartBattle();
    }

    public void StartBattle()
    {
        if (debugMode) Debug.Log("BattleSystemRefactored: Starting battle");

        // Reset the state and prepare for a new battle
        state = BattleState.START;
        StartCoroutine(SetUpBattle());
    }

    IEnumerator SetUpBattle()
    {
        if (debugMode) Debug.Log("BattleSystemRefactored: Setting up battle");

        // instantiate player
        GameObject pGO = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = pGO.GetComponent<UnitRefactored>();
        // **apply saved level before anything else**
        playerUnit.InitializeLevel(_savedPlayerLevel);

        // instantiate enemy
        GameObject eGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = eGO.GetComponent<UnitRefactored>();
        // **apply saved enemy level**
        enemyUnit.InitializeLevel(_savedEnemyLevel);

        // now the HUD will show the correct levels
        dialogueText.text = "A wild " + enemyUnit.unitName + " appeared!";
        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        yield return new WaitForSeconds(battleStartDelay);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    public void PlayerTurn()
    {
        if (debugMode) Debug.Log("BattleSystemRefactored: Player's turn");

        // Display the player's options
        dialogueText.text = "Choose an action:";
    }

    // Attack, Heal and enemyTurn coroutines -> moved to BattleActionsRefactored.cs

    /// <summary>Called by BattleActionsRefactored when the fight ends or a turn completes.</summary>
    public void EndBattle()
    {
        if (debugMode) Debug.Log("BattleSystemRefactored: Ending battle");

        // Check the battle state and update the dialogue text accordingly
        if (state == BattleState.WON)
        {
            // Player wins the battle

            // Level up the player and enemy units
            dialogueText.text = "You Won the Battle!!!";
            playerUnit.LevelUp(levelUpAmount);
            enemyUnit.LevelUp(Random.Range(1, 3));

            // Save the player and enemy levels
            _savedPlayerLevel = playerUnit.unitLevel;
            _savedEnemyLevel = enemyUnit.unitLevel;

            // Update the HUDs
            playerHUD.SetHUD(playerUnit);
            enemyHUD.SetHUD(enemyUnit);

            // Show the win menu
            if (menuSystem != null)
                menuSystem.ShowWinMenu();
            else
                Debug.LogWarning("MenuSystemRefactored not assigned.");

            //// Play win sound
            // implement win sound logic here

            //// Play win music
            // implement win music logic here
        }
        else if (state == BattleState.LOST)
        {
            // Player loses the battle

            // Display lose text (optional)
            dialogueText.text = "You Lost the Battle...";

            // you may want to still record the final player level as a high score
            PlayerPrefs.SetInt("BestLevel", playerUnit.unitLevel);

            // Show lose menu
            if (menuSystem != null)
                menuSystem.ShowLoseMenu();
            else if (debugMode)
                Debug.LogWarning("BattleSystemRefactored: MenuSystemRefactored not assigned.");

            //// Play lose sound
            // implement lose sound logic here

            //// Play lose music
            // implement lose music logic here

        }
    }
}
