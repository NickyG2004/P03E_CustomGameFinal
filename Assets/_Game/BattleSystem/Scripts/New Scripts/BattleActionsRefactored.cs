using UnityEngine;
using System.Collections;

// Reqired to use the BattleSystemRefactored
[RequireComponent(typeof(BattleSystemRefactored))]
public class BattleActionsRefactored : MonoBehaviour
{
    BattleSystemRefactored bs;

    void Awake() => bs = GetComponent<BattleSystemRefactored>();

    public IEnumerator PlayerAttack()
    {
        if (bs.debugMode) Debug.Log("Player attacking enemy");

        bool isDead = bs.enemyUnit.TakeDamage(bs.playerUnit.damage);
        bs.enemyHUD.SetHP(bs.enemyUnit.currentHP);
        bs.dialogueText.text = "The Attack was Successful!";
        yield return new WaitForSeconds(bs.buttonDelay);

        if (isDead)
        {
            if (bs.debugMode) Debug.Log("Enemy defeated");
            bs.dialogueText.text = bs.enemyUnit.unitName + " has been defeated!";
            bs.state = BattleState.WON;
            yield return new WaitForSeconds(bs.turnDelay);
            bs.EndBattle();
        }
        else
        {
            if (bs.debugMode) Debug.Log($"Enemy HP = {bs.enemyUnit.currentHP}");
            bs.state = BattleState.ENEMYTURN;
            bs.dialogueText.text = "Enemy's turn!";
            yield return new WaitForSeconds(bs.turnDelay);
            StartCoroutine(EnemyTurn());
        }
    }

    public IEnumerator PlayerHeal()
    {
        if (bs.debugMode) Debug.Log("Player healing");
        bs.dialogueText.text = "Healing...";
        yield return new WaitForSeconds(bs.buttonDelay);

        bs.playerUnit.Heal(bs.healAmount);
        bs.playerHUD.SetHP(bs.playerUnit.currentHP);
        bs.dialogueText.text = $"You healed for {bs.healAmount} HP!";
        yield return new WaitForSeconds(bs.turnDelay);

        bs.state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        if (bs.debugMode) Debug.Log("Enemy's turn");
        bs.dialogueText.text = bs.enemyUnit.unitName + " is attacking!";
        yield return new WaitForSeconds(bs.buttonDelay);

        bool isDead = bs.playerUnit.TakeDamage(bs.enemyUnit.damage);
        bs.playerHUD.SetHP(bs.playerUnit.currentHP);
        yield return new WaitForSeconds(bs.buttonDelay);

        if (isDead)
        {
            if (bs.debugMode) Debug.Log("Player defeated");
            bs.dialogueText.text = "You have been defeated!";
            bs.state = BattleState.LOST;
            yield return new WaitForSeconds(bs.turnDelay);
            bs.EndBattle();
        }
        else
        {
            if (bs.debugMode) Debug.Log($"Player HP = {bs.playerUnit.currentHP}");
            bs.state = BattleState.PLAYERTURN;
            bs.dialogueText.text = "Your turn!";
            yield return new WaitForSeconds(bs.turnDelay);
            bs.PlayerTurn();
        }
    }
}
