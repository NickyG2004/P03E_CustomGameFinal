// -----------------------------------------------------------------------------
// BattleActionsRefactored.cs
// -----------------------------------------------------------------------------
// Handles player and enemy actions: attacks, healing, and turn hand-offs.
// -----------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BattleSystemRefactored))]
public class BattleActionsRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    /// <summary>Reference to the main battle system.</summary>
    private BattleSystemRefactored _battleSystem;

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------
    private void Awake()
    {
        // Cache the BattleSystemRefactored component
        _battleSystem = GetComponent<BattleSystemRefactored>();
    }

    // -------------------------------------------------------------------------
    // Public Action Routines
    // -------------------------------------------------------------------------
    /// <summary>
    /// Performs the player's attack, handles crits, damage application, and turn transition.
    /// </summary>
    public IEnumerator PlayerAttackRoutine()
    {
        // Disable input and show attack message
        yield return ShowMessageRoutine("You attack!", _battleSystem.TurnDelay);

        // Roll damage with crit calculation
        bool wasCrit;
        int damage = _battleSystem.PlayerUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out wasCrit
        );

        // Show critical hit message if occurred
        if (wasCrit)
            yield return ShowMessageRoutine("Critical hit!", _battleSystem.ButtonDelay);

        // Apply damage and update HUD
        ApplyDamage(_battleSystem.EnemyUnit, _battleSystem.EnemyHUD, damage);

        // Check for enemy defeat
        if (_battleSystem.EnemyUnit.CurrentHP <= 0)
        {
            _battleSystem.State = BattleState.WON;
            yield return ShowMessageRoutine("Enemy defeated!", _battleSystem.TurnDelay);
            yield return _battleSystem.EndBattleRoutine();
            yield break;
        }

        // Show damage dealt and hand off to enemy
        yield return ShowMessageRoutine($"You deal {damage} damage!", _battleSystem.TurnDelay);
        yield return _battleSystem.EnemyTurnRoutine();
    }

    /// <summary>
    /// Performs the enemy's attack, handles crits, damage application, and end of turn.
    /// </summary>
    public IEnumerator EnemyAttackRoutine()
    {
        // Show enemy attack message
        yield return ShowMessageRoutine($"{_battleSystem.EnemyUnit.UnitName} attacks!", _battleSystem.TurnDelay);

        // Roll damage with crit calculation
        bool wasCrit;
        int damage = _battleSystem.EnemyUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out wasCrit
        );

        // Show critical hit message if occurred
        if (wasCrit)
            yield return ShowMessageRoutine("Enemy lands a critical!", _battleSystem.ButtonDelay);

        // Apply damage and update HUD
        ApplyDamage(_battleSystem.PlayerUnit, _battleSystem.PlayerHUD, damage);

        // Show damage taken
        yield return ShowMessageRoutine($"You take {damage} damage!", _battleSystem.TurnDelay);
    }

    /// <summary>
    /// Performs the player's heal action, including full-HP guard and turn hand-off.
    /// </summary>
    public IEnumerator PlayerHealRoutine()
    {
        // Disable input
        _battleSystem.SetActionButtonsInteractable(false);

        // Already at full health? Re-prompt
        if (_battleSystem.PlayerUnit.CurrentHP >= _battleSystem.PlayerUnit.MaxHP)
        {
            yield return ShowMessageRoutine("Already at full health!", _battleSystem.TurnDelay);
            yield return _battleSystem.PlayerTurnRoutine();
            yield break;
        }

        // Calculate heal amount and clamp
        int healValue = _battleSystem.PlayerUnit.GetRandomHealAmount(
            _battleSystem.HealMinMultiplier,
            _battleSystem.HealMaxMultiplier
        );
        int missingHP = _battleSystem.PlayerUnit.MaxHP - _battleSystem.PlayerUnit.CurrentHP;
        healValue = Mathf.Min(healValue, missingHP);

        // Show healing message
        yield return ShowMessageRoutine("Healing...", _battleSystem.ButtonDelay);

        // Apply heal and update HUD
        _battleSystem.PlayerUnit.Heal(healValue);
        _battleSystem.PlayerHUD.SetHP(_battleSystem.PlayerUnit.CurrentHP);

        // Show healed amount and transition
        yield return ShowMessageRoutine($"Recovered {healValue} HP!", _battleSystem.TurnDelay);
        yield return _battleSystem.EnemyTurnRoutine();
    }

    // -------------------------------------------------------------------------
    // Private Helpers (Pass 3)
    // -------------------------------------------------------------------------
    /// <summary>
    /// Displays a message for a set duration.
    /// </summary>
    private IEnumerator ShowMessageRoutine(string message, float delay)
    {
        _battleSystem.DialogueText.text = message;
        yield return new WaitForSeconds(delay);
    }

    /// <summary>
    /// Applies damage to a unit and updates its HUD.
    /// </summary>
    private void ApplyDamage(UnitRefactored target, BattleHUDRefactored hud, int damage)
    {
        target.TakeDamage(damage);
        hud.SetHP(target.CurrentHP);
    }
}
