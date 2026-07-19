using UnityEngine;

namespace Sirvival
{
    /// <summary>Pure XP-curve math. Unit-tested via edit-mode assertion.</summary>
    public static class Leveling
    {
        /// XP required to advance FROM `level` to `level+1`.
        public static int XpForLevel(int level)
            => Mathf.RoundToInt(5f + (level - 1) * 4f + (level - 1) * (level - 1) * 0.6f);
    }
}
