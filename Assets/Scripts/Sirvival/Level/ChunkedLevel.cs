using System.Collections.Generic;
using UnityEngine;

namespace Sirvival
{
    /// <summary>
    /// Infinite, deterministic decor generator. Keeps a square of chunks around the
    /// target loaded; each chunk scatters a few random kitchen props (seeded by its
    /// (x,y) coordinates, so the same chunk always looks the same). Chunks that fall
    /// out of range are destroyed.
    /// </summary>
    public class ChunkedLevel : MonoBehaviour
    {
        [SerializeField] private Transform target;       // chef (falls back to Camera.main)
        [SerializeField] private Sprite[] propSprites;   // kitchen decor sprites
        [SerializeField] private float chunkSize = 7f;
        [SerializeField] private int radius = 3;
        [SerializeField] private int propsPerChunk = 3;

        private readonly Dictionary<Vector2Int, GameObject> _active = new Dictionary<Vector2Int, GameObject>();

        private void Awake()
        {
            if (target == null)
            {
                var chef = GameObject.Find("Chef");
                if (chef != null) target = chef.transform;
                else if (Camera.main != null) target = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null || propSprites == null || propSprites.Length == 0) return;
            int cx = Mathf.FloorToInt(target.position.x / chunkSize);
            int cy = Mathf.FloorToInt(target.position.y / chunkSize);

            var needed = new HashSet<Vector2Int>();
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                var key = new Vector2Int(cx + dx, cy + dy);
                needed.Add(key);
                if (!_active.ContainsKey(key)) _active[key] = BuildChunk(key);
            }

            var stale = new List<Vector2Int>();
            foreach (var kv in _active)
                if (!needed.Contains(kv.Key)) stale.Add(kv.Key);
            foreach (var k in stale)
            {
                if (_active[k] != null) Destroy(_active[k]);
                _active.Remove(k);
            }
        }

        private GameObject BuildChunk(Vector2Int key)
        {
            var chunk = new GameObject("Chunk_" + key.x + "_" + key.y);
            chunk.transform.SetParent(transform, false);

            var rng = new System.Random((key.x * 73856093) ^ (key.y * 19349663));
            for (int i = 0; i < propsPerChunk; i++)
            {
                float px = (key.x + (float)rng.NextDouble()) * chunkSize;
                float py = (key.y + (float)rng.NextDouble()) * chunkSize;
                var prop = new GameObject("prop");
                prop.transform.SetParent(chunk.transform, false);
                prop.transform.position = new Vector3(px, py, 0f);
                float sc = 0.55f + (float)rng.NextDouble() * 0.45f;
                prop.transform.localScale = Vector3.one * sc;

                var sr = prop.AddComponent<SpriteRenderer>();
                sr.sprite = propSprites[rng.Next(propSprites.Length)];
                sr.color = Color.white;
                sr.sortingOrder = -50; // above floor (-100), below chef/enemies
            }
            return chunk;
        }
    }
}
