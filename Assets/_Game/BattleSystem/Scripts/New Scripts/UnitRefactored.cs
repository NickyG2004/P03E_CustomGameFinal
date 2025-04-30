using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a combat unit with level-based stats (HP, damage, speed)
/// and provides methods for combat actions.
/// </summary>
// [RequireComponent(typeof(StatsCalculator), typeof(HealthComponent))]
public class UnitRefactored : MonoBehaviour
{
    [Header("Display")]
    [Tooltip("Name of the unit displayed in the UI.")]
    public string unitName;

    [Header("Stats Configuration")]
    [Tooltip("Reference to the ScriptableObject that calculates base HP, damage, and speed by level.")]
    [SerializeField] private StatsCalculator statsCalculator;

    [Header("Health Component")]
    [Tooltip("Component that tracks and manages HP.")]
    [SerializeField] private HealthComponent healthComponent;

    // Made public so UI can bind directly; acts as the source for Level property
    public int unitLevel;
    private int damageValue;
    private int speedValue;

    private void Awake()
    {
        // Ensure references are set
        if (statsCalculator == null)
            statsCalculator = GetComponent<StatsCalculator>();
        if (healthComponent == null)
            healthComponent = GetComponent<HealthComponent>();
    }

    /// <summary>
    /// Initializes this unit at the given level, recalculating HP, damage, and speed.
    /// </summary>
    /// <param name="level">Level to initialize at</param>
    public void InitializeLevel(int level)
    {
        unitLevel = level;
        // Pull all stats from the calculator in one call
        statsCalculator.CalculateStats(
            level,
            out int maxHP,
            out int dmg,
            out int spd
        );

        // Apply HP and store internal values
        healthComponent.Initialize(maxHP);
        damageValue = dmg;
        speedValue = spd;
    }

    #region Public Properties

    public int Level => unitLevel;
    public int maxHP => healthComponent.maxHP;
    public int currentHP => healthComponent.currentHP;
    public int BaseDamage => damageValue;
    public int Speed => speedValue;

    #endregion

    #region Combat Rolls

    /// <summary>
    /// Rolls damage between minMultiplier * BaseDamage and maxMultiplier * BaseDamage,
    /// then applies a critical multiplier if the roll meets critChance.
    /// </summary>
    public int RollDamage(
        float minMultiplier,
        float maxMultiplier,
        float critChance,
        float critMultiplier,
        out bool wasCrit
    )
    {
        int minD = Mathf.FloorToInt(BaseDamage * minMultiplier);
        int maxD = Mathf.CeilToInt(BaseDamage * maxMultiplier);
        int roll = Random.Range(minD, maxD + 1);

        wasCrit = Random.value < critChance;
        if (wasCrit)
            roll = Mathf.CeilToInt(roll * critMultiplier);

        return roll;
    }

    /// <summary>
    /// Returns a random healing roll based on unitLevel and multipliers.
    /// </summary>
    public int GetRandomHealAmount(float minMultiplier, float maxMultiplier)
    {
        int minHeal = Mathf.FloorToInt(unitLevel * minMultiplier);
        int maxHeal = Mathf.CeilToInt(unitLevel * maxMultiplier);
        return Random.Range(minHeal, maxHeal + 1);
    }

    #endregion

    #region Combat Helpers

    /// <summary>Apply damage to this unit via its HealthComponent.</summary>
    public void TakeDamage(int amount) => healthComponent.TakeDamage(amount);

    /// <summary>Heal this unit via its HealthComponent.</summary>
    public void Heal(int amount) => healthComponent.Heal(amount);

    /// <summary>Increase level and reinitialize stats.</summary>
    public void LevelUp(int levels = 1) => InitializeLevel(unitLevel + levels);

    #endregion
}