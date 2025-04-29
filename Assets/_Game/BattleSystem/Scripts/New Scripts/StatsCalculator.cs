// -----------------------------------------------------------------------------
// StatsCalculator.cs
// -----------------------------------------------------------------------------
// Performs logarithmic stat scaling based on level input.
// -----------------------------------------------------------------------------
using UnityEngine;

public class StatsCalculator : MonoBehaviour
{
    #region Serialized Fields
    [Header("Base Stats at Level 1")] public int baseMaxHP;
    public int baseDamage;

    [Header("Log Growth Factors")] public float hpLogGrowthFactor = 10f;
    public float damageLogGrowthFactor = 2f;
    #endregion

    #region Public API
    /// <summary>
    /// Calculate HP and damage for a given level.
    /// </summary>
    public void CalculateStats(int level, out int scaledHP, out int scaledDamage)
    {
        scaledHP = Mathf.CeilToInt(baseMaxHP + hpLogGrowthFactor * Mathf.Log(level + 1));
        scaledDamage = Mathf.CeilToInt(baseDamage + damageLogGrowthFactor * Mathf.Log(level + 1));
    }
    #endregion
}
