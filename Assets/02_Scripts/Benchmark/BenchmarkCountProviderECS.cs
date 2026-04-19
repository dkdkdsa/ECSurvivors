using Game.ECS;
using Unity.Entities;
using UnityEngine;

namespace Game.Benchmark
{
    [DisallowMultipleComponent]
    public class BenchmarkCountProviderECS : MonoBehaviour, IBenchmarkCountProvider
    {
        private EntityManager _em;
        private EntityQuery _enemyQuery;
        private EntityQuery _bulletQuery;
        private EntityQuery _expQuery;
        private EntityQuery _playerQuery;
        private bool _ready;

        private bool TryInit()
        {
            if (_ready) return true;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return false;

            _em = world.EntityManager;
            _enemyQuery  = _em.CreateEntityQuery(typeof(EnemyTag));
            _bulletQuery = _em.CreateEntityQuery(typeof(BulletComponent));
            _expQuery    = _em.CreateEntityQuery(typeof(ExpTag));
            _playerQuery = _em.CreateEntityQuery(typeof(PlayerTag));
            _ready = true;
            return true;
        }

        public int GetEnemyCount()  => TryInit() ? _enemyQuery.CalculateEntityCount()  : 0;
        public int GetBulletCount() => TryInit() ? _bulletQuery.CalculateEntityCount() : 0;
        public int GetExpCount()    => TryInit() ? _expQuery.CalculateEntityCount()    : 0;
        public int GetPlayerCount() => TryInit() ? _playerQuery.CalculateEntityCount() : 0;
    }
}
