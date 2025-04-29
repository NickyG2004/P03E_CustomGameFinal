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

    public bool TakeDamage(int dmg) => _healthComp.TakeDamage(dmg);
    public void Heal(int amt) => _healthComp.Heal(amt);

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