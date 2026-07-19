using UnityEngine;

namespace Sirvival
{
    /// <summary>Dropped by dead enemies. Gets vacuumed toward the chef within pickupRange, grants XP on reach.</summary>
    public class XpGem : MonoBehaviour
    {
        private int _value;

        public static void Spawn(Vector3 pos, int value)
        {
            var go = new GameObject("XpGem");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SirvivalAssets.Circle();
            sr.color = new Color(0.3f, 0.9f, 0.4f);
            sr.sortingOrder = -10;
            go.transform.localScale = Vector3.one * 0.3f;
            go.AddComponent<XpGem>()._value = value;
        }

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.Player == null) return;
            Vector2 to = (Vector2)(rm.Player.position - transform.position);
            float dist = to.magnitude;
            if (dist <= 0.35f) { rm.AddXp(_value); Destroy(gameObject); return; }
            if (dist <= rm.Stats.pickupRange)
                transform.position += (Vector3)(to.normalized * 8f * Time.deltaTime);
        }
    }
}
