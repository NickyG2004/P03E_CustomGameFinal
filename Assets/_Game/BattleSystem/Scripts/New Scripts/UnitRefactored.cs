// -----------------------------------------------------------------------------
// Filename: UnitRefactored.cs
// (Refactored: 2025-04-30) (Tooltip Corrected: 2025-04-30)
// -----------------------------------------------------------------------------
// Represents a combat unit, managing its identity, level-based stats
// (calculated via StatsCalculator component), health (via HealthComponent), and
// providing methods for combat actions like damage rolls and healing.
// Requires StatsCalculator and HealthComponent component references assigned in the Inspector.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Represents a combat unit, managing its identity, level-based stats (calculated via StatsCalculator),
/// health (via HealthComponent), and providing methods for combat actions like damage rolls and healing.
/// </summary>
public class UnitRefactored : MonoBehaviour
{
    #region Inspector Fields

    [Header("Display")]
    [Tooltip("The name of the unit displayed in UI elements.")]
    [SerializeField] private string _unitName = "Unit"; // Default name if not set

    [Header("Dependencies")]
    [Tooltip("Reference to the StatsCalculator component responsible for stat calculations based on level. Assign from the Inspector.")]
    [SerializeField] private StatsCalculator _statsCalculator;

    [Tooltip("Component that tracks and manages HP. Must be assigned.")]
    [SerializeField] private HealthComponent _healthComponent;

    #endregion

    #region Private State

    private int _level;
    private int _damageValue;
    private int _speedValue;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Validates that essential dependencies (_statsCalculator, _healthComponent)
    /// are assigned in the inspector during Awake.
    /// </summary>
    private void Awake()
    {
        // Validate critical dependencies assigned in the Inspector
        if (_statsCalculator == null)
        {
            Debug.LogError($"[Unit] StatsCalculator component reference is not assigned on {gameObject.name}! Unit requires this component.", this);
        }
        if (_healthComponent == null)
        {
            Debug.LogError($"[Unit] HealthComponent component reference is not assigned on {gameObject.name}! Unit requires this component.", this);
        }
        // Note: Initialization (setting level, calculating stats) is typically triggered
        // externally by the system managing unit spawning (e.g., BattleSystemRefactored).
    }

    #endregion

    #region Public API

    /// <summary>
    /// Initializes this unit at the given level. Calculates and applies stats
    /// using the assigned StatsCalculator component and initializes the HealthComponent.
    /// Logs errors and returns early if dependencies are missing.
    /// </summary>
    /// <param name="levelToSet">The level to initialize the unit at (clamped to be >= 1).</param>
    public void InitializeLevel(int levelToSet)
    {
        _level = Mathf.Max(1, levelToSet); // Ensure level is at least 1

        // Check dependencies before proceeding
        if (_statsCalculator == null || _healthComponent == null)
        {
            Debug.LogError($"[Unit] Cannot initialize {gameObject.name} at level {_level} due to missing dependencies (StatsCalculator or HealthComponent). Check Inspector assignments.", this);
            return;
        }

        // Pull all stats from the calculator component
        _statsCalculator.CalculateStats(
            _level,
            out int calculatedMaxHP,
            out int calculatedDamage,
            out int calculatedSpeed
        );

        // Apply HP via HealthComponent and store internal stat values
        _healthComponent.Initialize(calculatedMaxHP);
        _damageValue = calculatedDamage;
        _speedValue = calculatedSpeed;

        // Optional: Log successful initialization (useful for debugging)
        // Debug.Log($"[Unit] '{UnitName}' initialized: Level={Level}, MaxHP={MaxHP}, BaseDamage={BaseDamage}, Speed={Speed}", this);
    }

    /// <summary>
    /// Increases the unit's level by the specified amount and re-initializes its stats
    /// by calling InitializeLevel with the new level.
    /// </summary>
    /// <param name="levelsToAdd">Number of levels to increase by (must be positive, default is 1).</param>
    public void LevelUp(int levelsToAdd = 1)
    {
        if (levelsToAdd <= 0)
        {
            // Log a warning if trying to level up by zero or negative amount
            // Debug.LogWarning($"[Unit] LevelUp called with non-positive value ({levelsToAdd}) for {UnitName}. No level change.", this);
            return;
        }
        InitializeLevel(Level + levelsToAdd); // Use the Level property
    }

    /// <summary>
    /// Applies damage to this unit via its HealthComponent.
    /// Logs an error if the HealthComponent is missing.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to apply.</param>
    /// <returns>True if the unit is defeated (HP <= 0) after taking damage, false otherwise or if HealthComponent is missing.</returns>
    public bool TakeDamage(int damageAmount)
    {
        if (_healthComponent == null)
        {
            Debug.LogError($"[Unit] HealthComponent missing on {gameObject.name}. Cannot take damage.", this);
            return false; // Cannot determine defeat state
        }
        return _healthComponent.TakeDamage(damageAmount);
    }

    /// <summary>
    /// Heals this unit via its HealthComponent.
    /// Logs an error if the HealthComponent is missing.
    /// </summary>
    /// <param name="healAmount">The amount of health to restore.</param>
    public void Heal(int healAmount)
    {
        if (_healthComponent == null)
        {
            Debug.LogError($"[Unit] HealthComponent missing on {gameObject.name}. Cannot heal.", this);
            return;
        }
        _healthComponent.Heal(healAmount);
    }

    /// <summary>
    /// Rolls damage based on the unit's base damage, specified multipliers, and critical hit chance.
    /// </summary>
    /// <param name="minMultiplier">Minimum damage multiplier (applied to BaseDamage).</param>
    /// <param name="maxMultiplier">Maximum damage multiplier (applied to BaseDamage).</param>
    /// <param name="critChance">Chance (0.0 to 1.0) of a critical hit.</param>
    /// <param name="critMultiplier">Damage multiplier applied on a critical hit.</param>
    /// <param name="wasCrit">Output parameter: Set to true if the roll resulted in a critical hit, false otherwise.</param>
    /// <returns>The calculated damage amount after applying multipliers and checking for critical hits.</returns>
    public int RollDamage(
        float minMultiplier,
        float maxMultiplier,
        float critChance,
        float critMultiplier,
        out bool wasCrit)
    {
        // Calculate base damage range
        int minD = Mathf.FloorToInt(BaseDamage * minMultiplier);
        int maxD = Mathf.CeilToInt(BaseDamage * maxMultiplier);
        // Ensure min is not greater than max
        if (minD > maxD) minD = maxD;
        // Ensure damage is at least 1 if possible
        minD = Mathf.Max(1, minD);
        maxD = Mathf.Max(1, maxD);

        int roll = UnityEngine.Random.Range(minD, maxD + 1); // Max is inclusive

        // Check for critical hit
        wasCrit = UnityEngine.Random.value < critChance; // Random.value is [0.0, 1.0)
        if (wasCrit)
        {
            // Apply critical multiplier, using Ceiling to ensure bonus damage rounds up
            roll = Mathf.CeilToInt(roll * critMultiplier);
        }

        return roll;
    }

    /// <summary>
    /// Calculates a random amount of healing based on the unit's level and specified multipliers.
    /// </summary>
    /// <param name="minMultiplier">Minimum heal multiplier (applied to level).</param>
    /// <param name="maxMultiplier">Maximum heal multiplier (applied to level).</param>
    /// <returns>A random heal amount within the calculated range (ensuring at least 1 if possible).</returns>
    public int GetRandomHealAmount(float minMultiplier, float maxMultiplier)
    {
        // Use Level property, which is already clamped >= 1
        int minHeal = Mathf.FloorToInt(Level * minMultiplier);
        int maxHeal = Mathf.CeilToInt(Level * maxMultiplier);

        // Ensure minHeal is not greater than maxHeal
        if (minHeal > maxHeal) minHeal = maxHeal;

        // Ensure heal amount is at least 1 if maxHeal is 1 or more
        minHeal = Mathf.Max(1, minHeal);
        maxHeal = Mathf.Max(1, maxHeal); // Ensure max is also at least 1

        return UnityEngine.Random.Range(minHeal, maxHeal + 1); // Max is inclusive
    }

    #endregion

    #region Public Properties

    /// <summary> Gets the display name of the unit (set in Inspector). </summary>
    public string UnitName => _unitName;

    /// <summary> Gets the current level of the unit. </summary>
    public int Level => _level;

    /// <summary> Gets the maximum HP of the unit from its HealthComponent. Returns 0 if HealthComponent is missing. </summary>
    public int MaxHP => _healthComponent != null ? _healthComponent.MaxHP : 0;

    /// <summary> Gets the current HP of the unit from its HealthComponent. Returns 0 if HealthComponent is missing. </summary>
    public int CurrentHP => _healthComponent != null ? _healthComponent.CurrentHP : 0;

    /// <summary> Gets the calculated base damage value of the unit for its current level. </summary>
    public int BaseDamage => _damageValue;

    /// <summary> Gets the calculated speed value of the unit for its current level. </summary>
    public int Speed => _speedValue;

    #endregion
}