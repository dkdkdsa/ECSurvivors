using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.FLIP)]
    public class FlipSystemGO : MonoBehaviour
    {
        private static FlipSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static FlipSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(FlipSystemGO));
                    _instance = go.AddComponent<FlipSystemGO>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void LateUpdate()
        {
            if (!RigidbodySystemGO.HasInstance) return;
            var bodies = RigidbodySystemGO.Instance.Bodies;
            int n = bodies.Count;
            if (n == 0) return;

            var velocities = new NativeArray<float3>(n, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var results    = new NativeArray<quaternion>(n, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var keep       = new NativeArray<byte>(n, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < n; i++)
                velocities[i] = bodies[i].velocity;

            new FlipJob
            {
                velocities = velocities,
                results = results,
                keep = keep
            }.Schedule(n, 64).Complete();

            for (int i = 0; i < n; i++)
            {
                if (keep[i] != 0) continue;
                var br = bodies[i].baseRotation;
                if (br != null) br.baseValue = results[i];
            }

            velocities.Dispose();
            results.Dispose();
            keep.Dispose();
        }

        [BurstCompile]
        private struct FlipJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> velocities;
            [WriteOnly] public NativeArray<quaternion> results;
            [WriteOnly] public NativeArray<byte> keep;

            public void Execute(int index)
            {
                float vx = velocities[index].x;
                if (vx == 0f)
                {
                    keep[index] = 1;
                    results[index] = quaternion.identity;
                    return;
                }
                keep[index] = 0;
                results[index] = vx > 0f
                    ? quaternion.RotateY(math.PI)
                    : quaternion.identity;
            }
        }
    }
}
