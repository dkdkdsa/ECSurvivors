using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.DROP)]
    public class DropSystemGO : MonoBehaviour
    {
        private static DropSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static DropSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(DropSystemGO));
                    _instance = go.AddComponent<DropSystemGO>();
                }
                return _instance;
            }
        }

        // ECS 코드와 동일하게 고정 시드 사용
        private Random _random = new Random(123);

        private readonly Queue<DropEventGO> _events = new Queue<DropEventGO>(64);
        private readonly Dictionary<GameObject, GameObjectPool<ExpGO>> _pools = new Dictionary<GameObject, GameObjectPool<ExpGO>>();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void EnqueueDrop(DropEventGO evt)
        {
            _events.Enqueue(evt);
        }

        public void Release(ExpGO exp)
        {
            if (exp.sourcePrefab != null && _pools.TryGetValue(exp.sourcePrefab, out var pool))
                pool.Release(exp);
            else
                exp.gameObject.SetActive(false);
        }

        private void Update()
        {
            while (_events.Count > 0)
            {
                var evt = _events.Dequeue();
                var pool = GetOrCreatePool(evt.prefab);
                if (pool == null) continue;

                for (int i = 0; i < evt.dropCount; i++)
                {
                    var dir = _random.NextFloat3Direction();
                    var pos = (float3)evt.position + dir;
                    var inst = pool.Get(pos, Quaternion.identity);
                    inst.sourcePrefab = evt.prefab;
                }
            }
        }

        private GameObjectPool<ExpGO> GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool)) return pool;

            var prefabComp = prefab.GetComponent<ExpGO>();
            if (prefabComp == null)
            {
                Debug.LogError($"{prefab.name} 에 ExpGO 가 없습니다");
                return null;
            }

            pool = new GameObjectPool<ExpGO>(prefabComp, transform, 1024);
            pool.Prewarm(256);
            _pools.Add(prefab, pool);
            return pool;
        }
    }
}
