using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

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
            Debug.Log("플레이어 사망하심");
            Time.timeScale = 0;
            OnPlayerDead?.Invoke();
        }
    }
}
