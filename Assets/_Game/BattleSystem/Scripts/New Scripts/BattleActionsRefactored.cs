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
        // Damage enemy and update HUD/text
        bool isDead = _bs.enemyUnit.TakeDamage(_bs.playerUnit.damage);
        _bs.enemyHUD.SetHP(_bs.enemyUnit.currentHP);
        _bs.dialogueText.text = "Attack successful!";
        yield return new WaitForSeconds(_bs.buttonDelay);

        if (isDead)
        {
            _bs.state = BattleState.WON;
            yield return new WaitForSeconds(_bs.turnDelay);
            _bs.EndBattle();
        }
        else
        {
            _bs.state = BattleState.ENEMYTURN;
            yield return new WaitForSeconds(_bs.turnDelay);
            yield return StartCoroutine(EnemyTurn());
        }
    }

    public IEnumerator PlayerHeal()
    {
        _bs.dialogueText.text = "Healing...";
        yield return new WaitForSeconds(_bs.buttonDelay);
        _bs.playerUnit.Heal(_bs.healAmount);
        _bs.playerHUD.SetHP(_bs.playerUnit.currentHP);
        _bs.dialogueText.text = $"Healed for {_bs.healAmount} HP!";
        yield return new WaitForSeconds(_bs.turnDelay);
        yield return StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        _bs.dialogueText.text = _bs.enemyUnit.unitName + " attacks!";
        yield return new WaitForSeconds(_bs.buttonDelay);
        bool isDead = _bs.playerUnit.TakeDamage(_bs.enemyUnit.damage);
        _bs.playerHUD.SetHP(_bs.playerUnit.currentHP);
        yield return new WaitForSeconds(_bs.buttonDelay);

        if (isDead)
        {
            _bs.state = BattleState.LOST;
            yield return new WaitForSeconds(_bs.turnDelay);
            _bs.EndBattle();
        }
        else
        {
            _bs.state = BattleState.PLAYERTURN;
            _bs.dialogueText.text = "Your turn!";
            yield return new WaitForSeconds(_bs.turnDelay);
            _bs.PlayerTurn();
        }
    }
}