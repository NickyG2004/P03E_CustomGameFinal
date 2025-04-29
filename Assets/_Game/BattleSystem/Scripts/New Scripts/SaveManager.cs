// -----------------------------------------------------------------------------
// SaveManager.cs
// -----------------------------------------------------------------------------
// Centralizes all PlayerPrefs operations for level and high-score data.
// -----------------------------------------------------------------------------
using UnityEngine;

public static class SaveManager
{
    private const string PLAYER_LEVEL_KEY = "PlayerLevel";
    private const string ENEMY_LEVEL_KEY = "EnemyLevel";
    private const string BEST_LEVEL_KEY = "BestLevel";

    /// <summary>Current player level (default=1).</summary>
    public static int PlayerLevel
    {
        get => PlayerPrefs.GetInt(PLAYER_LEVEL_KEY, 1);
        set { PlayerPrefs.SetInt(PLAYER_LEVEL_KEY, value); PlayerPrefs.Save(); }
    }

    /// <summary>Current enemy level (default=1).</summary>
    public static int EnemyLevel
    {
        get => PlayerPrefs.GetInt(ENEMY_LEVEL_KEY, 1);
        set { PlayerPrefs.SetInt(ENEMY_LEVEL_KEY, value); PlayerPrefs.Save(); }
    }

    /// <summary>Highest level ever reached (default=1).</summary>
    public static int BestLevel
    {
        get => PlayerPrefs.GetInt(BEST_LEVEL_KEY, 1);
        set { PlayerPrefs.SetInt(BEST_LEVEL_KEY, value); PlayerPrefs.Save(); }
    }

    /// <summary>Clears saved player/enemy levels; retains BestLevel.</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PLAYER_LEVEL_KEY);
        PlayerPrefs.DeleteKey(ENEMY_LEVEL_KEY);
        PlayerPrefs.Save();
    }
}