using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, NEXTMENU }


public class BattleSystem : MonoBehaviour
{
    // Set to true to enable debug logs
    public bool debugMode = false;

    // Prefabs for player and enemy units.
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    // Battle stations for player and enemy units.
    [SerializeField] private Transform playerBattleStation;
    [SerializeField] private Transform enemyBattleStation;

    // References to the player and enemy units.
    private Unit playerUnit;
    private Unit enemyUnit;

    // Reference to the dialogue text UI element for displaying battle messages.
    [SerializeField] private TextMeshProUGUI dialogueText;

    // Reference to the BattleHUD script for displaying unit information.
    [SerializeField] private BattleHUD playerHUD;
    [SerializeField] private BattleHUD enemyHUD;

    // Amount of HP to heal when the player uses the heal action.
    [SerializeField] private int healAmount = 10;

    // set delay timers
    [SerializeField] private float turnDelay = 2f;
    [SerializeField] private float battleStartDelay = 2f;
    [SerializeField] private float buttonDelay = 2f;

    // Level up amount for the player unit when they win a battle.
    [SerializeField] private int levelUpAmount = 1;

    // set MenuSystem reference if needed.
    [SerializeField] private MenuSystem menuSystem;

    // The current state of the battle.
    public BattleState state;

    // start is called before the first frame update.
    void Start()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Battle System Initialized");
        }

        // Set up the battle by instantiating player and enemy units.
        StartBattle();
    }

    // public function that returns the current battle state.
    public BattleState GetBattleState()
    {
        return state;
    }

    // public function to set the battle state.
    public void SetBattleState(BattleState newState)
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Setting battle state to " + newState);
        }
        state = newState;
    }

    // Function to start a battle
    public void StartBattle()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Starting battle");
        }

        // Set the battle state to START.
        state = BattleState.START;

        // Start the coroutine to set up the battle.
        StartCoroutine(SetUpBattle());
    }

    IEnumerator SetUpBattle()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Setting up battle");
        }

        // Instantiate player and enemy units at their respective battle stations.
        GameObject playerGo = Instantiate(playerPrefab, playerBattleStation);
        playerUnit = playerGo.GetComponent<Unit>();

        GameObject enemyGo = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = enemyGo.GetComponent<Unit>();

        // Set dialog text for the beginning of the battle.
        dialogueText.text = "A wild " + enemyUnit.unitName + " appeared!";

        if (debugMode)
        {
            Debug.Log("BattleSystem: Player Unit - " + playerUnit.unitName + ", Enemy Unit - " + enemyUnit.unitName);
        }

        // Initialize the HUDs with the player and enemy units.
        playerHUD.SetHUD(playerUnit);
        enemyHUD.SetHUD(enemyUnit);

        // delay for a moment to allow the HUDs to update.
        yield return new WaitForSeconds(battleStartDelay);

        // Start the player's turn.
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Player's turn");
        }

        // Set the dialog text to indicate it's the player's turn.
        dialogueText.text = "Choose an action:";
    }

    public void OnAttackButton()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Player chose to attack");
        }

        // If it's the player's turn, proceed with the attack.
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Player attacking enemy");
        }

        // set current state of the enemy. / Damage Enemy (Alive or Dead).
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

        // Update the enemy HUD with the new HP.
        enemyHUD.SetHP(enemyUnit.currentHP);

        // Update the dialog text to indicate the player is attacking.
        dialogueText.text = "The Attack was Successful!";

        // Set the dialog text to indicate the player is attacking.
        yield return new WaitForSeconds(buttonDelay);

        // check if the enemy is dead.
        if (isDead)
        {
            // debug message.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Enemy defeated");
            }

            // Update the dialog text to indicate the enemy is defeated.
            dialogueText.text = enemyUnit.unitName + " has been defeated!";

            // Set the battle state to WON.
            state = BattleState.WON;

            // Wait for a moment before ending the battle.
            yield return new WaitForSeconds(turnDelay);

            // End the battle.
            EndBattle();
        }
        else
        {
            // debug message.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Enemy took damage, remaining HP: " + enemyUnit.currentHP);
            }

            // If the enemy is still alive, proceed to the enemy's turn.
            state = BattleState.ENEMYTURN;

            // Update the dialog text to indicate the enemy's turn.
            dialogueText.text = "Enemy's turn!";

            // Wait for a moment before starting the enemy's turn.
            yield return new WaitForSeconds(turnDelay);

            // Start the enemy's turn.
            StartCoroutine(enemyTurn());

        }

    }
    public void OnHealButton()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Player chose to heal");
        }

        // If it's the player's turn, proceed with the heal action.
        if (state != BattleState.PLAYERTURN)
        {
            return;
        }

        // start heal coroutine.
        StartCoroutine(PlayerHeal());
    }

    IEnumerator PlayerHeal()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Player healing");
        }

        // Set the dialog text to indicate the player is healing.
        dialogueText.text = "Healing...";

        // Wait for a moment before healing.
        yield return new WaitForSeconds(buttonDelay);

        // Heal the player unit.
        playerUnit.Heal(healAmount);

        // Update the player's HUD with the new HP.
        playerHUD.SetHP(playerUnit.currentHP);

        // Update the dialog text to indicate the heal was successful.
        dialogueText.text = "You healed for " + healAmount + " HP!";

        // Wait for a moment before proceeding to the enemy's turn.
        yield return new WaitForSeconds(turnDelay);

        // Set the battle state to ENEMYTURN.
        state = BattleState.ENEMYTURN;

        // Start the enemy's turn.
        StartCoroutine(enemyTurn());
    }

    IEnumerator enemyTurn()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Enemy's turn");
        }

        // Set the dialog text to indicate it's the enemy's turn.
        dialogueText.text = enemyUnit.unitName + " is attacking!";

        // Wait for a moment before the enemy attacks.
        yield return new WaitForSeconds(buttonDelay);

        // Calculate the damage the enemy will deal to the player.
        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

        // Update the player's HUD with the new HP.
        playerHUD.SetHP(playerUnit.currentHP);

        // Wait for a moment before the enemy attacks.
        yield return new WaitForSeconds(buttonDelay);

        // Check if the player is dead.
        if (isDead)
        {
            // debug message.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Player defeated");
            }

            // Set the dialog text to indicate the player is defeated.
            dialogueText.text = "You have been defeated!";

            // Set the battle state to LOST.
            state = BattleState.LOST;

            // Wait for a moment before ending the battle.
            yield return new WaitForSeconds(turnDelay);

            // End the battle.
            EndBattle();
        }
        else
        {
            // debug message.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Player took damage, remaining HP: " + playerUnit.currentHP);
            }

            // If the player is still alive, proceed to the player's turn.
            state = BattleState.PLAYERTURN;

            // Update the dialog text to indicate it's the player's turn again.
            dialogueText.text = "Your turn!";

            // Wait for a moment before starting the players turn.
            yield return new WaitForSeconds(turnDelay);

            // Start the player's turn.
            PlayerTurn();
        }
    }

    

    

    

    void EndBattle()
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleSystem: Ending battle");
        }

        if (state == BattleState.WON)
        {
            // Set the dialog text to indicate the battle is over.
            dialogueText.text = "You Won the Battle!!!";

            // You can implement logic here to reward the player, such as giving experience points or items.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Player won, rewarding experience and items");
            }

            // Increase player level
            playerUnit.LevelUp(levelUpAmount);

            // Increase enemy level by 1 - 2 levels for the next round
            enemyUnit.LevelUp(Random.Range(1, 3));

            // Update the player's HUD with the new level and HP.
            playerHUD.SetHUD(playerUnit);

            // Optionally, you can also update the enemy HUD to indicate it has been defeated.
            enemyHUD.SetHUD(enemyUnit);

            // You can also implement logic to transition to a victory screen or return to the main menu.
            if (debugMode)
            {
                Debug.Log("BattleSystem: Player leveled up to level " + playerUnit.unitLevel);
            }

            // For example, you can load a victory menu.
            if (menuSystem != null)
            {
                menuSystem.ShowWinMenu();
            }
            else
            {
                Debug.LogWarning("BattleSystem: MenuSystem reference is not assigned.");
            }
        }

        if (state == BattleState.LOST)
        {
            // Set the dialog text to indicate the battle is over.
            dialogueText.text = "You Lost the Battle...";
        }

        // Here you can implement the logic to present the after battle menu or transition to another scene.
    }

}
