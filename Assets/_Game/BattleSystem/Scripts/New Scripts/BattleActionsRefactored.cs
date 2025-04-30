// -----------------------------------------------------------------------------
// BattleActionsRefactored.cs
// -----------------------------------------------------------------------------
// Implements the core battle coroutines: PlayerAttack, PlayerHeal, EnemyTurn.
// -----------------------------------------------------------------------------
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BattleSystemRefactored))]
public class BattleActionsRefactored : MonoBehaviour
{
    private BattleSystemRefactored _bs;

    private void Awake() => _bs = GetComponent<BattleSystemRefactored>();

    public IEnumerator PlayerAttack()
    {
        _bs.SetActionButtonsInteractable(false);
        _bs.dialogueText.text = "You attack!";
        yield return new WaitForSeconds(_bs.turnDelay);

        bool wasCrit;
        int dmg = _bs.playerUnit.RollDamage(
            _bs.damageMinMultiplier,
            _bs.damageMaxMultiplier,
            _bs.critChance,
            _bs.critMultiplier,
            out wasCrit
        );

        if (wasCrit)
        {
            _bs.dialogueText.text = "Critical hit!";
            yield return new WaitForSeconds(_bs.buttonDelay);
        }

        _bs.enemyUnit.TakeDamage(dmg);
        _bs.enemyHUD.SetHP(_bs.enemyUnit.currentHP);

        //  NEW: death check
        if (_bs.enemyUnit.currentHP <= 0)
        {
            _bs.state = BattleState.WON;
            yield return new WaitForSeconds(_bs.turnDelay);
            // new
            yield return _bs.StartCoroutine(_bs.EndBattle());

            yield break;
        }

        _bs.dialogueText.text = $"You deal {dmg} damage!";
        yield return new WaitForSeconds(_bs.turnDelay);

        yield return _bs.StartCoroutine(_bs.EnemyTurn());
    }


    public IEnumerator EnemyAttack()
    {
        _bs.dialogueText.text = $"{_bs.enemyUnit.unitName} attacks!";
        yield return new WaitForSeconds(_bs.turnDelay);

        bool wasCrit;
        int dmg = _bs.enemyUnit.RollDamage(
            _bs.damageMinMultiplier,  // or define separate fields if you want
            _bs.damageMaxMultiplier,
            _bs.critChance,
            _bs.critMultiplier,
            out wasCrit
        );

        if (wasCrit)
        {
            _bs.dialogueText.text = "Enemy lands a critical!";
            yield return new WaitForSeconds(_bs.buttonDelay);
        }

        _bs.playerUnit.TakeDamage(dmg);
        _bs.playerHUD.SetHP(_bs.playerUnit.currentHP);

        _bs.dialogueText.text = $"You take {dmg} damage!";
        yield return new WaitForSeconds(_bs.turnDelay);
    }



    public IEnumerator PlayerHeal()
    {
        // 1) Disable buttons
        _bs.SetActionButtonsInteractable(false);

        // 2) Full-HP guard
        if (_bs.playerUnit.currentHP >= _bs.playerUnit.maxHP)
        {
            _bs.playerHUD.SetHP(_bs.playerUnit.currentHP);
            _bs.dialogueText.text = "You’re already at full health!";
            yield return new WaitForSeconds(_bs.turnDelay);

            // Re-prompt
            yield return _bs.StartCoroutine(_bs.PlayerTurn());
            yield break;
        }

        // 3) Calculate level-scaled heal range
        int healValue = _bs.playerUnit.GetRandomHealAmount(_bs.healMinMultiplier, _bs.healMaxMultiplier);

        // (Optional) clamp so you never over-heal past MaxHP
        int missingHP = _bs.playerUnit.maxHP - _bs.playerUnit.currentHP;
        healValue = Mathf.Min(healValue, missingHP);

        // 4) Show “healing” text
        _bs.dialogueText.text = $"Healing…";
        yield return new WaitForSeconds(_bs.buttonDelay);

        // 5) Apply heal
        _bs.playerUnit.Heal(healValue);
        _bs.playerHUD.SetHP(_bs.playerUnit.currentHP);

        // 6) Report amount healed
        _bs.dialogueText.text = $"You recover {healValue} HP!";
        yield return new WaitForSeconds(_bs.turnDelay);

        // 7) Hand off to enemy
        yield return _bs.StartCoroutine(_bs.EnemyTurn());
    }

}