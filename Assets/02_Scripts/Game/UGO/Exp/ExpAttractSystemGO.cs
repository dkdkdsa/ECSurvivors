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
        private readonly Dictionary<ExpGO, int> _indexOf = new Dictionary<ExpGO, int>(4096);

        private NativeList<float3> _positions;
        private NativeList<float3> _inVel;
        private NativeList<float3> _outVel;

        public int ActiveCount => _items.Count;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _positions = new NativeList<float3>(4096, Allocator.Persistent);
            _inVel = new NativeList<float3>(4096, Allocator.Persistent);
            _outVel = new NativeList<float3>(4096, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_positions.IsCreated) _positions.Dispose();
            if (_inVel.IsCreated) _inVel.Dispose();
            if (_outVel.IsCreated) _outVel.Dispose();
            if (_instance == this) _instance = null;
        }

        public void Register(ExpGO exp)
        {
            _indexOf[exp] = _items.Count;
            _items.Add(exp);
        }

        public void Unregister(ExpGO exp)
        {
            if (!_indexOf.TryGetValue(exp, out int idx)) return;
            int last = _items.Count - 1;
            if (idx != last)
            {
                var tail = _items[last];
                _items[idx] = tail;
                _indexOf[tail] = idx;
            }
            _items.RemoveAt(last);
            _indexOf.Remove(exp);
        }

        private void Update()
        {
            if (!PlayerPositionHolderGO.HasValue) return;
            int count = _items.Count;
            if (count == 0) return;

            float2 playerPos = PlayerPositionHolderGO.Value.xy;
            float dt = Time.deltaTime;

            _positions.ResizeUninitialized(count);
            _inVel.ResizeUninitialized(count);
            _outVel.ResizeUninitialized(count);

            for (int i = 0; i < count; i++)
            {
                _positions[i] = _items[i].transform.position;
                var rb = _items[i].cachedRigid;
                _inVel[i] = rb != null ? (float3)rb.velocity : float3.zero;
            }

            new AttractJob
            {
                positions = _positions.AsArray(),
                inVelocities = _inVel.AsArray(),
                playerPos = playerPos,
                dt = dt,
                outVelocities = _outVel.AsArray()
            }.Schedule(count, 64).Complete();

            for (int i = 0; i < count; i++)
            {
                var rb = _items[i].cachedRigid;
                if (rb != null) rb.velocity = _outVel[i];
            }
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