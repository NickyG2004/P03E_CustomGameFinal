// -----------------------------------------------------------------------------
// Filename: BattleActionsRefactored.cs
// (Provided as Refactored Example)
// -----------------------------------------------------------------------------
// Handles player and enemy actions: attacks, healing, and turn hand-offs.
// Relies heavily on BattleSystemRefactored for state, data, and flow control.
// -----------------------------------------------------------------------------

using System; // Not strictly needed here unless Actions were used
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the execution logic for specific battle actions like Player Attack,
/// Player Heal, and Enemy Attack. Works in conjunction with BattleSystemRefactored.
/// </summary>
[RequireComponent(typeof(BattleSystemRefactored))]
public class BattleActionsRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    private BattleSystemRefactored _battleSystem; // Cached reference

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Caches the required BattleSystemRefactored component.
    /// </summary>
    private void Awake()
    {
        _battleSystem = GetComponent<BattleSystemRefactored>();
        if (_battleSystem == null)
        {
            Debug.LogError("[BattleActions] BattleSystemRefactored component not found on this GameObject!", this);
            this.enabled = false; // Disable if core dependency is missing
        }
    }

    // -------------------------------------------------------------------------
    // Public Action Coroutines (Called by BattleInputRefactored or BattleSystemRefactored)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Coroutine for the player's attack sequence. Disables input, performs the
    /// attack roll, applies damage, checks for enemy defeat, displays messages,
    /// and transitions to the enemy's turn if the battle continues.
    /// </summary>
    public IEnumerator PlayerAttackRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // Disable input and show initial message
        _battleSystem.SetActionButtonsInteractable(false);
        yield return ShowMessageRoutine("You attack!", _battleSystem.FeedbackDelay);

        // Roll damage with critical hit calculation
        int damage = _battleSystem.PlayerUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out bool wasCrit // Use 'out' parameter to get crit status
        );

        // Show critical hit message if it occurred
        if (wasCrit)
        {
            yield return ShowMessageRoutine("Critical hit!", _battleSystem.FeedbackDelay);
        }

        // Apply damage to enemy and update their HUD
        ApplyDamage(_battleSystem.EnemyUnit, _battleSystem.EnemyHUD, damage);

        // Check if the enemy was defeated
        if (_battleSystem.CheckEnemyDefeatAndProcess()) // Let BattleSystem handle end routine start
        {
            yield return ShowMessageRoutine($"Dealt {damage} damage. Enemy defeated!", _battleSystem.TurnDelay);
            // EndBattleRoutine is started by CheckEnemyDefeatAndProcess
            yield break; // Stop this coroutine as battle has ended
        }


        // If enemy survived, show damage dealt and hand off to enemy turn
        yield return ShowMessageRoutine($"You deal {damage} damage!", _battleSystem.TurnDelay);
        yield return StartCoroutine(_battleSystem.EnemyTurnRoutine()); // Start enemy's turn
    }

    /// <summary>
    /// Coroutine for the enemy's attack sequence. Shows message, performs attack roll,
    /// applies damage to the player, displays damage message. Does NOT handle turn transition
    /// back to player (that's done by BattleSystemRefactored.EnemyTurnRoutine).
    /// </summary>
    public IEnumerator EnemyAttackRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // Show enemy attack message
        yield return ShowMessageRoutine($"{_battleSystem.EnemyUnit.UnitName} attacks!", _battleSystem.FeedbackDelay);

        // Roll damage with critical hit calculation
        int damage = _battleSystem.EnemyUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out bool wasCrit
        );

        // Show critical hit message if occurred
        if (wasCrit)
        {
            yield return ShowMessageRoutine("Enemy lands a critical hit!", _battleSystem.FeedbackDelay);
        }

        // Apply damage to player and update their HUD
        ApplyDamage(_battleSystem.PlayerUnit, _battleSystem.PlayerHUD, damage);

        // Show damage taken message (delay handled by caller - EnemyTurnRoutine)
        yield return ShowMessageRoutine($"You take {damage} damage!", 0); // No extra delay here
    }


    /// <summary>
    /// Coroutine for the player's heal sequence. Disables input, calculates heal amount,
    /// applies healing, checks for full health edge case, displays messages,
    /// and transitions to the enemy's turn.
    /// </summary>
    public IEnumerator PlayerHealRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // Disable input immediately
        _battleSystem.SetActionButtonsInteractable(false);

        // Check if already at full health
        if (_battleSystem.PlayerUnit.CurrentHP >= _battleSystem.PlayerUnit.MaxHP)
        {
            yield return ShowMessageRoutine("Already at full health!", _battleSystem.TurnDelay);
            // Re-enable buttons and return control to player choice
            yield return StartCoroutine(_battleSystem.PlayerTurnRoutine());
            yield break; // Exit this routine
        }

        // Calculate heal amount based on player level and multipliers
        int healValue = _battleSystem.PlayerUnit.GetRandomHealAmount(
            _battleSystem.HealMinMultiplier,
            _battleSystem.HealMaxMultiplier
        );

        // Clamp heal amount so it doesn't exceed missing HP
        int missingHP = _battleSystem.PlayerUnit.MaxHP - _battleSystem.PlayerUnit.CurrentHP;
        healValue = Mathf.Min(healValue, missingHP);
        // Ensure at least 1 point is healed if possible and missing HP
        healValue = Mathf.Max(1, healValue);


        // Show initial healing message
        yield return ShowMessageRoutine("Healing...", _battleSystem.FeedbackDelay);

        // Apply heal and update player HUD
        _battleSystem.PlayerUnit.Heal(healValue);
        _battleSystem.PlayerHUD.SetHP(_battleSystem.PlayerUnit.CurrentHP);

        // Show result message and transition to enemy turn
        yield return ShowMessageRoutine($"Recovered {healValue} HP!", _battleSystem.TurnDelay);
        yield return StartCoroutine(_battleSystem.EnemyTurnRoutine()); // Start enemy's turn
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods
    // -------------------------------------------------------------------------

    /// <summary> Checks if the BattleSystem reference is valid. </summary>
    /// <returns>True if valid, false otherwise.</returns>
    private bool ValidateBattleSystemReference()
    {
        if (_battleSystem == null)
        {
            Debug.LogError("[BattleActions] BattleSystem reference is null! Cannot perform action.", this);
            return false;
        }
        if (_battleSystem.PlayerUnit == null || _battleSystem.EnemyUnit == null)
        {
            Debug.LogError("[BattleActions] BattleSystem units are not initialized! Cannot perform action.", this);
            return false;
        }
        return true;
    }


    /// <summary>
    /// Displays a message in the BattleSystem's dialogue text UI for a set duration.
    /// </summary>
    /// <param name="message">The text message to display.</param>
    /// <param name="delay">Duration in seconds to show the message.</param>
    private IEnumerator ShowMessageRoutine(string message, float delay)
    {
        if (_battleSystem?.DialogueText != null) // Safe check
        {
            _battleSystem.DialogueText.text = message;
            if (delay > 0) yield return new WaitForSeconds(delay);
        }
        else
        {
            Debug.LogWarning($"[BattleActions] Cannot show message '{message}', BattleSystem or DialogueText is null.", this);
        }
    }

    /// <summary>
    /// Applies damage to a target unit and updates its corresponding HUD.
    /// </summary>
    /// <param name="target">The UnitRefactored component receiving damage.</param>
    /// <param name="hud">The BattleHUDRefactored component to update.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    private void ApplyDamage(UnitRefactored target, BattleHUDRefactored hud, int damage)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
            if (hud != null)
            {
                hud.SetHP(target.CurrentHP); // Update HUD after taking damage
            }
            else
            {
                Debug.LogWarning($"[BattleActions] HUD not assigned for target '{target.name}', cannot update HP display.", this);
            }
        }
        else
        {
            Debug.LogError("[BattleActions] ApplyDamage called with a null target unit!", this);
        }
    }
}