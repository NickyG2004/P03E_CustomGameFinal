// HealthComponent.cs
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }

    /// <summary>Initialize both fields (call at start or on level-up)</summary>
    public void Initialize(int startingMaxHP)
    {
        maxHP = startingMaxHP;
        currentHP = startingMaxHP;
    }

    public void SetMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        return currentHP <= 0;
    }

    public void Heal(int amt)
    {
        currentHP = Mathf.Min(currentHP + amt, maxHP);
    }
}
