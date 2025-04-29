// -----------------------------------------------------------------------------
// HealthComponent.cs
// -----------------------------------------------------------------------------
// Tracks max/current HP, handles damage and healing.
// -----------------------------------------------------------------------------
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    #region Public Properties
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }
    #endregion

    #region Public API
    /// <summary>Initialize max & current HP to starting value.</summary>
    public void Initialize(int startingMaxHP)
    {
        maxHP = startingMaxHP;
        currentHP = startingMaxHP;
    }

    /// <summary>Adjust max HP, clamp current HP if above new max.</summary>
    public void SetMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    /// <summary>Apply damage, return true if HP <= 0.</summary>
    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        return currentHP <= 0;
    }

    /// <summary>Heal by amount, not exceeding maxHP.</summary>
    public void Heal(int amt)
    {
        currentHP = Mathf.Min(currentHP + amt, maxHP);
    }
    #endregion
}