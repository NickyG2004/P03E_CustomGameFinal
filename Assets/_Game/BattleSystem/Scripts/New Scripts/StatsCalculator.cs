// -----------------------------------------------------------------------------
// StatsCalculator.cs
// -----------------------------------------------------------------------------
// Performs scaling of HP, damage, and speed based on unit level using configurable
// growth factors and base values.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Calculates scaled stats (HP, damage, speed) for a given level.
/// </summary>
[CreateAssetMenu(menuName = "Stats/StatsCalculator")]
public class StatsCalculator : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------------------------
    [Header("Health (HP)")]
    [SerializeField, Tooltip("Base hit points at level 1")]
    private int _baseHP;
    [SerializeField, Tooltip("Growth factor for HP scaling (logarithmic)")]
    private float _hpGrowthFactor;

    [Header("Damage")]
    [SerializeField, Tooltip("Base damage at level 1")]
    private int _baseDamage;
    [SerializeField, Tooltip("Growth factor for damage scaling (logarithmic)")]
    private float _damageGrowthFactor;

    [Header("Speed")]
    [SerializeField, Tooltip("Base speed at level 1")]
    private int _baseSpeed;
    [SerializeField, Tooltip("Linear speed increase per level beyond 1")]
    private float _speedGrowthPerLevel;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------
    /// <summary>
    /// Calculates HP, damage, and speed values for the specified unit level.
    /// </summary>
    /// <param name="level">Unit level (must be >= 1).</param>
    /// <param name="hp">Output: computed hit points.</param>
    /// <param name="dmg">Output: computed damage.</param>
    /// <param name="spd">Output: computed speed.</param>
    public void CalculateStats(int level, out int hp, out int dmg, out int spd)
    {
        // calculate each stat using helper methods
        hp = CalculateHP(level);
        dmg = CalculateDamage(level);
        spd = CalculateSpeed(level);
    }

    // -------------------------------------------------------------------------
    // Helper Methods
    // -------------------------------------------------------------------------
    /// <summary>
    /// Computes the scaled HP for a given level.
    /// </summary>
    private int CalculateHP(int level)
    {
        // floor(baseHP * log(level + 1) * hpGrowthFactor)
        return Mathf.FloorToInt(_baseHP * Mathf.Log(level + 1) * _hpGrowthFactor);
    }

    /// <summary>
    /// Computes the scaled damage for a given level.
    /// </summary>
    private int CalculateDamage(int level)
    {
        // ceil(baseDamage * log(level + 1) * damageGrowthFactor)
        return Mathf.CeilToInt(_baseDamage * Mathf.Log(level + 1) * _damageGrowthFactor);
    }

    /// <summary>
    /// Computes the scaled speed for a given level.
    /// </summary>
    private int CalculateSpeed(int level)
    {
        // round(baseSpeed + speedGrowthPerLevel * (level - 1))
        return Mathf.RoundToInt(_baseSpeed + _speedGrowthPerLevel * (level - 1));
    }
}
