using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.EXP_ATTRACT)]
    public class ExpAttractSystemGO : MonoBehaviour
    {
        private static ExpAttractSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static ExpAttractSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(ExpAttractSystemGO));
                    _instance = go.AddComponent<ExpAttractSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<ExpGO> _items = new List<ExpGO>(4096);

        public int ActiveCount => _items.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(ExpGO exp) => _items.Add(exp);
        public void Unregister(ExpGO exp) => _items.Remove(exp);

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;
            int count = _items.Count;
            if (count == 0) return;

            float2 playerPos = PlayerPositionHolderGO.Value.xy;
            float dt = Time.deltaTime;

            var positions = new NativeArray<float3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var inVel = new NativeArray<float3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var outVel = new NativeArray<float3>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < count; i++)
            {
                positions[i] = _items[i].transform.position;
                var rb = _items[i].cachedRigid;
                inVel[i] = rb != null ? (float3)rb.velocity : float3.zero;
            }

            new AttractJob
            {
                positions = positions,
                inVelocities = inVel,
                playerPos = playerPos,
                dt = dt,
                outVelocities = outVel
            }.Schedule(count, 64).Complete();

            for (int i = 0; i < count; i++)
            {
                var rb = _items[i].cachedRigid;
                if (rb != null) rb.velocity = outVel[i];
            }

            positions.Dispose();
            inVel.Dispose();
            outVel.Dispose();
        }

        [BurstCompile]
        private struct AttractJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> positions;
            [ReadOnly] public NativeArray<float3> inVelocities;
            [ReadOnly] public float2 playerPos;
            [ReadOnly] public float dt;
            [WriteOnly] public NativeArray<float3> outVelocities;

            private const float FollowStrength = 12f;
            private const float MaxSpeed = 25f;
            private const float SlowDownDistance = 1.5f;

            public void Execute(int index)
            {
                float2 pos = positions[index].xy;
                float2 toPlayer = playerPos - pos;
                float distSq = math.lengthsq(toPlayer);

                float3 vIn = inVelocities[index];

                if (distSq < 0.0001f)
                {
                    outVelocities[index] = new float3(0f, 0f, vIn.z);
                    return;
                }

                float dist = math.sqrt(distSq);
                float2 dir = toPlayer / dist;
                float2 vel2 = vIn.xy;

                float targetSpeed = MaxSpeed;
                if (dist < SlowDownDistance)
                {
                    float ratio = dist / SlowDownDistance;
                    targetSpeed *= math.saturate(ratio);
                }

                float2 desiredVel = dir * targetSpeed;
                float t = math.saturate(FollowStrength * dt);
                vel2 = math.lerp(vel2, desiredVel, t);

                outVelocities[index] = new float3(vel2, vIn.z);
            }
        }
    }
}