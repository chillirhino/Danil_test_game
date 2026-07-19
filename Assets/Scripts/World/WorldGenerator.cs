using System.Collections.Generic;
using UnityEngine;

namespace PoK.World
{
    /// <summary>
    /// Endless, chunk-based world for the on-rails forest.
    /// Chunks of ground + path + scattered vegetation spawn ahead of the player
    /// and are recycled behind. Each chunk is seeded by its index, so the world
    /// is deterministic and seamless.
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Player")]
        public Transform player;

        [Header("Chunk layout")]
        public float chunkLength = 20f;
        public int chunksAhead = 4;
        public int chunksBehind = 1;
        public float groundWidth = 44f;
        public float pathWidth = 3.4f;

        [Header("Materials")]
        public Material groundMat;
        public Material pathMat;

        [Header("Prefabs")]
        public GameObject[] trees;
        public GameObject[] bushes;
        public GameObject[] rocks;
        public GameObject[] groundCover;

        [Header("Density (per chunk)")]
        public int treesPerChunk = 12;
        public int bushesPerChunk = 8;
        public int rocksPerChunk = 4;
        public int coverPerChunk = 45;

        readonly Dictionary<int, GameObject> _chunks = new Dictionary<int, GameObject>();
        int _lastCenter = int.MinValue;

        void Start()
        {
            if (player == null)
            {
                var p = GameObject.Find("Player");
                if (p != null) player = p.transform;
            }
            Refresh();
        }

        void Update()
        {
            if (player == null) return;
            int center = Mathf.FloorToInt(player.position.z / chunkLength);
            if (center != _lastCenter) Refresh();
        }

        void Refresh()
        {
            if (player == null) return;
            int center = Mathf.FloorToInt(player.position.z / chunkLength);
            _lastCenter = center;

            for (int i = center - chunksBehind; i <= center + chunksAhead; i++)
                if (!_chunks.ContainsKey(i)) _chunks[i] = BuildChunk(i);

            var remove = new List<int>();
            foreach (var kv in _chunks)
                if (kv.Key < center - chunksBehind || kv.Key > center + chunksAhead) remove.Add(kv.Key);
            foreach (var k in remove) { Destroy(_chunks[k]); _chunks.Remove(k); }
        }

        GameObject BuildChunk(int index)
        {
            var root = new GameObject("Chunk_" + index);
            root.transform.SetParent(transform, false);
            float z0 = index * chunkLength;
            float zc = z0 + chunkLength * 0.5f;

            MakeGround("Ground", groundMat, groundWidth, 0f, zc, true);
            MakeGround("Path", pathMat, pathWidth, 0.02f, zc, false);

            var rng = new System.Random(index * 99991 + 7);
            Scatter(root.transform, trees, treesPerChunk, z0, 6f, 2.8f, 20f, rng, 0.75f, 1.3f);
            Scatter(root.transform, bushes, bushesPerChunk, z0, 1.3f, 2.3f, 19f, rng, 0.7f, 1.4f);
            Scatter(root.transform, rocks, rocksPerChunk, z0, 0.7f, 2.3f, 18f, rng, 0.5f, 1.6f);
            Scatter(root.transform, groundCover, coverPerChunk, z0, 0.5f, 1.8f, 19f, rng, 0.6f, 1.5f);

            return root;

            void MakeGround(string name, Material mat, float width, float y, float cz, bool keepCollider)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
                g.name = name;
                g.transform.SetParent(root.transform, false);
                g.transform.localPosition = new Vector3(0f, y, cz);
                g.transform.localScale = new Vector3(width / 10f, 1f, chunkLength / 10f);
                if (mat != null) g.GetComponent<Renderer>().sharedMaterial = mat;
                if (!keepCollider) Destroy(g.GetComponent<Collider>());
            }
        }

        void Scatter(Transform parent, GameObject[] prefabs, int count, float z0,
            float targetH, float minAbsX, float maxX, System.Random rng, float sLo, float sHi)
        {
            if (prefabs == null || prefabs.Length == 0) return;
            for (int i = 0; i < count; i++)
            {
                var p = prefabs[rng.Next(prefabs.Length)];
                if (p == null) continue;
                float x = ((float)rng.NextDouble() * (maxX - minAbsX) + minAbsX) * (rng.Next(2) == 0 ? -1f : 1f);
                float z = z0 + (float)rng.NextDouble() * chunkLength;
                var go = Instantiate(p, new Vector3(x, 0f, z), Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f), parent);
                foreach (var col in go.GetComponentsInChildren<Collider>()) Destroy(col);
                float h = Height(go);
                float s = targetH / Mathf.Max(0.01f, h) * ((float)(rng.NextDouble() * (sHi - sLo) + sLo));
                go.transform.localScale = Vector3.one * s;
            }
        }

        static float Height(GameObject g)
        {
            bool has = false;
            Bounds b = new Bounds(g.transform.position, Vector3.zero);
            foreach (var r in g.GetComponentsInChildren<Renderer>())
            {
                if (!has) { b = r.bounds; has = true; }
                else b.Encapsulate(r.bounds);
            }
            return has ? b.size.y : 1f;
        }
    }
}
