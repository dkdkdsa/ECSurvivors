using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.ENEMY_ANIMATION)]
    public class EnemyAnimationSystemGO : MonoBehaviour
    {
        private static EnemyAnimationSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static EnemyAnimationSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(EnemyAnimationSystemGO));
                    _instance = go.AddComponent<EnemyAnimationSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<EnemyGO> _enemies = new List<EnemyGO>(2048);
        private readonly Dictionary<EnemyGO, int> _indexOf = new Dictionary<EnemyGO, int>(2048);

        private TransformAccessArray _transforms;
        private NativeList<quaternion> _baseRotations;
        private bool _dirty;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;

            _transforms = new TransformAccessArray(2048);
            _baseRotations = new NativeList<quaternion>(2048, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_transforms.isCreated) _transforms.Dispose();
            if (_baseRotations.IsCreated) _baseRotations.Dispose();
            if (_instance == this) _instance = null;
        }

        public void Register(EnemyGO enemy)
        {
            _indexOf[enemy] = _enemies.Count;
            _enemies.Add(enemy);
            _dirty = true;
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
            _dirty = true;
        }

        private void Rebuild()
        {
            _transforms.SetTransforms(null);
            _baseRotations.Clear();
            for (int i = 0; i < _enemies.Count; i++)
            {
                _transforms.Add(_enemies[i].transform);
                var br = _enemies[i].cachedBaseRotation;
                _baseRotations.Add(br != null ? (quaternion)br.baseValue : quaternion.identity);
            }
            _dirty = false;
        }

        private void LateUpdate()
        {
            int count = _enemies.Count;
            if (count == 0) return;
            if (_dirty || _transforms.length != count) Rebuild();

            for (int i = 0; i < count; i++)
            {
                var br = _enemies[i].cachedBaseRotation;
                _baseRotations[i] = br != null ? (quaternion)br.baseValue : quaternion.identity;
            }

            new WaddleJob
            {
                baseRotations = _baseRotations.AsArray(),
                elapsed = (float)Time.timeAsDouble
            }.Schedule(_transforms).Complete();
        }

        [BurstCompile]
        private struct WaddleJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<quaternion> baseRotations;
            public float elapsed;

            private const float Amplitude = 0.25f;
            private const float Frequency = 6f;

            public void Execute(int index, TransformAccess transform)
            {
                float phase = index * 0.37f;
                float angle = math.sin((elapsed + phase) * Frequency) * Amplitude;
                transform.rotation = math.mul(baseRotations[index], quaternion.RotateZ(angle));
            }
        }
    }
}