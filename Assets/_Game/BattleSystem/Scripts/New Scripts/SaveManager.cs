// -----------------------------------------------------------------------------
// Filename: SaveManager.cs
// (Refactored: 2025-04-30)
// -----------------------------------------------------------------------------
// Static utility class for managing persistent game data using PlayerPrefs.
// Handles getting/setting PlayerLevel, EnemyLevel, and BestLevel, ensuring
// values are valid and saved immediately. Provides a method to reset progress.
// -----------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Centralizes access to saved game data stored in PlayerPrefs.
/// Provides properties for player level, enemy level, and highest level reached,
/// along with a method to reset current progress.
/// </summary>
public static class SaveManager
{
    // --- Constants for PlayerPrefs Keys ---
    private const string PLAYER_LEVEL_KEY = "PlayerLevel";
    private const string ENEMY_LEVEL_KEY = "EnemyLevel";
    private const string BEST_LEVEL_KEY = "BestLevel";

    // --- Public Fields ---
    /// <summary>Set to true to enable detailed logging of save/load operations.</summary>
    public static bool EnableDebugLogging = false; // Default to off

    // --- Properties ---

    /// <summary>
    /// Gets or sets the player's current level. Defaults to 1 if not set.
    /// Value is clamped to be at least 1 when set. Saves PlayerPrefs immediately.
    /// </summary>
    public static int PlayerLevel
    {
        get => PlayerPrefs.GetInt(PLAYER_LEVEL_KEY, 1);
        set
        {
            int currentValue = PlayerLevel; // Get current value before setting
            int clampedValue = Mathf.Max(1, value); // Ensure level is never less than 1
            if (EnableDebugLogging && currentValue != clampedValue) // Log only if value changes
            {
                Debug.Log($"[SaveManager] Setting PlayerLevel: {clampedValue} (Previous: {currentValue})");
            }
            PlayerPrefs.SetInt(PLAYER_LEVEL_KEY, clampedValue);
            PlayerPrefs.Save(); // Ensure data is written immediately
        }
    }

    /// <summary>
    /// Gets or sets the enemy's current level. Defaults to 1 if not set.
    /// Value is clamped to be at least 1 when set. Saves PlayerPrefs immediately.
    /// </summary>
    public static int EnemyLevel
    {
        get => PlayerPrefs.GetInt(ENEMY_LEVEL_KEY, 1);
        set
        {
            int currentValue = EnemyLevel; // Get current value before setting
            int clampedValue = Mathf.Max(1, value); // Ensure level is never less than 1
            if (EnableDebugLogging && currentValue != clampedValue) // Log only if value changes
            {
                Debug.Log($"[SaveManager] Setting EnemyLevel: {clampedValue} (Previous: {currentValue})");
            }
            PlayerPrefs.SetInt(ENEMY_LEVEL_KEY, clampedValue);
            PlayerPrefs.Save(); // Ensure data is written immediately
        }
    }

    /// <summary>
    /// Gets or sets the highest level the player has ever reached. Defaults to 1 if not set.
    /// Value is clamped to be at least 1 when set. Saves PlayerPrefs immediately.
    /// </summary>
    public static int BestLevel
    {
        get => PlayerPrefs.GetInt(BEST_LEVEL_KEY, 1);
        set
        {
            int currentValue = BestLevel; // Get current value before setting
            int clampedValue = Mathf.Max(1, value); // Ensure best level is never less than 1
            // Optional: Only set if the new value is actually higher than the current BestLevel?
            // if (clampedValue > currentValue) { ... } // Current implementation allows setting it lower (but >= 1)
            if (EnableDebugLogging && currentValue != clampedValue) // Log only if value changes
            {
                Debug.Log($"[SaveManager] Setting BestLevel: {clampedValue} (Previous: {currentValue})");
            }
            PlayerPrefs.SetInt(BEST_LEVEL_KEY, clampedValue);
            PlayerPrefs.Save(); // Ensure data is written immediately
        }
    }

    // --- Public Methods ---

    /// <summary>
    /// Resets the saved PlayerLevel and EnemyLevel back to their defaults (by deleting the keys).
    /// Does NOT reset BestLevel. Saves PlayerPrefs immediately.
    /// </summary>
    public static void ResetProgress()
    {
        if (EnableDebugLogging)
        {
            Debug.Log($"[SaveManager] Resetting progress. Deleting keys: {PLAYER_LEVEL_KEY}, {ENEMY_LEVEL_KEY}");
        }
        PlayerPrefs.DeleteKey(PLAYER_LEVEL_KEY);
        PlayerPrefs.DeleteKey(ENEMY_LEVEL_KEY);
        PlayerPrefs.Save(); // Ensure deletions are saved
    }

    /// <summary>
    /// Resets ALL saved data managed by this class, including PlayerLevel, EnemyLevel, and BestLevel.
    /// Use with caution! Saves PlayerPrefs immediately.
    /// </summary>
    public static void ResetAllProgress()
    {
        if (EnableDebugLogging)
        {
            Debug.LogWarning($"[SaveManager] Resetting ALL progress! Deleting keys: {PLAYER_LEVEL_KEY}, {ENEMY_LEVEL_KEY}, {BEST_LEVEL_KEY}");
        }
        PlayerPrefs.DeleteKey(PLAYER_LEVEL_KEY);
        PlayerPrefs.DeleteKey(ENEMY_LEVEL_KEY);
        PlayerPrefs.DeleteKey(BEST_LEVEL_KEY);
        PlayerPrefs.Save(); // Ensure deletions are saved
    }
}