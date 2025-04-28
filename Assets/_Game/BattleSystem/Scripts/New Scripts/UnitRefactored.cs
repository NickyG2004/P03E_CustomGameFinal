using UnityEngine;


// Require Components to ensure they are attached to the GameObject
[RequireComponent(typeof(StatsCalculator))]
[RequireComponent(typeof(HealthComponent))]

public class UnitRefactored : MonoBehaviour
{
    [Header("DebugMode")]
    public bool debugMode = false;

    [Header("Identity")]
    public string unitName;
    public int unitLevel = 1;

    StatsCalculator statsCalc;
    HealthComponent healthComp;
    int damageStat;

    void Awake()
    {
        statsCalc = GetComponent<StatsCalculator>();
        healthComp = GetComponent<HealthComponent>();
        RecalculateStats();
        healthComp.Initialize(maxHP);  // full heal at start
    }


    public void LevelUp(int levels = 1)
    {
        // 1) remember old max
        int oldMax = healthComp.maxHP;

        // 2) bump level and recalc stats (RecalculateStats calls SetMaxHP internally)
        unitLevel = Mathf.Max(1, unitLevel + levels);
        RecalculateStats();

        // 3) heal only the difference in max HP
        int diff = healthComp.maxHP - oldMax;
        if (diff > 0)
            healthComp.Heal(diff);
    }

    void RecalculateStats()
    {
        statsCalc.CalculateStats(unitLevel, out int hp, out int dmg);
        healthComp.SetMaxHP(hp);
        damageStat = dmg;
    }

    public void InitializeLevel(int level)
    {
        // clamp to at least 1
        unitLevel = Mathf.Max(1, level);

        // recalc stats & heal to full
        RecalculateStats();
        healthComp.Initialize(maxHP);
    }


    public bool TakeDamage(int dmg) { return healthComp.TakeDamage(dmg); }
    public void Heal(int amt) { healthComp.Heal(amt); }

    // Exposed for HUD reads:
    public int maxHP => healthComp.maxHP;
    public int currentHP => healthComp.currentHP;
    public int damage => damageStat;
}
