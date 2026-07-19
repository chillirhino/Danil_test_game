using UnityEngine;

/// <summary>Persistent level progress (PlayerPrefs): which levels are unlocked and which are
/// completed (beaten at least once). Level 1 is always unlocked.</summary>
public static class LevelProgress
{
    private const string Key = "unlockedLevels";
    private const string DoneKey = "completedLevels"; // bitmask: bit (level-1) set = that level is completed

    static LevelProgress()
    {
        // One-time migration for saves made before completion tracking existed: every level
        // below the highest unlocked one must have been beaten to unlock it, so mark them done.
        if (!PlayerPrefs.HasKey(DoneKey))
        {
            int unlocked = Mathf.Max(1, PlayerPrefs.GetInt(Key, 1));
            int mask = 0;
            for (int lvl = 1; lvl < unlocked && lvl <= 31; lvl++) mask |= (1 << (lvl - 1));
            PlayerPrefs.SetInt(DoneKey, mask);
            PlayerPrefs.Save();
        }
    }

    public static int Unlocked => Mathf.Max(1, PlayerPrefs.GetInt(Key, 1));

    public static bool IsUnlocked(int level) => level <= Unlocked;

    /// <summary>True if the player has finished this level at least once.</summary>
    public static bool IsCompleted(int level)
    {
        if (level < 1 || level > 31) return false;
        return (PlayerPrefs.GetInt(DoneKey, 0) & (1 << (level - 1))) != 0;
    }

    /// <summary>Mark a level completed → record it as beaten AND unlock the next one.</summary>
    public static void Complete(int level)
    {
        if (level >= 1 && level <= 31)
        {
            int mask = PlayerPrefs.GetInt(DoneKey, 0) | (1 << (level - 1));
            PlayerPrefs.SetInt(DoneKey, mask);
        }
        if (level + 1 > Unlocked)
            PlayerPrefs.SetInt(Key, level + 1);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.DeleteKey(DoneKey);
        PlayerPrefs.Save();
    }
}
