// StatsCalculator.cs
using UnityEngine;

public class StatsCalculator : MonoBehaviour
{
    [Header("Base Stats (at Level 1)")]
    public int baseMaxHP;
    public int baseDamage;

    [Header("Logarithmic Growth Factors")]
    public float hpLogGrowthFactor = 10f;
    public float damageLogGrowthFactor = 2f;

    /// <summary>
    /// Outputs scaled HP & damage via your logarithmic formula.
    /// </summary>
    public void CalculateStats(int level, out int scaledHP, out int scaledDamage)
    {
        scaledHP = Mathf.CeilToInt(baseMaxHP + hpLogGrowthFactor * Mathf.Log(level + 1));
        scaledDamage = Mathf.CeilToInt(baseDamage + damageLogGrowthFactor * Mathf.Log(level + 1));
    }
}
