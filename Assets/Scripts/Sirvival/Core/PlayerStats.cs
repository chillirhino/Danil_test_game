using UnityEngine;

namespace Sirvival
{
    /// <summary>Live player stats. `baseStats` on RunManager is cloned into the run copy.</summary>
    [System.Serializable]
    public class PlayerStats
    {
        public float maxHP = 100f;
        public float moveSpeed = 5f;
        public float damage = 20f;
        public float fireRate = 1.5f;       // shots per second
        public float projectileSpeed = 9f;
        public float pickupRange = 1.5f;

        // weapon progression (raised by upgrades)
        public int projectiles = 1;         // sauce shots per volley
        public float auraRadiusMult = 1f;   // garlic aura radius multiplier
        public int extraPins = 0;           // extra rolling pins

        public PlayerStats Clone() => (PlayerStats)MemberwiseClone();
    }
}
