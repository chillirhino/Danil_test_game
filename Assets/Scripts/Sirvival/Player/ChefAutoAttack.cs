using UnityEngine;

namespace Sirvival
{
    /// <summary>Auto-fires the projectile prefab at the nearest enemy on a fireRate interval.</summary>
    public class ChefAutoAttack : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        private float _next;

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.State != RunState.Playing || projectilePrefab == null) return;
            if (Time.time < _next) return;

            var target = Nearest();
            if (target == null) return;

            _next = Time.time + 1f / Mathf.Max(0.01f, rm.Stats.fireRate);
            Vector2 dir = ((Vector2)(target.position - transform.position)).normalized;

            int n = Mathf.Max(1, rm.Stats.projectiles);
            const float spread = 14f; // degrees between shots in the fan
            for (int i = 0; i < n; i++)
            {
                float off = (i - (n - 1) * 0.5f) * spread;
                Vector2 d = Quaternion.Euler(0, 0, off) * dir;
                var p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                p.Launch(d, rm.Stats.projectileSpeed, rm.Stats.damage, 3f);
            }
        }

        private Transform Nearest()
        {
            Transform best = null;
            float bestSqr = float.MaxValue;
            foreach (var e in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                float d = ((Vector2)(e.transform.position - transform.position)).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = e.transform; }
            }
            return best;
        }
    }
}
