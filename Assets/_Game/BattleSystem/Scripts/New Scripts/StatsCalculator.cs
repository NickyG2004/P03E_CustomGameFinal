// -----------------------------------------------------------------------------
// Filename: StatsCalculator.cs (MonoBehaviour Version)
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// A MonoBehaviour component that calculates unit stats (HP, Damage, Speed)
// based on level using configurable base values and growth factors set in the
// Inspector. Ensures calculated stats are always at least 1.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Calculates scaled stats (HP, Damage, Speed) for a given level based on
/// configurable base values and growth factors assigned in the Inspector.
/// Attach this component to relevant GameObjects (e.g., Unit prefabs).
/// </summary>
public class StatsCalculator : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------
    [Header("Health (HP) Scaling")]
    [Tooltip("Base hit points at level 1. Must be at least 1.")]
    [SerializeField, Min(1)] private int _baseHP = 20; // Default value

    [Tooltip("Growth factor for HP scaling per level (uses natural logarithm: Log(level+1)). Higher values = faster HP increase. Must be non-negative.")]
    [SerializeField, Min(0f)] private float _hpGrowthFactor = 2.5f; // Default value

    [Header("Damage Scaling")]
    [Tooltip("Base damage at level 1. Must be at least 1.")]
    [SerializeField, Min(1)] private int _baseDamage = 5; // Default value

    [Tooltip("Growth factor for damage scaling per level (uses natural logarithm: Log(level+1)). Higher values = faster damage increase. Must be non-negative.")]
    [SerializeField, Min(0f)] private float _damageGrowthFactor = 1.5f; // Default value

    [Header("Speed Scaling")]
    [Tooltip("Base speed at level 1. Must be at least 1.")]
    [SerializeField, Min(1)] private int _baseSpeed = 10; // Default value

    [Tooltip("Flat speed increase added per level beyond level 1 (linear growth). Must be non-negative.")]
    [SerializeField, Min(0f)] private float _speedGrowthPerLevel = 0.5f; // Default value

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calculates the HP, damage, and speed values for a specified unit level
    /// based on the component's configured base values and growth factors.
    /// Clamps input level and output stats to be at least 1.
    /// </summary>
    /// <param name="level">The unit's level (input clamped to minimum 1).</param>
    /// <param name="hp">Output: Computed hit points (guaranteed >= 1).</param>
    /// <param name="dmg">Output: Computed damage (guaranteed >= 1).</param>
    /// <param name="spd">Output: Computed speed (guaranteed >= 1).</param>
    public void CalculateStats(int level, out int hp, out int dmg, out int spd)
    {
        // Clamp the input level to ensure it's at least 1
        int calculationLevel = Mathf.Max(1, level);

        // --- Calculate Raw Stats using Formulas ---

        // HP: Logarithmic scaling, rounded down
        float rawHP = _baseHP * Mathf.Log(calculationLevel + 1) * _hpGrowthFactor;

        // Damage: Logarithmic scaling, rounded up
        float rawDmg = _baseDamage * Mathf.Log(calculationLevel + 1) * _damageGrowthFactor;

        // Speed: Linear scaling, rounded to nearest
        float rawSpd = _baseSpeed + _speedGrowthPerLevel * (calculationLevel - 1);

        // --- Clamp and Assign Output Values ---

        // Ensure final HP is at least 1
        hp = Mathf.Max(1, Mathf.FloorToInt(rawHP));

        // Ensure final Damage is at least 1
        dmg = Mathf.Max(1, Mathf.CeilToInt(rawDmg));

        // Ensure final Speed is at least 1
        spd = Mathf.Max(1, Mathf.RoundToInt(rawSpd));

        // Optional: Log calculated stats for debugging
        // Debug.Log($"[{gameObject.name}/StatsCalculator] Lvl {level} (Calc as {calculationLevel}) -> HP:{hp}, DMG:{dmg}, SPD:{spd}", this);
    }
}