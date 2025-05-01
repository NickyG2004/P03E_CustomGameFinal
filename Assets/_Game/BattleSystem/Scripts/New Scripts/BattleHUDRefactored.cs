// -----------------------------------------------------------------------------
// Filename: BattleHUDRefactored.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Updates on-screen HUD elements (unit name, level, HP bar) for a combat unit.
// Ensures UI components are assigned and handles data validation.
// -----------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the display of a unit's core information (name, level, health)
/// on the battle UI.
/// </summary>
public class BattleHUDRefactored : MonoBehaviour
{
    #region Inspector Fields

    [Header("Debug")]
    [Tooltip("Enable detailed logging for HUD updates.")]
    [SerializeField] private bool _debugMode = false;

    [Header("UI Elements")]
    [Tooltip("TextMeshPro component for displaying the unit's name.")]
    [SerializeField] private TextMeshProUGUI _nameText;

    [Tooltip("TextMeshPro component for displaying the unit's level.")]
    [SerializeField] private TextMeshProUGUI _levelText;

    [Tooltip("UI Slider component representing the unit's health.")]
    [SerializeField] private Slider _hpSlider;

    #endregion

    #region Public API

    /// <summary>
    /// Sets all HUD elements based on the provided unit's data.
    /// Logs errors if UI components or the unit are not assigned.
    /// </summary>
    /// <param name="unit">The unit whose data should be displayed.</param>
    public void SetHUD(UnitRefactored unit)
    {
        // Validate UI components
        if (_nameText == null || _levelText == null || _hpSlider == null)
        {
            Debug.LogError("[BattleHUD] Required UI components are not assigned in the inspector! Cannot set HUD.", this);
            return;
        }

        // Validate Unit data
        if (unit == null)
        {
            Debug.LogError("[BattleHUD] Cannot set HUD for a null unit!", this);
            return;
        }

        // Log if debugging
        if (_debugMode)
        {
            Debug.Log($"[BattleHUD] Setting HUD for {unit.UnitName}: Lvl={unit.Level}, HP={unit.CurrentHP}/{unit.MaxHP}", this);
        }

        // Update UI elements
        _nameText.text = unit.UnitName;
        _levelText.text = $"Lvl: {unit.Level}"; // Used string interpolation
        _hpSlider.maxValue = unit.MaxHP;
        _hpSlider.value = unit.CurrentHP;
    }

    /// <summary>
    /// Updates only the HP slider value.
    /// Logs an error if the HP slider component is not assigned.
    /// </summary>
    /// <param name="hp">The current HP value to display.</param>
    public void SetHP(int hp)
    {
        // Validate UI component
        if (_hpSlider == null)
        {
            Debug.LogError("[BattleHUD] HP Slider component is not assigned in the inspector! Cannot set HP.", this);
            return;
        }

        // Log if debugging
        if (_debugMode)
        {
            Debug.Log($"[BattleHUD] Setting HP to {hp}", this);
        }

        // Update slider value
        _hpSlider.value = hp;
    }

    #endregion
}