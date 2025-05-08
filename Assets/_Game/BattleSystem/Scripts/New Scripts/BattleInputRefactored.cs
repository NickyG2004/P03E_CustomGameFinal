// -----------------------------------------------------------------------------
// Filename: BattleInputRefactored.cs
// (Provided as Refactored Example)
// -----------------------------------------------------------------------------
// Routes UI button callbacks (assigned in the Inspector) to the appropriate
// action coroutines in BattleActionsRefactored.
// Ensures player input is only processed during the PLAYERTURN state.
// -----------------------------------------------------------------------------

using UnityEngine;
// using System.Collections; // Not needed currently

/// <summary>
/// Connects UI Button onClick events (assigned in Inspector) to battle action methods
/// in the BattleActionsRefactored component. Ensures input is only processed
/// when the BattleSystemRefactored is in the PLAYERTURN state.
/// </summary>
[RequireComponent(typeof(BattleSystemRefactored), typeof(BattleActionsRefactored))]
public class BattleInputRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    //  Serialize Fields (Cached References)
    // -------------------------------------------------------------------------

    [Header("Sound Effects")]
    [SerializeField, Tooltip("Menu Button Sound.")]
    private AudioClip _MenuButtonSound = null;
    [SerializeField, Tooltip("Menu Button Sound Volume")]
    private float _MenuButtonSoundVolume = 1f;

    // -------------------------------------------------------------------------
    // Private Fields (Cached References)
    // -------------------------------------------------------------------------
    private BattleSystemRefactored _battleSystem;
    private BattleActionsRefactored _battleActions;
    private AudioSource _audioSource;

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Caches the required BattleSystem and BattleActions components.
    /// </summary>
    private void Awake()
    {
        _battleSystem = GetComponent<BattleSystemRefactored>();
        _battleActions = GetComponent<BattleActionsRefactored>();

        // Validate references
        if (_battleSystem == null) Debug.LogError("[BattleInput] BattleSystemRefactored component not found!", this);
        if (_battleActions == null) Debug.LogError("[BattleInput] BattleActionsRefactored component not found!", this);

        // Cache AudioSource for sound effects
        _audioSource = GetComponent<AudioSource>();
    }

    // -------------------------------------------------------------------------
    // UI Button Callbacks (Assign these methods to Button onClick events in Inspector)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the Attack button is clicked.
    /// Validates it's the player's turn before starting the PlayerAttackRoutine.
    /// </summary>
    public void OnAttackButton()
    {
        // Only allow action if the battle system and actions components are valid
        // and the game state is PLAYERTURN.
        if (_battleSystem != null && _battleActions != null && _battleSystem.State == BattleState.PLAYERTURN)
        {
            // Play button sound effect
            if (_audioSource != null && _MenuButtonSound != null)
            {
                _audioSource.PlayOneShot(_MenuButtonSound, _MenuButtonSoundVolume);
            }
            StartCoroutine(_battleActions.PlayerAttackRoutine());
        }
        else if (_battleSystem != null && _battleSystem.State != BattleState.PLAYERTURN)
        {
            // Optional: Log or give feedback if clicked during wrong turn
            // Debug.Log("[BattleInput] Attack button clicked, but not player's turn.");
        }
    }

    /// <summary>
    /// Called when the Heal button is clicked.
    /// Validates it's the player's turn before starting the PlayerHealRoutine.
    /// </summary>
    public void OnHealButton()
    {
        // Only allow action if the battle system and actions components are valid
        // and the game state is PLAYERTURN.
        if (_battleSystem != null && _battleActions != null && _battleSystem.State == BattleState.PLAYERTURN)
        {
            // Play button sound effect
            if (_audioSource != null && _MenuButtonSound != null)
            {
                _audioSource.PlayOneShot(_MenuButtonSound, _MenuButtonSoundVolume);
            }

            StartCoroutine(_battleActions.PlayerHealRoutine());
        }
        else if (_battleSystem != null && _battleSystem.State != BattleState.PLAYERTURN)
        {
            // Optional: Log or give feedback if clicked during wrong turn
            // Debug.Log("[BattleInput] Heal button clicked, but not player's turn.");
        }
    }

    /// <summary>
    /// Called when the Defend button is clicked.
    /// Validates it's the player's turn before starting the PlayerDefendRoutine.
    /// </summary>
    public void OnDefendButton()
    {
        if (_battleSystem != null && _battleActions != null && _battleSystem.State == BattleState.PLAYERTURN)
        {
            // Play button sound effect
            if (_audioSource != null && _MenuButtonSound != null)
            {
                _audioSource.PlayOneShot(_MenuButtonSound, _MenuButtonSoundVolume);
            }

            StartCoroutine(_battleActions.PlayerDefendRoutine());
        }
        else if (_battleSystem != null && _battleSystem.State != BattleState.PLAYERTURN)
        {
            // Optional: Log or give feedback if clicked during wrong turn
            // Debug.Log("[BattleInput] Defend button clicked, but not player's turn.");
        }
    }
}