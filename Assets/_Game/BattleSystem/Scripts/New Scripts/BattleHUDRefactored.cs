// -----------------------------------------------------------------------------
// BattleHUDRefactored.cs
// -----------------------------------------------------------------------------
// Updates on-screen HUD elements (unit name, level, HP bar) for a Unit.
// -----------------------------------------------------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleHUDRefactored : MonoBehaviour
{
    #region Serialized Fields
    [Tooltip("Enable debug logging for HUD updates")] public bool debugMode = false;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider hpSlider;
    #endregion

    #region Public API
    public void SetHUD(UnitRefactored unit)
    {
        if (debugMode)
            Debug.Log($"HUD: {unit.unitName} L{unit.unitLevel} HP{unit.currentHP}/{unit.maxHP}");
        nameText.text = unit.unitName;
        levelText.text = "Lvl: " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(int hp)
    {
        if (debugMode) Debug.Log($"HUD: HP = {hp}");
        hpSlider.value = hp;
    }
    #endregion
}