using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.UGO
{
    [DefaultExecutionOrder(SystemOrderGO.AUTO_ATTACK)]
    public class AutoAttackSystemGO : MonoBehaviour
    {
        private static AutoAttackSystemGO _instance;
        public static bool HasInstance => _instance != null;
        public static AutoAttackSystemGO Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(AutoAttackSystemGO));
                    _instance = go.AddComponent<AutoAttackSystemGO>();
                }
                return _instance;
            }
        }

        private readonly List<AutoAttackGO> _attackers = new List<AutoAttackGO>(8);
        private readonly Dictionary<GameObject, GameObjectPool<BulletGO>> _pools = new Dictionary<GameObject, GameObjectPool<BulletGO>>();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(AutoAttackGO attacker) => _attackers.Add(attacker);
        public void Unregister(AutoAttackGO attacker) => _attackers.Remove(attacker);
        public IReadOnlyList<AutoAttackGO> GetAttackersForEnforce() => _attackers;

        public void ReleaseBullet(BulletGO bullet)
        {
            if (bullet.sourcePrefab != null && _pools.TryGetValue(bullet.sourcePrefab, out var pool))
                pool.Release(bullet);
            else
                bullet.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            int attackerCount = _attackers.Count;
            if (attackerCount == 0) return;

            float dt = RigidbodySystemGO.FixedDT;
            for (int i = 0; i < attackerCount; i++)
            {
                if (_attackers[i].currentInterval > 0f)
                    _attackers[i].currentInterval -= dt;
            }

            if (!EnemyMoveSystemGO.HasInstance) return;
            var enemyList = GetEnemyTransforms();
            int enemyCount = enemyList.Count;
            if (enemyCount == 0) return;

            var enemyPositions = new NativeArray<float3>(enemyCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < enemyCount; i++)
                enemyPositions[i] = enemyList[i].position;

            var attackerPositions = new NativeArray<float3>(attackerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var radiiSq           = new NativeArray<float> (attackerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var resultIdx         = new NativeArray<int>   (attackerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var canFire           = new NativeArray<byte>  (attackerCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < attackerCount; i++)
            {
                attackerPositions[i] = _attackers[i].transform.position;
                radiiSq[i] = _attackers[i].radius * _attackers[i].radius;
                canFire[i] = (byte)(_attackers[i].currentInterval <= 0f ? 1 : 0);
            }

            new FindClosestJob
            {
                attackerPositions = attackerPositions,
                radiiSq = radiiSq,
                canFire = canFire,
                enemyPositions = enemyPositions,
                resultIdx = resultIdx
            }.Schedule(attackerCount, 4).Complete();

            for (int i = 0; i < attackerCount; i++)
            {
                int idx = resultIdx[i];
                if (idx < 0) continue;

                var attacker = _attackers[i];
                attacker.currentInterval = attacker.interval;

                float3 myPos = attackerPositions[i];
                float3 targetPos = enemyPositions[idx];
                float2 dir2 = math.normalize((targetPos - myPos).xy);

                var pool = GetOrCreatePool(attacker.prefab);
                if (pool == null) continue;

                var bullet = pool.Get(myPos, Quaternion.identity);
                bullet.sourcePrefab = attacker.prefab;
                bullet.dir = new Vector3(dir2.x, dir2.y, 0f);
                bullet.setup = attacker.setup;
                bullet.lifeTime = attacker.setup.lifeTime;
                bullet.transform.localScale = Vector3.one * attacker.setup.size;
            }

            attackerPositions.Dispose();
            radiiSq.Dispose();
            resultIdx.Dispose();
            canFire.Dispose();
            enemyPositions.Dispose();
        }

        private static readonly List<Transform> _enemyTransformCache = new List<Transform>(2048);

        private List<Transform> GetEnemyTransforms()
        {
            _enemyTransformCache.Clear();
            if (EnemyAttackSystemGO.HasInstance)
            {
                EnemyAttackSystemGO.Instance.CollectTransformsInto(_enemyTransformCache);
            }
            return _enemyTransformCache;
        }

        private GameObjectPool<BulletGO> GetOrCreatePool(GameObject prefab)
        {
            if (prefab == null) return null;
            if (_pools.TryGetValue(prefab, out var pool)) return pool;

            var prefabComp = prefab.GetComponent<BulletGO>();
            if (prefabComp == null)
            {
                Debug.LogError($"{prefab.name} 에 BulletGO 가 없습니다");
                return null;
            }

            pool = new GameObjectPool<BulletGO>(prefabComp, transform, 512);
            pool.Prewarm(128);
            _pools.Add(prefab, pool);
            return pool;
        }

        [BurstCompile]
        private struct FindClosestJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> attackerPositions;
            [ReadOnly] public NativeArray<float> radiiSq;
            [ReadOnly] public NativeArray<byte> canFire;
            [ReadOnly] public NativeArray<float3> enemyPositions;
            [WriteOnly] public NativeArray<int> resultIdx;

            public void Execute(int index)
            {
                if (canFire[index] == 0) { resultIdx[index] = -1; return; }

                float2 myPos = attackerPositions[index].xy;
                float closestDistSq = radiiSq[index];
                int closest = -1;

                for (int i = 0; i < enemyPositions.Length; i++)
                {
                    float2 diff = enemyPositions[i].xy - myPos;
                    float distSq = math.lengthsq(diff);
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closest = i;
                    }
                }

                resultIdx[index] = closest;
            }
        }
    }
}
