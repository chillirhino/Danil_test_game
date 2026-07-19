using UnityEngine;

namespace Sirvival
{
    /// <summary>Pure wave-difficulty curves by elapsed seconds. Unit-tested.</summary>
    public static class WaveMath
    {
        /// Seconds between spawns: starts 1.2s, tightens over time, floors at 0.15s.
        public static float SpawnInterval(float t) => Mathf.Max(0.15f, 1.2f - t * 0.01f);

        /// Enemy HP grows slowly with time survived.
        public static float EnemyHp(float t) => 30f + t * 1.5f;

        /// Enemy speed grows slowly, capped.
        public static float EnemySpeed(float t) => Mathf.Min(4.5f, 2f + t * 0.02f);
    }
}
