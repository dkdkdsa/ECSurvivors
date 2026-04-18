using System;
using Game.ECS;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class PlayerInfoBridge : MonoBehaviour
    {
        public UnityEvent<PlayerInfo, PlayerInfo> OnPlayerInfoChanged;

        private EntityManager _entityManager;
        private EntityQuery _query;

        private bool _initialized;
        private bool _hasValue;
        private PlayerInfo _oldInfo;

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

            PlayerInfo currentInfo = _query.GetSingleton<PlayerInfo>();

            if (!_hasValue)
            {
                _oldInfo = currentInfo;
                _hasValue = true;
                return;
            }

            if (!_oldInfo.Equals(currentInfo))
            {
                PlayerInfo previous = _oldInfo;
                _oldInfo = currentInfo;
                OnPlayerInfoChanged?.Invoke(previous, currentInfo);
            }
        }

        private bool TryInitialize()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return false;

            _entityManager = world.EntityManager;
            _query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PlayerInfo>());
            _initialized = true;
            return true;
        }

    }
}