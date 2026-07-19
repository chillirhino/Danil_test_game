using UnityEngine;

namespace Sirvival
{
    /// <summary>Chef HP with brief invulnerability after each hit; death ends the run.</summary>
    public class ChefHealth : MonoBehaviour
    {
        public float Current { get; private set; }
        public float Max => RunManager.Instance != null ? RunManager.Instance.Stats.maxHP : 100f;

        private float _invulnUntil;

        private void Start() { Current = Max; }

        public void TakeDamage(float amount)
        {
            if (Time.time < _invulnUntil || Current <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            _invulnUntil = Time.time + 0.5f;
            RunManager.Instance?.RaiseStatsChanged();
            if (Current <= 0f) RunManager.Instance?.EndRun();
        }
    }
}
