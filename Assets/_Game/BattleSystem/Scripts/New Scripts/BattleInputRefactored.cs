using UnityEngine;

[RequireComponent(typeof(BattleSystemRefactored), typeof(BattleActionsRefactored))]
public class BattleInputRefactored : MonoBehaviour
{
    BattleSystemRefactored bs;
    BattleActionsRefactored ba;

    void Awake()
    {
        bs = GetComponent<BattleSystemRefactored>();
        ba = GetComponent<BattleActionsRefactored>();
    }

    public void OnAttackButton()
    {
        if (bs.debugMode) Debug.Log("Player chose to attack");
        if (bs.state != BattleState.PLAYERTURN) return;
        StartCoroutine(ba.PlayerAttack());
    }

    public void OnHealButton()
    {
        if (bs.debugMode) Debug.Log("Player chose to heal");
        if (bs.state != BattleState.PLAYERTURN) return;
        StartCoroutine(ba.PlayerHeal());
    }
}
