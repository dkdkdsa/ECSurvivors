using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.ENEMY_MOVE)]
    public class EnemyMoveSystemGO : MonoBehaviour
    {
        private static EnemyMoveSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static EnemyMoveSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(EnemyMoveSystemGO));
                    _instance = go.AddComponent<EnemyMoveSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<EnemyGO> _enemies = new List<EnemyGO>(2048);
        private readonly Dictionary<EnemyGO, int> _indexOf = new Dictionary<EnemyGO, int>(2048);

        private NativeList<float3> _positions;
        private NativeList<float> _speeds;
        private NativeList<float3> _velocities;

        public int ActiveCount => _enemies.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _positions = new NativeList<float3>(2048, Allocator.Persistent);
            _speeds = new NativeList<float>(2048, Allocator.Persistent);
            _velocities = new NativeList<float3>(2048, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_positions.IsCreated) _positions.Dispose();
            if (_speeds.IsCreated) _speeds.Dispose();
            if (_velocities.IsCreated) _velocities.Dispose();
            if (_instance == this) _instance = null;
        }

        public void Register(EnemyGO enemy)
        {
            _indexOf[enemy] = _enemies.Count;
            _enemies.Add(enemy);
        }

        public void Unregister(EnemyGO enemy)
        {
            if (!_indexOf.TryGetValue(enemy, out int idx)) return;
            int last = _enemies.Count - 1;
            if (idx != last)
            {
                var tail = _enemies[last];
                _enemies[idx] = tail;
                _indexOf[tail] = idx;
            }
            _enemies.RemoveAt(last);
            _indexOf.Remove(enemy);
        }

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;
            int count = _enemies.Count;
            if (count == 0) return;

            float3 playerPos = PlayerPositionHolderGO.Value;

            _positions.ResizeUninitialized(count);
            _speeds.ResizeUninitialized(count);
            _velocities.ResizeUninitialized(count);

            for (int i = 0; i < count; i++)
            {
                _positions[i] = _enemies[i].transform.position;
                _speeds[i] = _enemies[i].cachedUnit != null ? _enemies[i].cachedUnit.moveSpeed : 0f;
            }

            new MoveJob
            {
                positions = _positions.AsArray(),
                speeds = _speeds.AsArray(),
                playerPos = playerPos,
                velocities = _velocities.AsArray()
            }.Schedule(count, 64).Complete();

            for (int i = 0; i < count; i++)
            {
                var rb = _enemies[i].cachedRigid;
                if (rb != null) rb.velocity = _velocities[i];
            }
        }

        [BurstCompile]
        private struct MoveJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public NativeArray<float> speeds;
            [ReadOnly] public float3 playerPos;
            [WriteOnly] public NativeArray<float3> velocities;

            public void Execute(int index)
            {
                float3 toPlayer = playerPos - positions[index];
                float lenSq = math.lengthsq(toPlayer);
                if (lenSq < 1e-6f)
                {
                    velocities[index] = float3.zero;
                    return;
                }
                float3 dir = toPlayer * math.rsqrt(lenSq);
                velocities[index] = dir * speeds[index];
            }
        }
    }
}