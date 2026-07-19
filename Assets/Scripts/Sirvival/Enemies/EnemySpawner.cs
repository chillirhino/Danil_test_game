using UnityEngine;

namespace Sirvival
{
    /// <summary>Spawns the enemy prefab just outside the camera view at a ramping rate.</summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Enemy[] enemyPrefabs;
        private float _next;

        private void Update()
        {
            var rm = RunManager.Instance;
            if (rm == null || rm.State != RunState.Playing || enemyPrefabs == null || enemyPrefabs.Length == 0) return;
            if (Time.time < _next) return;
            _next = Time.time + WaveMath.SpawnInterval(rm.Elapsed);
            Spawn(rm);
        }

        private void Spawn(RunManager rm)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 c = rm.Player != null ? rm.Player.position : Vector3.zero;
            float h = cam.orthographicSize + 1f;
            float w = h * cam.aspect;
            float a = Random.value * Mathf.PI * 2f;
            Vector3 pos = c + new Vector3(Mathf.Cos(a) * w, Mathf.Sin(a) * h, 0f);
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var e = Instantiate(prefab, pos, Quaternion.identity);
            e.Configure(WaveMath.EnemyHp(rm.Elapsed), WaveMath.EnemySpeed(rm.Elapsed));
        }
    }
}
