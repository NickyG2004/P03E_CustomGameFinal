using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    // Set to true to enable debug logs
    public bool debugMode = false;

     [SerializeField] private TextMeshProUGUI nameText;
     [SerializeField] private TextMeshProUGUI levelText;
     [SerializeField] private Slider hpSlider;

    public void SetHUD(Unit unit)
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleHUD: Setting HUD for " + unit.unitName);
        }

        // Update the HUD elements with the unit's information.
        nameText.text = unit.unitName;
        levelText.text = "Lvl: " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(int hp)
    {
        // debug message.
        if (debugMode)
        {
            Debug.Log("BattleHUD: Setting HP to " + hp);
        }

        // Update the HP slider value.
        hpSlider.value = hp;
    }
}
