// -----------------------------------------------------------------------------
// StatsCalculator.cs
// -----------------------------------------------------------------------------
// Performs logarithmic stat scaling based on level input.
// -----------------------------------------------------------------------------
using UnityEngine;

// [CreateAssetMenu(menuName = "Stats/StatsCalculator")]
public class StatsCalculator : MonoBehaviour
{
    #region Serialized Fields
    [Header("HP")]
    public int baseHP;
    public float hpGrowthFactor;      // used with Mathf.Log(level+1)

    [Header("Damage")]
    public int baseDamage;
    public float damageGrowthFactor;  // ditto

    [Header("Speed")]
    [Tooltip("Base speed at level 1")]
    public int baseSpeed;
    [Tooltip("Linear speed gained per level above 1")]
    public float speedGrowthPerLevel;

    #endregion

    #region Public API
    /// <summary>
    /// Calculates HP, damage, and speed for the given level.
    /// </summary>
    public void CalculateStats(int level, out int hp, out int dmg, out int spd)
    {
        // HP & Damage as before
        hp = Mathf.FloorToInt(baseHP * Mathf.Log(level + 1) * hpGrowthFactor);
        dmg = Mathf.CeilToInt(baseDamage * Mathf.Log(level + 1) * damageGrowthFactor);

        // Speed: baseSpeed + growth × (level–1)
        spd = Mathf.RoundToInt(baseSpeed + speedGrowthPerLevel * (level - 1));
    }
    #endregion
}
