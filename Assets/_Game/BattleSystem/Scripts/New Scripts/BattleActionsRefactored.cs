// -----------------------------------------------------------------------------
// Filename: BattleActionsRefactored.cs
// (Provided as Refactored Example)
// -----------------------------------------------------------------------------
// Handles player and enemy actions: attacks, healing, and turn hand-offs.
// Relies heavily on BattleSystemRefactored for state, data, and flow control.
// -----------------------------------------------------------------------------

using System; // Not strictly needed here unless Actions were used
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the execution logic for specific battle actions like Player Attack,
/// Player Heal, and Enemy Attack. Works in conjunction with BattleSystemRefactored.
/// </summary>
[RequireComponent(typeof(BattleSystemRefactored))]
public class BattleActionsRefactored : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Private Fields
    // -------------------------------------------------------------------------
    private BattleSystemRefactored _battleSystem; // Cached reference
    private AudioSource _audioSource; // Cached reference for sound effects

    [Header("VFX Prefabs")]
    [SerializeField, Tooltip("Particle effect prefab to spawn when the player heals.")]
    private GameObject _vfxHealBurstPlayer; // For the player's heal

    [Header("Message Timing")]
    [SerializeField, Tooltip("How long the initial 'Player attacks/heals' message should linger ideally."), Range(0.5f, 5f)]
    private float _actionAnnouncementLingerTime = 2.0f; // Default to 2 seconds, adjust as needed

    [Header("Sound Effects")]
    [SerializeField, Tooltip("Attack Action Sound.")]
    private AudioClip _attackActionSound = null;
    [SerializeField, Tooltip("Attack Action Volume")]
    private float _attackActionSoundVolume = 1f;
    [SerializeField, Tooltip("Miss Action Sound.")]
    private AudioClip _missActionSound = null;
    [SerializeField, Tooltip("Miss Action Volume")]
    private float _missActionSoundVolume = 1f;
    [SerializeField, Tooltip("Hurt Action Sound.")]
    private AudioClip _hurtActionSound = null;
    [SerializeField, Tooltip("Hurt Action Volume")]
    private float _hurtActionSoundVolume = 1f;
    [SerializeField, Tooltip("Crit Action Sound.")]
    private AudioClip _critctionSound = null;
    [SerializeField, Tooltip("Crit Action Volume")]
    private float _critActionSoundVolume = 1f;
    [SerializeField, Tooltip("Cast Heal Action Sound.")]
    private AudioClip _healActionSound = null;
    [SerializeField, Tooltip("Cast Heal Action Volume")]
    private float _healActionSoundVolume = 1f;
    [SerializeField, Tooltip("Heal Action Sound.")]
    private AudioClip _castHealActionSound = null;
    [SerializeField, Tooltip("Heal Action Volume")]
    private float _CastHealActionSoundVolume = 1f;
    [SerializeField, Tooltip("Defend Action Sound.")]
    private AudioClip _defendActionSound = null;
    [SerializeField, Tooltip("Defend Action Volume")]
    private float _defendActionSoundSound = 1f;

    // -------------------------------------------------------------------------
    // Unity Callbacks
    // -------------------------------------------------------------------------

    /// <summary>
    /// Caches the required BattleSystemRefactored component.
    /// </summary>
    private void Awake()
    {
        _battleSystem = GetComponent<BattleSystemRefactored>();
        if (_battleSystem == null)
        {
            Debug.LogError("[BattleActions] BattleSystemRefactored component not found on this GameObject!", this);
            this.enabled = false; // Disable if core dependency is missing
        }

        // Cache AudioSource for sound effects
        _audioSource = GetComponent<AudioSource>();
    }

    // -------------------------------------------------------------------------
    // Public Action Coroutines (Called by BattleInputRefactored or BattleSystemRefactored)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Coroutine for the player's attack sequence. Disables input, performs the
    /// attack roll, applies damage, checks for enemy defeat, displays messages,
    /// and transitions to the enemy's turn if the battle continues.
    /// </summary>
    public IEnumerator PlayerAttackRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        UnitAnimatorController playerAnimController = null;
        if (_battleSystem.PlayerUnit != null)
        {
            playerAnimController = _battleSystem.PlayerUnit.GetComponent<UnitAnimatorController>();
            if (playerAnimController == null) Debug.LogWarning("[PlayerAttackRoutine] PlayerUnit's UnitAnimatorController not found.", _battleSystem.PlayerUnit);
        }

        UnitAnimatorController enemyAnimController = null;
        if (_battleSystem.EnemyUnit != null)
        {
            enemyAnimController = _battleSystem.EnemyUnit.GetComponent<UnitAnimatorController>();
            if (enemyAnimController == null) Debug.LogWarning("[PlayerAttackRoutine] EnemyUnit's UnitAnimatorController not found.", _battleSystem.EnemyUnit);
        }

        _battleSystem.SetActionButtonsInteractable(false);

        StartCoroutine(ShowMessageRoutine($"You Attack!", _actionAnnouncementLingerTime));
        yield return new WaitForSeconds(1f); // Your adjusted wait for "You Attack!" visibility

        playerAnimController?.TriggerAttack(true);
        float attackImpactDelay = 0.15f;
        yield return new WaitForSeconds(attackImpactDelay);

        UnitRefactored attacker = _battleSystem.PlayerUnit;
        UnitRefactored defender = _battleSystem.EnemyUnit;
        int speedDifference = attacker.Speed - defender.Speed;
        float speedModifier = speedDifference * _battleSystem.AccuracySpeedFactor;
        float finalHitChance = Mathf.Clamp(
            _battleSystem.BaseHitChance + speedModifier,
            _battleSystem.MinHitChance,
            _battleSystem.MaxHitChance
        );
        bool didHit = UnityEngine.Random.value <= finalHitChance;

        if (!didHit)
        {
            if (_audioSource != null && _missActionSound != null) _audioSource.PlayOneShot(_missActionSound, _missActionSoundVolume);
            yield return StartCoroutine(ShowMessageRoutine("Your attack missed!", _actionAnnouncementLingerTime));
            yield return StartCoroutine(_battleSystem.EnemyTurnRoutine());
            yield break;
        }

        // --- Successful Hit Logic ---
        int damage = _battleSystem.PlayerUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out bool wasCrit
        );

        // --- IMMEDIATE ENEMY REACTION (VISUAL) ---
        // Trigger enemy's hurt animation NOW, before any blocking messages.
        enemyAnimController?.TriggerHurt();

        // --- SOUNDS & MESSAGES ---
        if (wasCrit)
        {
            // Play crit sound.
            // Assuming _critActionSoundVolume is defined.
            // If you have a dedicated _critActionSound AudioClip, use it.
            // Otherwise, your current method of using _hurtActionSound clip is used.
            // Example with a dedicated _critActionSound (you'd need to declare this field):
            // if (_audioSource != null && _critActionSound != null)
            // {
            //     _audioSource.PlayOneShot(_critActionSound, _critActionSoundVolume);
            // }
            // else if (_audioSource != null && _hurtActionSound != null) // Fallback to your current logic
            // {
            //     _audioSource.PlayOneShot(_hurtActionSound, _critActionSoundVolume);
            // }

            // Using your current sound logic for crit:
            if (_audioSource != null && _hurtActionSound != null)
            {
                _audioSource.PlayOneShot(_hurtActionSound, _critActionSoundVolume);
            }


            // Show crit message and WAIT. Enemy has already started its hurt animation.
            yield return StartCoroutine(ShowMessageRoutine("Critical hit!", _actionAnnouncementLingerTime));
        }
        else // Normal hit
        {
            if (_audioSource != null && _hurtActionSound != null)
            {
                _audioSource.PlayOneShot(_hurtActionSound, _hurtActionSoundVolume);
            }
            // Enemy's hurt animation was already triggered above.
        }

        // Apply damage to enemy and update their HUD
        ApplyDamage(_battleSystem.EnemyUnit, _battleSystem.EnemyHUD, damage);

        if (_battleSystem.CheckEnemyDefeatAndProcess())
        {
            yield return StartCoroutine(ShowMessageRoutine($"Dealt {damage} damage. Enemy defeated!", _actionAnnouncementLingerTime));
            yield break;
        }

        yield return StartCoroutine(ShowMessageRoutine($"You deal {damage} damage!", _actionAnnouncementLingerTime));
        yield return StartCoroutine(_battleSystem.EnemyTurnRoutine());
    }

    /// <summary>
    /// Coroutine for the enemy's attack sequence. Shows message, performs attack roll,
    /// applies damage to the player, displays damage message. Does NOT handle turn transition
    /// back to player (that's done by BattleSystemRefactored.EnemyTurnRoutine).
    /// </summary>
    public IEnumerator EnemyAttackRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // play attack sound
        //_audioSource?.PlayOneShot(_attackActionSound, _attackActionSoundVolume); // Play attack sound

        // Show enemy attack message
        yield return ShowMessageRoutine($"{_battleSystem.EnemyUnit.UnitName} attacks!", _battleSystem.FeedbackDelay);

        // Get Animator Controllers for both Enemy (attacker) and Player (defender)
        UnitAnimatorController enemyAnimController = null;
        if (_battleSystem.EnemyUnit != null)
        {
            enemyAnimController = _battleSystem.EnemyUnit.GetComponent<UnitAnimatorController>();
        }
        if (enemyAnimController == null && _battleSystem.EnemyUnit != null)
        {
            Debug.LogWarning($"[BattleActions] EnemyUnit '{_battleSystem.EnemyUnit.name}' is missing UnitAnimatorController component.", _battleSystem.EnemyUnit);
        }

        UnitAnimatorController playerAnimController = null;
        if (_battleSystem.PlayerUnit != null)
        {
            playerAnimController = _battleSystem.PlayerUnit.GetComponent<UnitAnimatorController>();
        }
        if (playerAnimController == null && _battleSystem.PlayerUnit != null)
        {
            Debug.LogWarning($"[BattleActions] PlayerUnit '{_battleSystem.PlayerUnit.name}' is missing UnitAnimatorController component.", _battleSystem.PlayerUnit);
        }

       

        // --- Trigger Enemy's Attack Tween ---
        enemyAnimController?.TriggerAttack(false); // Pass false for enemy lunge direction


        // --- Accuracy Check ---
        UnitRefactored attacker = _battleSystem.EnemyUnit;
        UnitRefactored defender = _battleSystem.PlayerUnit;

        // Calculate modifier based on speed difference
        int speedDifference = attacker.Speed - defender.Speed;
        float speedModifier = speedDifference * _battleSystem.AccuracySpeedFactor;

        // Calculate final hit chance (Base + Modifier), clamped within Min/Max bounds
        // Using same base chance for enemy, could vary later if needed
        float finalHitChance = Mathf.Clamp(
            _battleSystem.BaseHitChance + speedModifier,
            _battleSystem.MinHitChance,
            _battleSystem.MaxHitChance
        );

        // Optional Debug Log: Uncomment to see calculation details in Console
        // Debug.Log($"[EnemyAttack Accuracy] AttackerSpd:{attacker.Speed}, DefenderSpd:{defender.Speed} => SpdDiff:{speedDifference} => Mod:{speedModifier:P1}. BaseHit:{_battleSystem.BaseHitChance:P1} => FinalHit:{finalHitChance:P1}");

        // Perform the hit check using the final calculated chance
        bool didHit = UnityEngine.Random.value <= finalHitChance;

        if (!didHit)
        {
            // Play miss sound
            _audioSource?.PlayOneShot(_missActionSound, _missActionSoundVolume); // Play miss sound
            // Show miss message (use FeedbackDelay so it's visible before BattleSystem's TurnDelay kicks in)
            yield return StartCoroutine(ShowMessageRoutine($"{attacker.UnitName}'s attack missed!", _battleSystem.FeedbackDelay));
            // Skip damage/crit logic. The calling BattleSystem.EnemyTurnRoutine handles the next steps.
            yield break; // End this EnemyAttackRoutine early
        }
        // --- End Accuracy Check ---

        // Roll damage with critical hit calculation
        int damage = _battleSystem.EnemyUnit.RollDamage(
            _battleSystem.DamageMinMultiplier,
            _battleSystem.DamageMaxMultiplier,
            _battleSystem.CritChance,
            _battleSystem.CritMultiplier,
            out bool wasCrit
        );

        // Show critical hit message if occurred
        if (wasCrit)
        {
            //_audioSource?.PlayOneShot(_critctionSound, _critActionSoundVolume); // Play crit sound
            yield return ShowMessageRoutine("Enemy lands a critical hit!", _battleSystem.FeedbackDelay);
        }

        // Play hurt sound
        _audioSource?.PlayOneShot(_hurtActionSound, _hurtActionSoundVolume); // Play hurt sound

        // --- Trigger Player's Hurt Tween ---
        playerAnimController?.TriggerHurt();

        // Apply damage to player and update their HUD
        ApplyDamage(_battleSystem.PlayerUnit, _battleSystem.PlayerHUD, damage);

        // Show damage taken message (delay handled by caller - EnemyTurnRoutine)
        yield return ShowMessageRoutine($"You take {damage} damage!", 0); // No extra delay here
    }


    /// <summary>
    /// Coroutine for the player's heal sequence. Disables input, calculates heal amount,
    /// applies healing, checks for full health edge case, displays messages,
    /// and transitions to the enemy's turn.
    /// </summary>
    public IEnumerator PlayerHealRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // Get the Player's Animator Controller
        UnitAnimatorController playerAnimController = null;
        if (_battleSystem.PlayerUnit != null)
        {
            playerAnimController = _battleSystem.PlayerUnit.GetComponent<UnitAnimatorController>();
        }
        if (playerAnimController == null && _battleSystem.PlayerUnit != null)
        {
            Debug.LogWarning($"[BattleActions] PlayerUnit '{_battleSystem.PlayerUnit.name}' is missing UnitAnimatorController component.", _battleSystem.PlayerUnit);
        }

        _battleSystem.SetActionButtonsInteractable(false);

        // Check if already at full health
        if (_battleSystem.PlayerUnit.CurrentHP >= _battleSystem.PlayerUnit.MaxHP)
        {
            // Show the warning message
            yield return StartCoroutine(ShowMessageRoutine("Already at full health!", _battleSystem.TurnDelay));

            // --- CORRECTED LOGIC ---
            // Reset the prompt text directly
            if (_battleSystem.DialogueText != null)
            {
                _battleSystem.DialogueText.text = "Choose an action:";
            }
            // Re-enable the action buttons so the player can choose again
            _battleSystem.SetActionButtonsInteractable(true);
            // --- END CORRECTION ---

            // Exit the PlayerHealRoutine immediately, letting the player choose again.
            yield break;
        }

        // Calculate heal amount using the debugged Unit method
        int healValue = _battleSystem.PlayerUnit.GetRandomHealAmount(
            _battleSystem.HealMinMultiplier,
            _battleSystem.HealMaxMultiplier
        );
        Debug.Log($"[PlayerHealRoutine] Initial healValue from GetRandomHealAmount: {healValue}"); // DEBUG

        // Clamp heal amount so it doesn't exceed missing HP
        int missingHP = _battleSystem.PlayerUnit.MaxHP - _battleSystem.PlayerUnit.CurrentHP;
        Debug.Log($"[PlayerHealRoutine] Missing HP: {missingHP}"); // DEBUG
        int clampedHealValue = Mathf.Min(healValue, missingHP);
        Debug.Log($"[PlayerHealRoutine] HealValue after clamping to missing HP: {clampedHealValue}"); // DEBUG

        // Apply Option B: Ensure at least 1 HP is healed if calculated > 0
        int finalHealValue = clampedHealValue; // Start with clamped value
        if (finalHealValue > 0)
        {
            finalHealValue = Mathf.Max(1, finalHealValue);
            Debug.Log($"[PlayerHealRoutine] HealValue after Mathf.Max(1, value): {finalHealValue}"); // DEBUG
        }

        if (finalHealValue <= 0)
        {
            Debug.Log($"[PlayerHealRoutine] Final Heal Value is <= 0. Showing message and returning."); // DEBUG
            yield return StartCoroutine(ShowMessageRoutine("Unable to heal further!", _battleSystem.TurnDelay));
            yield return StartCoroutine(_battleSystem.PlayerTurnRoutine());
            yield break;
        }

        // --- Trigger Player's Heal Tween ---
        playerAnimController?.TriggerHeal();

        // play cast heal sound
        //_audioSource?.PlayOneShot(_castHealActionSound, _CastHealActionSoundVolume); // Play cast heal sound
        yield return StartCoroutine(ShowMessageRoutine("Healing...", _battleSystem.FeedbackDelay));

        // Apply final heal amount
        Debug.Log($"[PlayerHealRoutine] Applying final heal: {finalHealValue}"); // DEBUG
        _battleSystem.PlayerUnit.Heal(finalHealValue);
        _battleSystem.PlayerHUD.SetHP(_battleSystem.PlayerUnit.CurrentHP);

        // play heal sound
        //_audioSource?.PlayOneShot(_healActionSound, _healActionSoundVolume); // Play heal sound
        _audioSource?.PlayOneShot(_castHealActionSound, _CastHealActionSoundVolume); // Play cast heal sound


        // --- SPAWN HEAL PARTICLE EFFECT ---
        if (_vfxHealBurstPlayer != null && _battleSystem.PlayerUnit != null)
        {
            Transform spawnTransform = _battleSystem.PlayerUnit.transform.Find("temp_Player_Art"); 
            if (spawnTransform == null) spawnTransform = _battleSystem.PlayerUnit.transform;
            Instantiate(_vfxHealBurstPlayer, spawnTransform.position, Quaternion.identity);
            Debug.Log($"[BattleActions] Spawned Heal VFX at {_battleSystem.PlayerUnit.transform.position}");
        }
        else if (_vfxHealBurstPlayer == null)
        {
            Debug.LogWarning("[BattleActions] _vfxHealBurstPlayer prefab not assigned in Inspector. Cannot spawn heal particle effect.");
        }
        // --- END SPAWN HEAL PARTICLE EFFECT ---

        //_battleSystem.PlayerUnit?.SetAnimatorIsPlayerTurn(false); // Tell animator player's turn is ending

        yield return StartCoroutine(ShowMessageRoutine($"Recovered {finalHealValue} HP!", _battleSystem.TurnDelay));
        yield return StartCoroutine(_battleSystem.EnemyTurnRoutine());
    }

    /// <summary>
    /// Coroutine for the player's defend sequence. Disables input, sets the
    /// player's state to defending, shows a message, and transitions to the enemy's turn.
    /// </summary>
    public IEnumerator PlayerDefendRoutine()
    {
        if (!ValidateBattleSystemReference()) yield break;

        // Get the Player's Animator Controller
        UnitAnimatorController playerAnimController = null;
        if (_battleSystem.PlayerUnit != null)
        {
            playerAnimController = _battleSystem.PlayerUnit.GetComponent<UnitAnimatorController>();
        }
        if (playerAnimController == null && _battleSystem.PlayerUnit != null)
        {
            Debug.LogWarning($"[BattleActions] PlayerUnit '{_battleSystem.PlayerUnit.name}' is missing UnitAnimatorController component.", _battleSystem.PlayerUnit);
        }

        // Disable input and set defending state
        _battleSystem.SetActionButtonsInteractable(false);

        // --- Trigger Player's Defend Tween ---
        playerAnimController?.TriggerDefend();

        // play defend sound
        _audioSource?.PlayOneShot(_defendActionSound, _defendActionSoundSound); // Play defend sound

        _battleSystem.PlayerUnit.StartDefending();


        //_battleSystem.PlayerUnit?.SetAnimatorIsPlayerTurn(false); // Tell animator player's turn is ending

        // Show message and transition turn
        yield return StartCoroutine(ShowMessageRoutine("You brace yourself!", _battleSystem.TurnDelay));
        yield return StartCoroutine(_battleSystem.EnemyTurnRoutine()); // Start enemy's turn
    }

    // -------------------------------------------------------------------------
    // Private Helper Methods
    // -------------------------------------------------------------------------

    /// <summary> Checks if the BattleSystem reference is valid. </summary>
    /// <returns>True if valid, false otherwise.</returns>
    private bool ValidateBattleSystemReference()
    {
        if (_battleSystem == null)
        {
            Debug.LogError("[BattleActions] BattleSystem reference is null! Cannot perform action.", this);
            return false;
        }
        if (_battleSystem.PlayerUnit == null || _battleSystem.EnemyUnit == null)
        {
            Debug.LogError("[BattleActions] BattleSystem units are not initialized! Cannot perform action.", this);
            return false;
        }
        return true;
    }


    /// <summary>
    /// Displays a message in the BattleSystem's dialogue text UI for a set duration.
    /// </summary>
    /// <param name="message">The text message to display.</param>
    /// <param name="delay">Duration in seconds to show the message.</param>
    private IEnumerator ShowMessageRoutine(string message, float delay)
    {
        if (_battleSystem?.DialogueText != null) // Safe check
        {
            _battleSystem.DialogueText.text = message;
            if (delay > 0) yield return new WaitForSeconds(delay);
        }
        else
        {
            Debug.LogWarning($"[BattleActions] Cannot show message '{message}', BattleSystem or DialogueText is null.", this);
        }
    }

    /// <summary>
    /// Applies damage to a target unit and updates its corresponding HUD.
    /// </summary>
    /// <param name="target">The UnitRefactored component receiving damage.</param>
    /// <param name="hud">The BattleHUDRefactored component to update.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    private void ApplyDamage(UnitRefactored target, BattleHUDRefactored hud, int damage)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
            if (hud != null)
            {
                hud.SetHP(target.CurrentHP); // Update HUD after taking damage
            }
            else
            {
                Debug.LogWarning($"[BattleActions] HUD not assigned for target '{target.name}', cannot update HP display.", this);
            }
        }
        else
        {
            Debug.LogError("[BattleActions] ApplyDamage called with a null target unit!", this);
        }
    }
}