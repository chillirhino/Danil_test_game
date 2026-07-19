using System.Collections.Generic;
using UnityEngine;

namespace Sirvival
{
    /// <summary>N kinematic "rolling pins" orbit the chef and damage enemies they sweep through.</summary>
    public class RollingPinWeapon : MonoBehaviour
    {
        [SerializeField] private int count = 2;
        [SerializeField] private float orbitRadius = 1.7f;
        [SerializeField] private float angularSpeed = 200f; // deg/s
        [SerializeField] private float damageMul = 0.8f;

        private Transform[] _pins;
        private float _angle;
        private readonly Dictionary<Enemy, float> _cooldown = new Dictionary<Enemy, float>();

        public float Damage => (RunManager.Instance != null ? RunManager.Instance.Stats.damage : 20f) * damageMul;

        private void Start() => Rebuild(count);

        private void Rebuild(int n)
        {
            if (_pins != null)
                foreach (var p in _pins) if (p != null) Destroy(p.gameObject);
            _pins = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                var pin = new GameObject("Pin" + i);
                pin.transform.SetParent(transform, false);
                var sr = pin.AddComponent<SpriteRenderer>();
                sr.sprite = SirvivalAssets.Px();
                sr.color = new Color(0.92f, 0.82f, 0.6f);
                sr.sortingOrder = 6;
                pin.transform.localScale = new Vector3(0.75f, 0.32f, 1f);
                var cc = pin.AddComponent<CircleCollider2D>();
                cc.radius = 0.4f; cc.isTrigger = true;
                var rb = pin.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                var hit = pin.AddComponent<PinDamage>();
                hit.owner = this;
                _pins[i] = pin.transform;
            }
        }

        private void Update()
        {
            var rm = RunManager.Instance;
            int desired = count + (rm != null ? rm.Stats.extraPins : 0);
            if (_pins == null || _pins.Length != desired) Rebuild(desired);
            if (_pins.Length == 0) return;
            _angle += angularSpeed * Time.deltaTime;
            for (int i = 0; i < _pins.Length; i++)
            {
                float a = (_angle + i * 360f / _pins.Length) * Mathf.Deg2Rad;
                _pins[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
            }
        }

        /// One hit per enemy per 0.4s so a lingering pin doesn't delete an enemy instantly.
        public bool CanHit(Enemy e)
        {
            if (_cooldown.TryGetValue(e, out float t) && Time.time < t) return false;
            _cooldown[e] = Time.time + 0.4f;
            return true;
        }
    }
}
