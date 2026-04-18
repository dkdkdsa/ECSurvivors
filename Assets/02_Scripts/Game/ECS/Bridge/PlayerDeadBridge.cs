using Game.ECS;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class PlayerDeadBridge : MonoBehaviour
    {
        public UnityEvent OnPlayerDead;

        private EntityManager _entityManager;
        private EntityQuery _query;

        private bool _initialized;
        private bool _hasSeenPlayer;
        private bool _triggered;

        private void Start()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!_initialized)
            {
                if (!TryInitialize())
                    return;
            }

            bool hasPlayer = !_query.IsEmptyIgnoreFilter;

            if (hasPlayer)
            {
                _hasSeenPlayer = true;
                return;
            }

            if (!_hasSeenPlayer)
                return;

            if (!_triggered)
            {
                _triggered = true;
                Time.timeScale = 0;
                OnPlayerDead?.Invoke();
            }
        }

        private bool TryInitialize()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return false;

            _entityManager = world.EntityManager;
            _query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>());
            _initialized = true;
            return true;
        }
    }
}