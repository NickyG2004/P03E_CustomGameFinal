using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUDRefactored : MonoBehaviour
{
    public bool debugMode = false;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider hpSlider;

    public void SetHUD(UnitRefactored unit)
    {
        if (debugMode) Debug.Log("BattleHUDRefactored: Setting HUD for " + unit.unitName);
        nameText.text = unit.unitName;
        levelText.text = "Lvl: " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHP;
        hpSlider.value = unit.currentHP;
    }

    public void SetHP(int hp)
    {
        if (debugMode) Debug.Log("BattleHUDRefactored: Setting HP to " + hp);
        hpSlider.value = hp;
    }
}
