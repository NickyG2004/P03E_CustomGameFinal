// -----------------------------------------------------------------------------
// UnitRefactored.cs
// -----------------------------------------------------------------------------
// Represents any battle unit; interfaces with StatsCalculator & HealthComponent.
// -----------------------------------------------------------------------------
using UnityEngine;

[RequireComponent(typeof(StatsCalculator), typeof(HealthComponent))]
public class UnitRefactored : MonoBehaviour
{
    #region Serialized Fields
    [Header("Identity")] public string unitName;
    [Tooltip("Starting level of this unit")] public int unitLevel = 1;
    #endregion

    #region Private Fields
    private StatsCalculator _statsCalc;
    private HealthComponent _healthComp;
    private int _damageValue;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _statsCalc = GetComponent<StatsCalculator>();
        _healthComp = GetComponent<HealthComponent>();
        InitializeStats();
        _healthComp.Initialize(_healthComp.maxHP);
    }
    #endregion

    #region Public API
    public void InitializeLevel(int level)
    {
        unitLevel = Mathf.Max(1, level);
        InitializeStats();
        _healthComp.Initialize(_healthComp.maxHP);
    }

    public void LevelUp(int levels = 1)
    {
        int oldMax = _healthComp.maxHP;
        unitLevel = Mathf.Max(1, unitLevel + levels);
        InitializeStats();
        int gain = _healthComp.maxHP - oldMax;
        if (gain > 0) _healthComp.Heal(gain);
        SaveManager.PlayerLevel = unitLevel;
    }

    /// <summary>
    /// Re-calculates (base) damage for the current unitLevel and returns it.
    /// </summary>
    public int calcDamage
    {
        get
        {
            // re-run your stat calc each time
            _statsCalc.CalculateStats(unitLevel, out _, out int dmg);
            return dmg;
        }
    }

    /// <summary>
    /// Returns a random heal amount based on this unit’s level and the given multipliers.
    /// </summary>
    public int GetRandomHealAmount(float minMultiplier, float maxMultiplier)
    {
        // Compute the raw range
        int minHeal = Mathf.FloorToInt(Level * minMultiplier);
        int maxHeal = Mathf.CeilToInt(Level * maxMultiplier);

        // Roll and return
        return Random.Range(minHeal, maxHeal + 1);
    }

    /// <summary>
    /// Returns a random damage roll between minMultiplier*BaseDamage and maxMultiplier*BaseDamage,
    /// then applies a critical multiplier if roll < critChance.
    /// </summary>
    public int RollDamage(
        float minMultiplier,
        float maxMultiplier,
        float critChance,
        float critMultiplier,
        out bool wasCrit
    )
    {
        // 1) Base roll range
        int minD = Mathf.FloorToInt(BaseDamage * minMultiplier);
        int maxD = Mathf.CeilToInt(BaseDamage * maxMultiplier);
        int roll = Random.Range(minD, maxD + 1);

        // 2) Crit check
        wasCrit = Random.value < critChance;
        if (wasCrit)
        {
            roll = Mathf.CeilToInt(roll * critMultiplier);
        }

        return roll;
    }

    public bool TakeDamage(int dmg) => _healthComp.TakeDamage(dmg);
    public void Heal(int amt) => _healthComp.Heal(amt);

    /// <summary> The current, in battle level of this unit </summary>
    public int Level => unitLevel;

    /// <summary>Recalculates base damage via StatsCalculator.</summary>
    public int BaseDamage
    {
        get
        {
            _statsCalc.CalculateStats(unitLevel, out _, out int dmg);
            return dmg;
        }
    }

    public int maxHP => _healthComp.maxHP;
    public int currentHP => _healthComp.currentHP;
    public int damage => _damageValue;
    #endregion

    #region Helpers
    private void InitializeStats()
    {
        _statsCalc.CalculateStats(unitLevel, out int hp, out int dmg);
        _healthComp.SetMaxHP(hp);
        _damageValue = dmg;
    }
    #endregion
}