using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Game.ECS
{
    public class PlayerDeadBridge : MonoBehaviour
    {
        private EntityQuery _query;
        private bool _triggered;

        public UnityEvent OnPlayerDead;

        private void Awake()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _query = em.CreateEntityQuery(ComponentType.ReadOnly<PlayerTag>());
        }

        private void Update()
        {
            if (_query.IsEmpty && !_triggered)
            {
                _triggered = true;
                Time.timeScale = 0;
                OnPlayerDead?.Invoke();
            }
        }
    }
}