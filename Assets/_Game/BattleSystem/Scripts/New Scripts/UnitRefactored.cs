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

    [Header("Defense Configuration")]
    [SerializeField, Tooltip("Constant used in damage reduction formula when defending: Multiplier = DefConstant / (DefConstant + Defense). Higher values mean Defense matters less initially.")]
    [Min(1f)] private float _defenseConstant = 100f; // Default: 100 is common

    [Header("Component References")]
    [SerializeField, Tooltip("Reference to the component controlling animations.")]
    private UnitAnimatorController _animatorController;

    #endregion

    #region Private State

    private int _level;
    private int _damageValue;
    private int _speedValue;
    private int _defenseValue;

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

        // Find the Animator Controller on the same GameObject
        if (_animatorController == null) _animatorController = GetComponent<UnitAnimatorController>();
        if (_animatorController == null) Debug.LogWarning($"[Unit] UnitAnimatorController not found on {gameObject.name}. Animations won't work.", this);
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
            out int calculatedSpeed,
            out int calculatedDefense
        );

        // Apply HP via HealthComponent and store internal stat values
        _healthComponent.Initialize(calculatedMaxHP);
        _damageValue = calculatedDamage;
        _speedValue = calculatedSpeed;
        _defenseValue = calculatedDefense;

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
    /// Applies damage to this unit. If the unit IsDefending, damage is reduced (halved, minimum 1).
    /// Passes the final damage amount to the HealthComponent.
    /// Logs an error if the HealthComponent is missing.
    /// </summary>
    /// <param name="damageAmount">The initial amount of damage before reduction.</param>
    /// <returns>True if the unit is defeated (HP <= 0) after taking damage, false otherwise or if HealthComponent is missing.</returns>
    public bool TakeDamage(int damageAmount)
    {
        if (_healthComponent == null)
        {
            Debug.LogError($"[Unit] HealthComponent missing on {gameObject.name}. Cannot take damage.", this);
            return false;
        }

        // Start with the original damage
        int actualDamage = damageAmount;

        // --- Apply Defense Reduction ---
        if (IsDefending)
        {
            // Use the unit's Defense stat (which comes from StatsCalculator)
            float effectiveDefense = Mathf.Max(0f, Defense); // Ensure defense isn't negative

            // Calculate the damage multiplier using the formula
            // Multiplier = DefenseConstant / (DefenseConstant + Defense)
            // Ensures division by zero is impossible if _defenseConstant >= 1 and effectiveDefense >= 0
            float damageMultiplier = _defenseConstant / (_defenseConstant + effectiveDefense);

            // Apply the multiplier to the incoming damage
            float reducedDamageFloat = actualDamage * damageMultiplier;

            // Round to the nearest integer and ensure at least 1 damage is dealt
            int reducedDamageInt = Mathf.Max(1, Mathf.RoundToInt(reducedDamageFloat));

            // Log the details for debugging/tuning
            Debug.Log($"[{UnitName}] Defending! Defense:{Defense}, Multiplier:{damageMultiplier:F2}. Damage reduced from {actualDamage} to {reducedDamageInt}.");

            // Update the actual damage to be applied
            actualDamage = reducedDamageInt;
        }
        // --- End Defense Reduction ---

        // Pass the final (potentially reduced) damage to the HealthComponent
        return _healthComponent.TakeDamage(actualDamage);
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

    /// <summary> Sets the unit's state to Defending. </summary>
    public void StartDefending()
    {
        IsDefending = true;
        // Optional: Add visual feedback later (e.g., enable shield icon, play sound)
        // Debug.Log($"[{UnitName}] Started Defending.");
    }

    /// <summary> Resets the unit's Defending state to false. </summary>
    public void EndDefending()
    {
        if (IsDefending) // Only log/update visuals if it was actually true
        {
            IsDefending = false;
            // Optional: Add visual feedback later (e.g., disable shield icon)
            // Debug.Log($"[{UnitName}] Ended Defending.");
        }
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
    /// The result can be 0 if multipliers/level are low.
    /// </summary>
    /// <param name="minMultiplier">Minimum heal multiplier (applied to level).</param>
    /// <param name="maxMultiplier">Maximum heal multiplier (applied to level).</param>
    /// <returns>A random heal amount within the calculated range.</returns>
    public int GetRandomHealAmount(float minMultiplier, float maxMultiplier)
    {
        int effectiveLevel = Mathf.Max(1, Level);
        Debug.Log($"[GetRandomHealAmount] Level: {Level}, EffectiveLevel: {effectiveLevel}, MinMult: {minMultiplier}, MaxMult: {maxMultiplier}"); // DEBUG

        int minHeal = Mathf.FloorToInt(effectiveLevel * minMultiplier);
        int maxHeal = Mathf.CeilToInt(effectiveLevel * maxMultiplier);
        Debug.Log($"[GetRandomHealAmount] Calculated Base Range: [{minHeal}, {maxHeal}]"); // DEBUG

        if (minHeal > maxHeal)
        {
            Debug.LogWarning($"[GetRandomHealAmount] minHeal ({minHeal}) > maxHeal ({maxHeal}). Clamping minHeal."); // DEBUG
            minHeal = maxHeal;
        }

        minHeal = Mathf.Max(0, minHeal); // Allow 0 healing initially
        maxHeal = Mathf.Max(0, maxHeal);
        Debug.Log($"[GetRandomHealAmount] Clamped Range (>=0): [{minHeal}, {maxHeal}]"); // DEBUG

        int result = UnityEngine.Random.Range(minHeal, maxHeal + 1);
        Debug.Log($"[GetRandomHealAmount] Random.Range({minHeal}, {maxHeal + 1}) Result: {result}"); // DEBUG
        return result;
    }

    /// <summary> Tells the animator controller whether it's this unit's turn. </summary>
    public void SetAnimatorIsPlayerTurn(bool isTurn)
    {
        _animatorController?.SetIsPlayerTurn(isTurn); // Call the method on the controller script
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

    /// <summary> Gets whether the unit is currently in a defending state. </summary>
    public bool IsDefending { get; private set; } = false; // Default to false

    /// <summary> Gets the calculated defense value of the unit for its current level. </summary>
    public int Defense => _defenseValue;

    #endregion
}