// -----------------------------------------------------------------------------
// Filename: UnitAnimatorController.cs
// Date: 2025-05-04
// -----------------------------------------------------------------------------
// Handles controlling animations for a battle unit based on game state
// communicated from other scripts (like BattleSystem or BattleActions).
// Requires an Animator component on the same GameObject.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Manages triggering animations on a unit's Animator component.
/// Provides public methods to be called by other systems.
/// </summary>
[RequireComponent(typeof(Animator))]
public class UnitAnimatorController : MonoBehaviour
{
    #region Private Fields

    private Animator _animator;

    // Animator Parameter Hashes (more efficient than strings)
    private readonly int _hashIsPlayerTurn = Animator.StringToHash("IsPlayerTurn");
    private readonly int _hashAttackTrigger = Animator.StringToHash("Attack");
    private readonly int _hashHealTrigger = Animator.StringToHash("Heal");
    private readonly int _hashDefendTrigger = Animator.StringToHash("Defend");
    private readonly int _hashHurtTrigger = Animator.StringToHash("Hurt");
    private readonly int _hashDefeatedTrigger = Animator.StringToHash("Defeated");
    // Add hashes for any other parameters you create (e.g., IsDefending bool)

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Caches the Animator component reference.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError($"[UnitAnimatorController] Animator component not found on {gameObject.name}!", this);
            this.enabled = false; // Disable if Animator is missing
        }
    }

    #endregion

    #region Public Animation Triggers / State Setters

    /// <summary>
    /// Sets the 'IsPlayerTurn' boolean parameter on the Animator.
    /// Used to switch between looping idle and static idle (first frame).
    /// </summary>
    /// <param name="isTurn">True if it's this unit's turn, false otherwise.</param>
    public void SetIsPlayerTurn(bool isTurn)
    {
        if (_animator == null) return;
        _animator.SetBool(_hashIsPlayerTurn, isTurn);
        // Debug.Log($"[{gameObject.name}] Animator.SetBool(IsPlayerTurn, {isTurn})", this);
    }

    /// <summary> Triggers the Attack animation. </summary>
    public void TriggerAttack()
    {
        if (_animator == null) return;
        _animator.SetTrigger(_hashAttackTrigger);
        // Debug.Log($"[{gameObject.name}] Animator.SetTrigger(Attack)", this);
    }

    /// <summary> Triggers the Heal animation. </summary>
    public void TriggerHeal()
    {
        if (_animator == null) return;
        _animator.SetTrigger(_hashHealTrigger);
        // Debug.Log($"[{gameObject.name}] Animator.SetTrigger(Heal)", this);
    }

    /// <summary> Triggers the Defend animation/stance change. </summary>
    public void TriggerDefend()
    {
        if (_animator == null) return;
        _animator.SetTrigger(_hashDefendTrigger);
        // Debug.Log($"[{gameObject.name}] Animator.SetTrigger(Defend)", this);
        // If Defend is a persistent state (not just trigger), use SetBool("IsDefending", true/false)
    }

    /// <summary> Triggers the Hurt animation. </summary>
    public void TriggerHurt()
    {
        if (_animator == null) return;
        _animator.SetTrigger(_hashHurtTrigger);
        // Debug.Log($"[{gameObject.name}] Animator.SetTrigger(Hurt)", this);
    }

    /// <summary> Triggers the Defeated animation. </summary>
    public void TriggerDefeated()
    {
        if (_animator == null) return;
        _animator.SetTrigger(_hashDefeatedTrigger);
        // Debug.Log($"[{gameObject.name}] Animator.SetTrigger(Defeated)", this);
    }

    // Add methods for other triggers/parameters as needed (e.g., SetBool("IsDefending", bool))

    #endregion
}