using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Identity")]
    public string unitName;
    public int unitLevel = 1;

    [Header("Base Stats (at Level 1)")]
    public int baseMaxHP;
    public int baseDamage;

    [Header("Logarithmic Growth Factors")]
    [Tooltip("Controls how steeply HP grows.  Higher -> larger HP gains.")]
    public float hpLogGrowthFactor = 10f;
    [Tooltip("Controls how steeply Damage grows.")]
    public float damageLogGrowthFactor = 2f;

    [Header("Runtime Stats")]
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }
    public int damage { get; private set; }

    void Awake()
    {
        // Initialize runtime stats from your base values
        RecalculateStats();
        currentHP = maxHP;
    }

    /// <summary>
    /// Call this when the unit levels up.  You can pass >1 if you grant multiple levels.
    /// </summary>
    public void LevelUp(int levels = 1)
    {
        unitLevel = Mathf.Max(1, unitLevel + levels);
        RecalculateStats();

        // Heal to full on level up (optional)
        currentHP = maxHP;
    }

    /// <summary>
    /// Recalculates maxHP and damage using a logarithmic curve.
    /// </summary>
    private void RecalculateStats()
    {
        // Mathf.Log uses the natural logarithm (base e).
        // We add +1 so that at level 1, log(2) still gives a small bump.
        maxHP = Mathf.CeilToInt(baseMaxHP + hpLogGrowthFactor * Mathf.Log(unitLevel + 1));
        damage = Mathf.CeilToInt(baseDamage + damageLogGrowthFactor * Mathf.Log(unitLevel + 1));
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        return currentHP <= 0;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }
}

