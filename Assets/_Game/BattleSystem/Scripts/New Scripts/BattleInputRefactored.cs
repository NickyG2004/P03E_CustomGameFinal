// -----------------------------------------------------------------------------
// BattleInputRefactored.cs
// -----------------------------------------------------------------------------
// Routes UI button callbacks to BattleActionsRefactored methods.
// -----------------------------------------------------------------------------
using UnityEngine;

[RequireComponent(typeof(BattleSystemRefactored), typeof(BattleActionsRefactored))]
public class BattleInputRefactored : MonoBehaviour
{
    private BattleSystemRefactored _bs;
    private BattleActionsRefactored _ba;

    private void Awake()
    {
        _bs = GetComponent<BattleSystemRefactored>();
        _ba = GetComponent<BattleActionsRefactored>();
    }

    public void OnAttackButton()
    {
        if (_bs.state != BattleState.PLAYERTURN) return;
        StartCoroutine(_ba.PlayerAttack());
    }

    public void OnHealButton()
    {
        if (_bs.state != BattleState.PLAYERTURN) return;
        StartCoroutine(_ba.PlayerHeal());
    }
}