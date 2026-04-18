using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.ENEMY_SPAWN)]
    public class EnemySpawnSystemGO : MonoBehaviour
    {
        private static EnemySpawnSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static EnemySpawnSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(EnemySpawnSystemGO));
                    _instance = go.AddComponent<EnemySpawnSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<EnemySpawnerGO> _spawners = new List<EnemySpawnerGO>(4);
        private readonly Dictionary<GameObject, GameObjectPool<EnemyGO>> _pools = new Dictionary<GameObject, GameObjectPool<EnemyGO>>();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(EnemySpawnerGO spawner) => _spawners.Add(spawner);
        public void Unregister(EnemySpawnerGO spawner) => _spawners.Remove(spawner);

        public void Release(EnemyGO enemy)
        {
            if (enemy.sourcePrefab != null && _pools.TryGetValue(enemy.sourcePrefab, out var pool))
                pool.Release(enemy);
            else
                enemy.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;
            float3 playerPos = PlayerPositionHolderGO.Value;
            float dt = Time.deltaTime;

            for (int i = 0; i < _spawners.Count; i++)
            {
                var spawner = _spawners[i];

                spawner.timer -= dt;
                if (spawner.timer > 0f) continue;

                spawner.timer = spawner.tick;

                var pool = GetOrCreatePool(spawner.enemyPrefab);

                for (int j = 0; j < spawner.spawnPerTick; j++)
                {
                    float angle = spawner.random.NextFloat(0f, math.PI * 2f);
                    float3 offset = new float3(
                        math.cos(angle) * spawner.radius,
                        math.sin(angle) * spawner.radius,
                        0f);

                    var enemy = pool.Get(playerPos + offset, Quaternion.identity);
                    enemy.sourcePrefab = spawner.enemyPrefab;
                }
            }
        }

        private GameObjectPool<EnemyGO> GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool)) return pool;

            var prefabComp = prefab.GetComponent<EnemyGO>();
            if (prefabComp == null)
            {
                return null;
            }

            pool = new GameObjectPool<EnemyGO>(prefabComp, transform, 512);
            pool.Prewarm(128);
            _pools.Add(prefab, pool);
            return pool;
        }
    }
}
