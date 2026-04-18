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

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(EnemyGO enemy) => _enemies.Add(enemy);
        public void Unregister(EnemyGO enemy) => _enemies.Remove(enemy);

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;
            int count = _enemies.Count;
            if (count == 0) return;

            float3 playerPos = PlayerPositionHolderGO.Value;

            var positions = new NativeArray<float3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var speeds    = new NativeArray<float> (count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var velocities = new NativeArray<float3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < count; i++)
            {
                positions[i] = _enemies[i].transform.position;
                speeds[i] = _enemies[i].cachedUnit != null ? _enemies[i].cachedUnit.moveSpeed : 0f;
            }

            new MoveJob
            {
                positions = positions,
                speeds = speeds,
                playerPos = playerPos,
                velocities = velocities
            }.Schedule(count, 64).Complete();

            for (int i = 0; i < count; i++)
            {
                var rb = _enemies[i].cachedRigid;
                if (rb != null) rb.velocity = velocities[i];
            }

            positions.Dispose();
            speeds.Dispose();
            velocities.Dispose();
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
