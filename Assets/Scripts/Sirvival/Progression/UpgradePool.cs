using System.Collections.Generic;

namespace Sirvival
{
    /// <summary>Pure, deterministic-by-seed upgrade drafting. Unit-tested.</summary>
    public static class UpgradePool
    {
        private static readonly Upgrade[] All =
        {
            new Upgrade("dmg",   "Sharper Blade",  "+25% damage to all enemies", "sauce",  Rarity.Rare,   s => s.damage *= 1.25f),
            new Upgrade("rate",  "Quick Hands",    "+20% fire rate",             "pin",    Rarity.Common, s => s.fireRate *= 1.20f),
            new Upgrade("spd",   "Light Feet",     "+12% move speed",            "boots",  Rarity.Common, s => s.moveSpeed *= 1.12f),
            new Upgrade("hp",    "Hearty Meal",    "+25 max HP",                 "heart",  Rarity.Rare,   s => s.maxHP += 25f),
            new Upgrade("mag",   "Big Tray",       "+30% pickup range",          "boots",  Rarity.Common, s => s.pickupRange *= 1.30f),
            new Upgrade("pspd",  "Long Throw",     "+20% projectile speed",      "arrow",  Rarity.Common, s => s.projectileSpeed *= 1.20f),
            // weapon progression
            new Upgrade("sauce", "Extra Sauce",    "+1 sauce projectile",        "sauce",  Rarity.Epic,   s => s.projectiles += 1),
            new Upgrade("aura",  "Bigger Aura",    "+25% garlic aura radius",    "garlic", Rarity.Rare,   s => s.auraRadiusMult *= 1.25f),
            new Upgrade("pin",   "Extra Rolling Pin", "+1 orbiting pin",         "pin",    Rarity.Epic,   s => s.extraPins += 1),
        };

        /// Deterministic Fisher–Yates shuffle by seed; returns the first `count` distinct upgrades.
        public static Upgrade[] RollChoices(int count, int seed)
        {
            var list = new List<Upgrade>(All);
            var rng = new System.Random(seed);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list.GetRange(0, System.Math.Min(count, list.Count)).ToArray();
        }
    }
}
