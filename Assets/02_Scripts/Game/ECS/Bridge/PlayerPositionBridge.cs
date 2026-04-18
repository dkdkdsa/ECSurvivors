using Game.ECS;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class PlayerPositionBridge : MonoBehaviour
    {
        public UnityEvent<Vector3> OnPlayerPositionChanged;

        private EntityManager _entityManager;
        private EntityQuery _query;

        private bool _initialized;
        private bool _hasValue;
        private float3 _oldPosition;

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

            if (_query.IsEmptyIgnoreFilter)
                return;

            PlayerPosition current = _query.GetSingleton<PlayerPosition>();

            if (!_hasValue)
            {
                _oldPosition = current.Value;
                _hasValue = true;
                return;
            }

            if (!_oldPosition.Equals(current.Value))
            {
                _oldPosition = current.Value;
                OnPlayerPositionChanged?.Invoke(current.Value);
            }
        }

        private bool TryInitialize()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return false;

            _entityManager = world.EntityManager;
            _query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerPosition>());
            _initialized = true;
            return true;
        }
    }
}