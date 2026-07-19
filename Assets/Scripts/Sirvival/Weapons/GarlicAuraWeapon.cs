using UnityEngine;

namespace Sirvival
{
    /// <summary>Constant close-range damage ring: periodically hurts every enemy in radius.</summary>
    public class GarlicAuraWeapon : MonoBehaviour
    {
        [SerializeField] private float radius = 2.2f;
        [SerializeField] private float interval = 0.6f;
        [SerializeField] private float damageMul = 0.5f; // fraction of Stats.damage per tick

        private float _next;
        private Transform _ring;

        private void Start()
        {
            var ring = new GameObject("GarlicRing");
            ring.transform.SetParent(transform, false);
            var sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = SirvivalAssets.Circle();
            sr.color = new Color(0.6f, 0.9f, 0.4f, 0.13f);
            sr.sortingOrder = -5;
            _ring = ring.transform;
        }

        private float CurrentRadius()
        {
            var rm = RunManager.Instance;
            return radius * (rm != null ? rm.Stats.auraRadiusMult : 1f);
        }

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.State != RunState.Playing) return;

            float r = CurrentRadius();
            // _circle sprite is ~0.64 world units in diameter at scale 1
            if (_ring != null) _ring.localScale = Vector3.one * (r * 2f / 0.64f);

            if (Time.time < _next) return;
            _next = Time.time + interval;

            float dmg = rm.Stats.damage * damageMul;
            var hits = Physics2D.OverlapCircleAll(transform.position, r);
            foreach (var col in hits)
            {
                var e = col.GetComponent<Enemy>();
                if (e != null) e.TakeDamage(dmg);
            }
        }
    }
}
