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

        private NativeList<float3> _velocities;
        private NativeList<quaternion> _results;
        private NativeList<byte> _keep;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _velocities = new NativeList<float3>(1024, Allocator.Persistent);
            _results = new NativeList<quaternion>(1024, Allocator.Persistent);
            _keep = new NativeList<byte>(1024, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_velocities.IsCreated) _velocities.Dispose();
            if (_results.IsCreated) _results.Dispose();
            if (_keep.IsCreated) _keep.Dispose();
            if (_instance == this) _instance = null;
        }

        private void LateUpdate()
        {
            if (!RigidbodySystemGO.HasInstance) return;
            var bodies = RigidbodySystemGO.Instance.Bodies;
            int n = bodies.Count;
            if (n == 0) return;

            _velocities.ResizeUninitialized(n);
            _results.ResizeUninitialized(n);
            _keep.ResizeUninitialized(n);

            for (int i = 0; i < n; i++)
                _velocities[i] = bodies[i].velocity;

            new FlipJob
            {
                velocities = _velocities.AsArray(),
                results = _results.AsArray(),
                keep = _keep.AsArray()
            }.Schedule(n, 64).Complete();

            for (int i = 0; i < n; i++)
            {
                if (_keep[i] != 0) continue;
                var br = bodies[i].baseRotation;
                if (br != null) br.baseValue = _results[i];
            }
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