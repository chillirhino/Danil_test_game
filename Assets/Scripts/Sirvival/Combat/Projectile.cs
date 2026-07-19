using UnityEngine;

namespace Sirvival
{
    /// <summary>Straight-flying shot; damages the first enemy it overlaps, then dies. Trigger collider.</summary>
    public class Projectile : MonoBehaviour
    {
        private Vector2 _vel;
        private float _damage;
        private float _dieAt;

        public void Launch(Vector2 dir, float speed, float damage, float life)
        {
            _vel = dir.normalized * speed;
            _damage = damage;
            _dieAt = Time.time + life;
        }

        private void Update()
        {
            transform.position += (Vector3)(_vel * Time.deltaTime);
            if (Time.time >= _dieAt) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var e = other.GetComponent<Enemy>();
            if (e != null) { e.TakeDamage(_damage); Destroy(gameObject); }
        }
    }
}
