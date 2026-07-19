using UnityEngine;

namespace Sirvival
{
    /// <summary>Attached to each orbiting pin; forwards trigger hits to its owner weapon.</summary>
    public class PinDamage : MonoBehaviour
    {
        [HideInInspector] public RollingPinWeapon owner;

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        private void TryHit(Collider2D other)
        {
            if (owner == null) return;
            var e = other.GetComponent<Enemy>();
            if (e != null && owner.CanHit(e)) e.TakeDamage(owner.Damage);
        }
    }
}
