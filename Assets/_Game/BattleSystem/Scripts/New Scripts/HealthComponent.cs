// -----------------------------------------------------------------------------
// Filename: HealthComponent.cs
// (Provided as Refactored Example)
// -----------------------------------------------------------------------------
// Simple component to track maximum and current health points (HP).
// Provides methods for initialization, taking damage, and healing,
// ensuring HP stays within valid bounds [0, MaxHP].
// Used by Unit classes or any entity needing health management.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Manages health points (HP) for an entity. Handles initialization,
/// damage application (checking for defeat), and healing (clamping to max HP).
/// </summary>
public class HealthComponent : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Public Properties (Read-only access from outside)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the maximum possible health points for this entity.
    /// Set during Initialize or via SetMaxHP.
    /// </summary>
    public int MaxHP { get; private set; }

    /// <summary>
    /// Gets the current health points value. Always clamped between 0 and MaxHP.
    /// Modified by TakeDamage and Heal methods.
    /// </summary>
    public int CurrentHP { get; private set; }

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes the component, setting both maximum and current HP.
    /// Ensures values are non-negative.
    /// </summary>
    /// <param name="startingMaxHP">Initial maximum health points (must be >= 0).</param>
    public void Initialize(int startingMaxHP)
    {
        MaxHP = Mathf.Max(0, startingMaxHP); // Prevent negative max HP
        CurrentHP = MaxHP; // Start at full health
    }

    /// <summary>
    /// Updates the maximum HP value. Also clamps the current HP
    /// so it doesn't exceed the new maximum. Ensures new max HP is non-negative.
    /// </summary>
    /// <param name="newMaxHP">The new maximum health points value (must be >= 0).</param>
    public void SetMaxHP(int newMaxHP)
    {
        MaxHP = Mathf.Max(0, newMaxHP); // Prevent negative max HP
        // Ensure current HP does not exceed the new max HP
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
        // Also ensure current HP isn't negative (might happen if MaxHP becomes 0)
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    /// <summary>
    /// Applies damage to the entity's CurrentHP. Ensures CurrentHP does not go below 0.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply (should be non-negative).</param>
    /// <returns>True if the entity's HP dropped to 0 or below as a result of this damage, false otherwise.</returns>
    public bool TakeDamage(int damageAmount)
    {
        // Ensure damage isn't negative (which would heal)
        int actualDamage = Mathf.Max(0, damageAmount);

        CurrentHP -= actualDamage;
        // Clamp HP to a minimum of 0
        CurrentHP = Mathf.Max(0, CurrentHP);

        // Return true if HP is 0 (defeated)
        return CurrentHP <= 0;
    }

    /// <summary>
    /// Restores health points to the entity's CurrentHP.
    /// Ensures CurrentHP does not exceed MaxHP.
    /// </summary>
    /// <param name="healAmount">Amount of health points to restore (should be non-negative).</param>
    public void Heal(int healAmount)
    {
        // Ensure healing isn't negative (which would damage)
        int actualHeal = Mathf.Max(0, healAmount);

        CurrentHP += actualHeal;
        // Clamp HP to the maximum allowed
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
    }
}