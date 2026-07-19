using UnityEngine;

namespace Sirvival
{
    /// <summary>A "customer": chases the chef, deals contact damage, has HP, drops XP on death.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float maxHp = 30f;
        [SerializeField] private float contactDamage = 8f;
        [SerializeField] private int xpValue = 1;
        [SerializeField] private float hpMult = 1f;     // per-type scaling (fat = tanky, biz = fragile)
        [SerializeField] private float speedMult = 1f;

        private float _hp;
        private Rigidbody2D _rb;

        public void Configure(float hp, float spd) { maxHp = hp * hpMult; speed = spd * speedMult; _hp = maxHp; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _hp = maxHp;
        }

        private void FixedUpdate()
        {
            var player = RunManager.Instance != null ? RunManager.Instance.Player : null;
            if (player == null) { _rb.linearVelocity = Vector2.zero; return; }
            Vector2 dir = ((Vector2)player.position - _rb.position).normalized;
            _rb.linearVelocity = dir * speed;
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0f) Die();
        }

        private void Die()
        {
            RunManager.Instance?.AddKill();
            XpGem.Spawn(transform.position, xpValue);
            Destroy(gameObject);
        }

        private void OnCollisionStay2D(Collision2D c)
        {
            var hp = c.collider.GetComponent<ChefHealth>();
            if (hp != null) hp.TakeDamage(contactDamage);
        }
    }
}
