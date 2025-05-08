// -----------------------------------------------------------------------------
// Filename: UnitAnimatorController.cs
// (Modified for always looping idle - IsPlayerTurn logic removed)
// (Further modification for robust Defend tint reset)
// -----------------------------------------------------------------------------
// Handles controlling DOTween sequences for a battle unit.
// Can also interact with a Unity Animator for base states like Idle if needed.
// -----------------------------------------------------------------------------

using UnityEngine;
using DG.Tweening; // Ensure DOTween is imported

[RequireComponent(typeof(Animator))] // Keep if your Idle is a Unity Animation state
public class UnitAnimatorController : MonoBehaviour
{
    #region Private Fields
    private Animator _animator; // Used if Animator component handles looping idle

    [Header("Tween Targets (Assign in Inspector)")]
    [SerializeField, Tooltip("The child GameObject that holds the main sprite and will be tweened.")]
    private Transform _artTransform;
    [SerializeField, Tooltip("The SpriteRenderer on the Art object, for color/fade tweens.")]
    private SpriteRenderer _artSpriteRenderer;

    // Animator Parameter Hashes (if using Animator for more than just idle)
    private readonly int _hashAttackTrigger = Animator.StringToHash("Attack");
    private readonly int _hashHealTrigger = Animator.StringToHash("Heal");
    private readonly int _hashDefendTrigger = Animator.StringToHash("Defend");
    private readonly int _hashHurtTrigger = Animator.StringToHash("Hurt");
    private readonly int _hashDefeatedTrigger = Animator.StringToHash("Defeated");

    // Color State Management for Defend Tint
    private Color _trueOriginalSpriteColor; // Captured in Awake
    private bool _isDefendTintActive = false;
    #endregion

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        // if (_animator == null) Debug.LogError(...); // Your existing null check

        if (_artTransform == null)
        {
            Transform artChild = transform.Find("Art");
            if (artChild != null) _artTransform = artChild;
            else Debug.LogError($"[UnitAnimatorController] Art Transform not assigned and not found as child 'Art' on {gameObject.name}!", this);
        }

        if (_artSpriteRenderer == null && _artTransform != null)
        {
            _artSpriteRenderer = _artTransform.GetComponentInChildren<SpriteRenderer>();
            if (_artSpriteRenderer == null) Debug.LogWarning($"[UnitAnimatorController] Art Sprite Renderer not found on {gameObject.name}. Color/fade tweens might not work.", this);
        }

        if (_artSpriteRenderer != null)
        {
            // Capture the color as it is when the game starts/object awakens
            _trueOriginalSpriteColor = _artSpriteRenderer.color;
            Debug.Log($"[{gameObject.name}] Awake: Stored _trueOriginalSpriteColor = {_trueOriginalSpriteColor}", this);
        }
        else
        {
            // If no sprite renderer, set a default to avoid errors, though color changes won't work
            _trueOriginalSpriteColor = Color.white;
            Debug.LogWarning($"[{gameObject.name}] Awake: _artSpriteRenderer is null. _trueOriginalSpriteColor defaulted to white.", this);
        }
    }

    public void TriggerAttack(bool isPlayerUnit = true)
    {
        if (_artTransform == null) return;
        _artTransform.DOKill(); // Kill previous transform tweens
        Vector3 originalPosition = _artTransform.localPosition;
        float lungeDirection = isPlayerUnit ? 1f : -1f;
        Sequence attackSequence = DOTween.Sequence();
        attackSequence.Append(_artTransform.DOLocalMoveX(originalPosition.x + (0.5f * lungeDirection), 0.15f).SetEase(Ease.OutQuad))
                      .Append(_artTransform.DOLocalMoveX(originalPosition.x, 0.25f).SetEase(Ease.InQuad));
    }

    public void TriggerHeal()
    {
        if (_artTransform == null || _artSpriteRenderer == null) return;
        _artTransform.DOKill();
        _artSpriteRenderer.DOKill(); // Kill previous color tweens specifically

        // Determine what color to return to after the green flash
        Color colorToReturnToAfterHealFlash = _isDefendTintActive ? _artSpriteRenderer.color : _trueOriginalSpriteColor;
        // Note: if _isDefendTintActive is true, _artSpriteRenderer.color *should* be the defend tint.

        Sequence healSequence = DOTween.Sequence();
        healSequence.Append(_artTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.4f, 5, 0.5f))
                  .Join(_artSpriteRenderer.DOColor(Color.green, 0.2f)) // Flash green
                  .Append(_artSpriteRenderer.DOColor(colorToReturnToAfterHealFlash, 0.2f)); // Return to appropriate color
    }

    public void TriggerDefend()
    {
        if (_artTransform == null) return; // SpriteRenderer check below

        _artTransform.DOKill(true); // Kill all tweens on artTransform, complete them if possible

        _artTransform.DOPunchScale(new Vector3(0f, -0.1f, 0), 0.3f, 3, 0.5f);

        if (_artSpriteRenderer != null)
        {
            _artSpriteRenderer.DOKill(true); // Kill previous color tweens, complete them
            Color defendColor = new Color(0.7f, 0.75f, 0.8f, _trueOriginalSpriteColor.a); // Use original alpha
            _artSpriteRenderer.color = defendColor; // Set color directly first
            _artSpriteRenderer.DOColor(defendColor, 0.15f).SetEase(Ease.OutQuad); // Optional: slight tween for visual flair if needed
            _isDefendTintActive = true;
            Debug.Log($"[{gameObject.name}] TriggerDefend: Applied defendColor ({defendColor}). _isDefendTintActive = true. TrueOriginal was {_trueOriginalSpriteColor}", this);
        }
    }

    public void TriggerHurt()
    {
        if (_artTransform == null || _artSpriteRenderer == null) return;
        _artTransform.DOKill(true); // Complete and kill transform tweens
        _artSpriteRenderer.DOKill(true); // Complete and kill color tweens

        Color colorBeforeHurt = _artSpriteRenderer.color;
        Debug.Log($"[{gameObject.name}] TriggerHurt: colorBeforeHurt = {colorBeforeHurt}", this);

        Sequence hurtSequence = DOTween.Sequence();
        hurtSequence.Append(_artTransform.DOShakePosition(0.3f, strength: new Vector3(0.2f, 0.1f, 0), vibrato: 10, randomness: 90, fadeOut: true))
                  .Join(_artSpriteRenderer.DOColor(Color.red, 0.1f)) // Flash red
                  .Append(_artSpriteRenderer.DOColor(colorBeforeHurt, 0.2f).SetDelay(0.1f)); // Return to color it was before getting hurt
    }

    public void TriggerDefeated(float duration = 1.0f)
    {
        if (_artSpriteRenderer != null)
        {
            _artSpriteRenderer.DOKill(true);
            _artSpriteRenderer.DOFade(0f, duration).SetEase(Ease.InQuad);
        }
        if (_artTransform != null)
        {
            _artTransform.DOKill(true);
            _artTransform.DOScale(Vector3.zero, duration * 1.2f).SetEase(Ease.InBack);
        }
    }

    /// <summary>
    /// Resets visual effects applied by TriggerDefend, like color tint,
    /// back to the true original sprite color.
    /// </summary>
    public void EndDefendVisuals()
    {
        if (_artSpriteRenderer != null)
        {
            if (_isDefendTintActive)
            {
                Debug.Log($"[UnitAnimatorController] EndDefendVisuals: Attempting to revert from DEFEND TINT. Current color before DOKill: {_artSpriteRenderer.color}, Target (trueOriginal): {_trueOriginalSpriteColor}");
                _artSpriteRenderer.DOKill(true); // Complete any ongoing tweens immediately AND kill them.
                _artSpriteRenderer.color = _trueOriginalSpriteColor; // Set color DIRECTLY.
                _isDefendTintActive = false;
                Debug.Log($"[UnitAnimatorController] EndDefendVisuals: Color FORCED to _trueOriginalSpriteColor. New current color: {_artSpriteRenderer.color}. _isDefendTintActive = {_isDefendTintActive}");
            }
            else
            {
                // If defend tint wasn't active, but we want to ensure it's the original color anyway (e.g. after complex interactions)
                // This might be too aggressive if other tints are meant to persist across turns.
                // For now, only revert if _isDefendTintActive was true.
                Debug.LogWarning($"[UnitAnimatorController] EndDefendVisuals: Called but _isDefendTintActive was false. Current color: {_artSpriteRenderer.color}. True original: {_trueOriginalSpriteColor}");
            }
        }
        else
        {
            Debug.LogError("[UnitAnimatorController] EndDefendVisuals: _artSpriteRenderer is null!");
        }
    }
}
