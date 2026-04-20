using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.RIGIDBODY)]
    public class RigidbodySystemGO : MonoBehaviour
    {
        public const float FixedDT = 1f / 60f;

        private static RigidbodySystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static RigidbodySystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(RigidbodySystemGO));
                    _instance = go.AddComponent<RigidbodySystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<RigidbodyGO> _bodies = new List<RigidbodyGO>(1024);
        private readonly Dictionary<RigidbodyGO, int> _indexOf = new Dictionary<RigidbodyGO, int>(1024);
        public IReadOnlyList<RigidbodyGO> Bodies => _bodies;

        private TransformAccessArray _transforms;
        private NativeList<float3> _velocities;
        private bool _dirty;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _transforms = new TransformAccessArray(1024);
            _velocities = new NativeList<float3>(1024, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_transforms.isCreated) _transforms.Dispose();
            if (_velocities.IsCreated) _velocities.Dispose();
            if (_instance == this) _instance = null;
        }

        public void Register(RigidbodyGO body)
        {
            _indexOf[body] = _bodies.Count;
            _bodies.Add(body);
            _dirty = true;
        }

        public void Unregister(RigidbodyGO body)
        {
            if (!_indexOf.TryGetValue(body, out int idx)) return;
            int last = _bodies.Count - 1;
            if (idx != last)
            {
                var tail = _bodies[last];
                _bodies[idx] = tail;
                _indexOf[tail] = idx;
            }
            _bodies.RemoveAt(last);
            _indexOf.Remove(body);
            _dirty = true;
        }

        private void RebuildArrays()
        {
            if (_transforms.isCreated) _transforms.Dispose();
            _transforms = new TransformAccessArray(_bodies.Count);

            _velocities.Clear();

            for (int i = 0; i < _bodies.Count; i++)
            {
                _transforms.Add(_bodies[i].transform);
                _velocities.Add(_bodies[i].velocity);
            }
            _dirty = false;
        }

        private void FixedUpdate()
        {
            if (_bodies.Count == 0) return;
            if (_dirty || _transforms.length != _bodies.Count) RebuildArrays();

            for (int i = 0; i < _bodies.Count; i++)
                _velocities[i] = _bodies[i].velocity;

            var job = new IntegrateJob
            {
                velocities = _velocities.AsArray(),
                dt = FixedDT
            };

            job.Schedule(_transforms).Complete();
        }

        [BurstCompile]
        private struct IntegrateJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<float3> velocities;
            public float dt;

            public void Execute(int index, TransformAccess transform)
            {
                float3 v = velocities[index];
                float3 p = transform.position;
                p += new float3(v.x, v.y, 0f) * dt;
                transform.position = p;
            }
        }
    }
}